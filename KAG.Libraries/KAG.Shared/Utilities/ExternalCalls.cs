using System.Diagnostics;

namespace KAG.Shared.Utilities
{
	public static class ExternalCalls
	{
		public static Process CreateProcess(string workingDirectory, string fileName, bool showWindow, string arguments = null, bool useShellExecute = true)
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

			if (!showWindow)
			{
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.CreateNoWindow = true;
			}

			return process;
		}
	}
}