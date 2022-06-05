using System;

namespace KAG.Unity.Common.DataBindings
{
	public interface IValueDataBindingSource
	{
		event Action<object> OnSourceChanged;
		object Value { get; }
	}
}