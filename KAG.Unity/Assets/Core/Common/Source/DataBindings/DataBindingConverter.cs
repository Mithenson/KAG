using System;

namespace KAG.Unity.Common.DataBindings
{
	public abstract class DataBindingConverter<TInput, TOutput> : IDataBindingConverter<TInput, TOutput>
	{
		public Type OutputType => typeof(TOutput);

		object IDataBindingConverter.Convert(object value) => ConvertImplicitly(value);
		TOutput IDataBindingConverter<TInput, TOutput>.Convert(TInput input) => ConvertExplicitly(input);
		
		public object ConvertImplicitly(object value)
		{
			if (value is TInput input)
				return ConvertExplicitly(input);

			return value;
		}
		public abstract TOutput ConvertExplicitly(TInput input);
	}
}