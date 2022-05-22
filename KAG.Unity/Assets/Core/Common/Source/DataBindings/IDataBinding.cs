using System;

namespace KAG.Unity.Common.DataBindings
{
	public interface IDataBinding : IDisposable
	{
		bool IsActive { get; }
		void SetActive(bool value);
	}
}