using System;
using System.Diagnostics;
using KAG.Shared.Utilities;
using UnityEngine;

namespace KAG.Unity.Common
{
	[Serializable]
	public abstract class TrackedProcess
	{
		public Process Value => _value;
	
		protected abstract string WorkingDirectory { get; }
		protected abstract string FileName { get; }
		protected virtual string Arguments => null;
		protected virtual bool UseShellExecute => true;
	
		[SerializeField]
		private bool _showWindow;
	
		private Process _value;

		public TrackedProcess() =>
			_value = ExternalCalls.CreateProcess(WorkingDirectory, FileName, _showWindow, Arguments, UseShellExecute);

		public virtual bool Start()
		{
			if (_value.Responding)
				return false;
		
			_value.Start();
			Application.quitting += OnApplicationQuit;
			
			return true;
		}

		public virtual bool Kill()
		{
			if (_value.HasExited)
				return false;
		
			_value.Kill();
			Application.quitting -= OnApplicationQuit;
			
			return true;
		}

		private void OnApplicationQuit() => 
			Kill();
	}
}