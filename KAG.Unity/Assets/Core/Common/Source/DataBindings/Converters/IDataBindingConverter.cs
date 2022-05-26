using System;

namespace KAG.Unity.Common.DataBindings
{
	public interface IDataBindingConverter
	{
		Type OutputType { get; }
		object Convert(object value);
	}

	public interface IDataBindingConverter<in TInput, out TOutput> : IDataBindingConverter
	{
		TOutput Convert(TInput input);
	}
}