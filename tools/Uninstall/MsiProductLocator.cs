using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Uninstall;

public interface IMsiProductLocator
{
    /// <summary>Returns the braced product code of the installed rw MSI, or null if it is not installed.</summary>
    string? FindProductCode();
}

public static class MsiProductLocators
{
    public static IMsiProductLocator ForCurrentOs() =>
        OperatingSystem.IsWindows() ? new RegistryMsiProductLocator() : new NoMsiProductLocator();
}

public sealed class NoMsiProductLocator : IMsiProductLocator
{
    public string? FindProductCode() => null;
}

[SupportedOSPlatform("windows")]
public sealed class RegistryMsiProductLocator : IMsiProductLocator
{
    private const string UninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
    private const string ProductName = "rw";
    private const string Publisher = "Rhys Walker";

    public string? FindProductCode()
    {
        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        {
            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                var code = Search(hive, view);
                if (code is not null)
                {
                    return code;
                }
            }
        }

        return null;
    }

    private static string? Search(RegistryHive hive, RegistryView view)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, view);
        using var uninstall = baseKey.OpenSubKey(UninstallKeyPath);
        if (uninstall is null)
        {
            return null;
        }

        foreach (var name in uninstall.GetSubKeyNames())
        {
            using var entry = uninstall.OpenSubKey(name);
            if (entry is not null
                && name.StartsWith('{')
                && entry.GetValue("WindowsInstaller") is 1
                && entry.GetValue("DisplayName") as string == ProductName
                && entry.GetValue("Publisher") as string == Publisher)
            {
                return name;
            }
        }

        return null;
    }
}
