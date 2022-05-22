namespace KAG.Unity.Common.DataBindings
{
	public interface IDataBindingConverter
	{
		object Convert(object value);
	}

	public interface IDataBindingConverter<in TInput, out TOutput> : IDataBindingConverter
	{
		TOutput Convert(TInput input);
	}
}