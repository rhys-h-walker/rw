using System.CommandLine;
using Rw.Core.Interfaces;

namespace Uninstall;

public sealed class UninstallTool(
    IProcessRunner processRunner,
    IFileSystem fileSystem,
    IMsiProductLocator msiProductLocator) : IRwTool
{
    public Command CreateCommand()
    {
        var confirm = new Option<bool>("--confirm", "--force", "-c")
        {
            Description = "Actually uninstall; without this the command only reports what it would do",
        };

        var command = new Command("uninstall", "Uninstall rw from this machine");
        command.Options.Add(confirm);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var environment = new InstallEnvironment(
                OperatingSystem.IsWindows(),
                Environment.ProcessPath ?? string.Empty,
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            var handler = new UninstallHandler(
                processRunner,
                fileSystem,
                msiProductLocator,
                parseResult.InvocationConfiguration.Output,
                parseResult.InvocationConfiguration.Error);

            return await handler.RunAsync(new UninstallRequest(parseResult.GetValue(confirm), environment), cancellationToken);
        });

        return command;
    }
}
