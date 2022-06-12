using System;
using Autofac;

namespace KAG.Server.DependencyInjection
{
	public abstract class Installer : IInstaller
	{
		protected readonly ContainerBuilder Builder;
		
		protected Installer(ContainerBuilder builder) => 
			Builder = builder;

		public abstract void Install();
	}
	
	public abstract class Installer<TImp> : Installer
		where TImp : Installer
	{
		protected Installer(ContainerBuilder builder) 
			: base(builder) { }
		
		public static void Install(ContainerBuilder builder)
		{
			var implementation = (TImp)Activator.CreateInstance(typeof(TImp), builder);
			implementation.Install();
		}
	}
	
	public abstract class Installer<TImp, TParam1> : Installer
		where TImp : Installer
	{
		protected Installer(ContainerBuilder builder) 
			: base(builder) { }
		
		public static void Install(ContainerBuilder builder, TParam1 param1)
		{
			var implementation = (TImp)Activator.CreateInstance(typeof(TImp), builder, param1);
			implementation.Install();
		}
	}
}