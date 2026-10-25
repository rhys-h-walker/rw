namespace Rw.Core.Interfaces;

public interface IProcessRunner
{
    /// <summary>Runs a program attached to this console (so prompts such as sudo work) and returns its exit code.</summary>
    Task<int> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken);

    /// <summary>Starts a program and returns immediately, so it can outlive this process.</summary>
    void StartDetached(string fileName, IReadOnlyList<string> arguments);
}
