using System.Diagnostics;
using Rw.Core.Interfaces;

namespace Rw.Core.Processes;

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<int> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        using var process = Process.Start(CreateStartInfo(fileName, arguments))
            ?? throw new InvalidOperationException($"Could not start {fileName}.");
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    public void StartDetached(string fileName, IReadOnlyList<string> arguments)
    {
        using var process = Process.Start(CreateStartInfo(fileName, arguments));
    }

    private static ProcessStartInfo CreateStartInfo(string fileName, IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo(fileName) { UseShellExecute = false };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        return startInfo;
    }
}
