using System.CommandLine;
using ImageConvert;

namespace ImageConvert.Tests.Support;

internal sealed record RunResult(int ExitCode, string Output, string Error);

internal static class ToolRunner
{
    /// <summary>Runs the real image-convert command through the System.CommandLine parser.</summary>
    public static async Task<RunResult> RunAsync(params string[] args)
    {
        var command = new ImageConvertTool().CreateCommand();

        using var output = new StringWriter();
        using var error = new StringWriter();
        var configuration = new InvocationConfiguration { Output = output, Error = error };

        var exitCode = await command.Parse(args).InvokeAsync(configuration);

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }
}
