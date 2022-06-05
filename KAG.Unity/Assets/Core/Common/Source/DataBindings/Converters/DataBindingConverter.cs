using System;

namespace KAG.Unity.Common.DataBindings
{
	public abstract class DataBindingConverter<TInput, TOutput> : IDataBindingConverter<TInput, TOutput>
	{
		public Type OutputType => typeof(TOutput);

		bool IDataBindingConverter.TryConvert(object input, out object output) => TryConvertImplicitly(input, out output);
		bool IDataBindingConverter<TInput, TOutput>.TryConvert(TInput input, out TOutput output) => TryConvertExplicitly(input, out output);
		
		public bool TryConvertImplicitly(object rawInput, out object rawOutput)
		{
			if (rawInput is TInput input)
			{
				if (TryConvertExplicitly(input, out var output))
				{
					rawOutput = output;
					return true;
				}
				else
				{
					rawOutput = default;
					return false;
				}
			}

			rawOutput = default;
			return false;
		}
		public abstract bool TryConvertExplicitly(TInput input, out TOutput output);
	}
}