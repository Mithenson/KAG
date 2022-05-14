using System;
using KAG.Shared.Utilities;
using KAG.Unity.Common;

namespace KAG.Unity.Network
{
	[Serializable]
	public sealed class LocalConsoleServerViaDockerProcess : TrackedProcess
	{
		private const string PlayfabRelativePath = @"..\KAG.Playfab";
	
		protected override string WorkingDirectory => PlayfabRelativePath;
		protected override string FileName => "LocalMultiplayerAgent.exe";

		public override bool Kill()
		{
			if (!base.Kill())
				return false;

			var shutdownProcess = ExternalCalls.CreateProcess(
				PlayfabRelativePath, 
				"powershell.exe", 
				false, 
				".\\ShutdownLocalMultiplayerAgent.ps1");

			shutdownProcess.Start();
			return true;
		}
	}
}