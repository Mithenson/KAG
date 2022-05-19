using System;

namespace KAG.Unity.Common.DataBindings
{
	public interface IDataBindingSource
	{ 
		event Action<object> OnSourceChanged;
	}
}