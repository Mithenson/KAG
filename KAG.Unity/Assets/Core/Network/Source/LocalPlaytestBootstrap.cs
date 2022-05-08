using System.Diagnostics;
using DarkRift.Client.Unity;
using UnityEngine;

public class LocalPlaytestBootstrap : MonoBehaviour
{
	private const string PlayfabRelativePath = @"..\KAG.Playfab";

	[SerializeField]
	private bool _runInContainer;

	[SerializeField]
	private ushort _containerPort;

	[SerializeField]
	private ushort _directPort;

	[SerializeField]
	private bool _showWindow;
	
	private Process _process;

	private void Awake()
	{
		var client = GetComponent<UnityClient>();
		client.Port = _runInContainer ? _containerPort : _directPort;
		
		if (!Application.isEditor)
			return;

		_process = _runInContainer 
			? StartProcess(PlayfabRelativePath, "LocalMultiplayerAgent.exe") 
			: StartProcess(@"..\KAG.DarkRift", "DarkRift.Server.Console.exe");
	}

	private void OnApplicationQuit()
	{
		if (!Application.isEditor)
			return;
		
		if (_process != null && !_process.HasExited)
			_process.Kill();

		if (_runInContainer)
			StartProcess(PlayfabRelativePath, "powershell.exe", ".\\ShutdownLocalMultiplayerAgent.ps1");
	}

	private Process StartProcess(string workingDirectory, string fileName, string arguments = null, bool useShellExecute = true)
	{
		var process = new Process
		{
			StartInfo =
			{
				WorkingDirectory = workingDirectory,
				FileName = fileName,
				UseShellExecute = useShellExecute
			}
		};

		if (!string.IsNullOrEmpty(arguments))
			process.StartInfo.Arguments = arguments;

		if (!_showWindow)
		{
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.CreateNoWindow = true;
		}

		process.Start();
		return process;
	}
}