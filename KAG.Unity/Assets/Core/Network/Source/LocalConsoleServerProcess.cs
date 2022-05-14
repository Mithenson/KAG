using System;
using KAG.Unity.Common;

namespace KAG.Unity.Network
{
	[Serializable]
	public sealed class LocalConsoleServerProcess : TrackedProcess
	{
		protected override string WorkingDirectory => @"..\KAG.DarkRift";
		protected override string FileName => "DarkRift.Server.Console.exe";
	}
}