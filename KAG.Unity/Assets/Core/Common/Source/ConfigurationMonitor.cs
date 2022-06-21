using System;
using UnityEngine;

namespace KAG.Unity.Common
{
	public abstract class ConfigurationMonitor
	{
		public abstract void Initialize(ScriptableObject configuration);
	}
	
	public sealed class ConfigurationMonitor<TConfiguration> : ConfigurationMonitor
		where TConfiguration : ScriptableObject
	{
		public event Action OnInitialized;
		
		public bool IsOperational { get; private set; }
		public TConfiguration Configuration { get; private set; }

		public override void Initialize(ScriptableObject configuration) => 
			Initialize((TConfiguration)configuration);
		public void Initialize(TConfiguration configuration)
		{
			Configuration = configuration;
			IsOperational = true;
			
			OnInitialized?.Invoke();
		}
	}
}