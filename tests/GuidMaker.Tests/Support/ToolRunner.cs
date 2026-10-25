using System.CommandLine;
using GuidMaker.Models;
using Rw.Core.Interfaces;

namespace GuidMaker.Tests.Support;

internal sealed record RunResult(int ExitCode, string Output, string Error);

internal static class ToolRunner
{
    /// <summary>Runs CommandOptionHandler directly with hand-built option values.</summary>
    public static async Task<RunResult> RunHandlerAsync(GuidMakerOptionValues options, IGuidGenerator generator)
    {
        var command = new Command("handler-under-test");
        command.SetAction(parseResult => CommandOptionHandler.HandleCommandOptions(options, parseResult, generator));

        return await InvokeAsync(command, Array.Empty<string>());
    }

    /// <summary>Runs the real guid-maker command through the System.CommandLine parser.</summary>
    public static async Task<RunResult> RunToolAsync(IGuidGenerator generator, params string[] args)
    {
        var command = new GuidMakerTool(generator).CreateCommand();

        return await InvokeAsync(command, args);
    }

    /// <summary>
    /// Runs guid-maker as a subcommand of a root command, as Program.cs does. Needed for built-in
    /// options such as --help, which System.CommandLine only adds to the root command.
    /// </summary>
    public static async Task<RunResult> RunUnderRootAsync(IGuidGenerator generator, params string[] args)
    {
        var root = new RootCommand("test root");
        root.Add(new GuidMakerTool(generator).CreateCommand());

        return await InvokeAsync(root, ["guid-maker", .. args]);
    }

    private static async Task<RunResult> InvokeAsync(Command command, string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var configuration = new InvocationConfiguration { Output = output, Error = error };

        var exitCode = await command.Parse(args).InvokeAsync(configuration);

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }
}
