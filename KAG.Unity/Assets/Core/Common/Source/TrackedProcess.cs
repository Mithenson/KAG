using System;
using System.Diagnostics;
using KAG.Shared.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KAG.Unity.Common
{
	[Serializable]
	public abstract class TrackedProcess
	{
		public bool IsRunning 
		{
			get
			{
				try
				{
					if (!_value.HasExited)
						return true;
				}
				catch
				{
					return false;
				}

				return false;
			}
		}
		public Process Value => _value;
	
		protected abstract string WorkingDirectory { get; }
		protected abstract string FileName { get; }
		protected virtual string Arguments => null;
		protected virtual bool UseShellExecute => true;
	
		[SerializeField]
		private bool _showWindow;
	
		private Process _value;
		
		public virtual bool Start()
		{
			_value ??= ExternalCalls.CreateProcess(WorkingDirectory, FileName, _showWindow, Arguments, UseShellExecute);

			if (IsRunning)
				return false;
			
			_value.Start();
			Application.quitting += OnApplicationQuit;
			
			return true;
		}

		public virtual bool Kill()
		{
			if (!IsRunning)
				return false;
		
			_value.Kill();
			Application.quitting -= OnApplicationQuit;
			
			return true;
		}

		private void OnApplicationQuit() => 
			Kill();
	}
}