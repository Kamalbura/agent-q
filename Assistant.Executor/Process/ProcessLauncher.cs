using System.Diagnostics;
using Assistant.Executor.Abstractions;

namespace Assistant.Executor.Process;

public sealed class ProcessLauncher : IProcessLauncher
{
    public void Launch(string target)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        };

        System.Diagnostics.Process.Start(startInfo);
    }
}
