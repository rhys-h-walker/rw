using Rw.Core.Interfaces;

namespace Uninstall;

public sealed record UninstallRequest(bool Confirmed, InstallEnvironment Environment);

public sealed class UninstallHandler(
    IProcessRunner processRunner,
    IFileSystem fileSystem,
    IMsiProductLocator msiProductLocator,
    TextWriter output,
    TextWriter error)
{
    public async Task<int> RunAsync(UninstallRequest request, CancellationToken cancellationToken)
    {
        return InstallDetector.Detect(request.Environment) switch
        {
            InstallKind.WindowsMsi => UninstallMsi(request),
            InstallKind.DebPackage => await UninstallDebAsync(request, cancellationToken),
            InstallKind.LocalBinary => UninstallLocalBinary(request),
            _ => ReportUnknown(request),
        };
    }

    private int UninstallMsi(UninstallRequest request)
    {
        var productCode = msiProductLocator.FindProductCode();
        if (productCode is null)
        {
            error.WriteLine("rw is running from the MSI install folder, but its entry was not found in Installed apps.");
            return 1;
        }

        string[] arguments = ["/x", productCode, "/passive"];
        output.WriteLine($"Installed with the Windows MSI ({productCode}).");

        if (!request.Confirmed)
        {
            return DryRun($"msiexec {string.Join(' ', arguments)}");
        }

        // rw.exe is locked while running, so the uninstaller must outlive this process.
        output.WriteLine("Starting the Windows uninstaller. rw will exit now and the uninstall finishes in its own window.");
        processRunner.StartDetached("msiexec.exe", arguments);
        return 0;
    }

    private async Task<int> UninstallDebAsync(UninstallRequest request, CancellationToken cancellationToken)
    {
        string[] arguments = ["apt-get", "remove", "-y", "rw"];
        output.WriteLine("Installed with the .deb package.");

        if (!request.Confirmed)
        {
            return DryRun($"sudo {string.Join(' ', arguments)}");
        }

        output.WriteLine("Running apt through sudo. You may be asked for your password.");
        return await processRunner.RunAsync("sudo", arguments, cancellationToken);
    }

    private int UninstallLocalBinary(UninstallRequest request)
    {
        var path = request.Environment.ExecutablePath;
        output.WriteLine($"Installed with the tar.gz installer ({path}).");

        if (!request.Confirmed)
        {
            return DryRun($"delete {path}");
        }

        if (fileSystem.FileExists(path))
        {
            fileSystem.DeleteFile(path);
        }

        output.WriteLine("Removed rw.");
        return 0;
    }

    private int ReportUnknown(UninstallRequest request)
    {
        error.WriteLine($"rw is running from {request.Environment.ExecutablePath}, which is not a location an installer uses, so there is nothing to uninstall.");
        return 1;
    }

    private int DryRun(string action)
    {
        output.WriteLine($"Dry run: would run: {action}");
        output.WriteLine("Run again with --confirm to uninstall.");
        return 0;
    }
}
