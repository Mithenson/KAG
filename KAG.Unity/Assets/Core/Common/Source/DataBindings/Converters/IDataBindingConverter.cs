using System;

namespace KAG.Unity.Common.DataBindings
{
	public interface IDataBindingConverter
	{
		Type OutputType { get; }
		bool TryConvert(object input, out object output);
	}

	public interface IDataBindingConverter<in TInput, TOutput> : IDataBindingConverter
	{
		bool TryConvert(TInput input, out TOutput output);
	}
}