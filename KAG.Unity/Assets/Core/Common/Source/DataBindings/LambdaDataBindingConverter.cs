using System;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class LambdaDataBindingConverter<TInput, TOutput> : IDataBindingConverter<TInput, TOutput>
	{
		private Func<TInput, TOutput> _func;
		
		public LambdaDataBindingConverter(Func<TInput, TOutput> func) => 
			_func = func;
		
		public object Convert(object value)
		{
			if (value is TInput input)
				return Convert(input);

			return value;
		}
		public TOutput Convert(TInput input) => 
			_func(input);

		public static implicit operator LambdaDataBindingConverter<TInput, TOutput>(Func<TInput, TOutput> func) =>
			new LambdaDataBindingConverter<TInput, TOutput>(func);
	}
}