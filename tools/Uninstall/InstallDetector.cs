namespace Uninstall;

public enum InstallKind
{
    Unknown,
    WindowsMsi,
    DebPackage,
    LocalBinary,
}

public sealed record InstallEnvironment(
    bool IsWindows,
    string ExecutablePath,
    string HomeDirectory,
    string LocalAppDataDirectory);

public static class InstallDetector
{
    public static InstallKind Detect(InstallEnvironment environment)
    {
        var executable = Normalize(environment.ExecutablePath);

        if (environment.IsWindows)
        {
            var msiPath = Normalize(environment.LocalAppDataDirectory) + "/Programs/rw/rw.exe";
            return string.Equals(executable, msiPath, StringComparison.OrdinalIgnoreCase)
                ? InstallKind.WindowsMsi
                : InstallKind.Unknown;
        }

        if (executable == "/usr/bin/rw")
        {
            return InstallKind.DebPackage;
        }

        return executable == Normalize(environment.HomeDirectory) + "/.local/bin/rw"
            ? InstallKind.LocalBinary
            : InstallKind.Unknown;
    }

    private static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');
}
