using System;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class LambdaDataBindingConverter<TInput, TOutput> : DataBindingConverter<TInput, TOutput>
	{
		private Func<TInput, TOutput> _func;
		
		public LambdaDataBindingConverter(Func<TInput, TOutput> func) => 
			_func = func;

		public override bool TryConvertExplicitly(TInput input, out TOutput output)
		{
			output = _func(input);
			return true;
		}

		public static implicit operator LambdaDataBindingConverter<TInput, TOutput>(Func<TInput, TOutput> func) =>
			new LambdaDataBindingConverter<TInput, TOutput>(func);
	}
}