using System;

namespace KAG.Unity.Common.DataBindings
{
	public abstract class DataBindingConverter<TInput, TOutput> : IDataBindingConverter<TInput, TOutput>
	{
		public Type OutputType => typeof(TOutput);

		public object Convert(object value)
		{
			if (value is TInput input)
				return Convert(input);

			return value;
		}
		public abstract TOutput Convert(TInput input);
	}
}