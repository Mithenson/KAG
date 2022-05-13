using System;

namespace KAG.Shared
{
	public interface IComponentPool
	{
		Component Acquire(Type componentType);
		TComponent Acquire<TComponent>() where TComponent : Component;

		void Return(Component component);
	}
}