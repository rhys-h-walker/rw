namespace Uninstall.Tests;

public class InstallDetectorTests
{
    [Fact]
    public void Detect_WindowsExeInLocalAppDataPrograms_ReturnsWindowsMsi()
    {
        var environment = new InstallEnvironment(true, @"C:\Users\me\AppData\Local\Programs\rw\rw.exe", @"C:\Users\me", @"C:\Users\me\AppData\Local");

        Assert.Equal(InstallKind.WindowsMsi, InstallDetector.Detect(environment));
    }

    [Fact]
    public void Detect_WindowsPathDiffersOnlyByCase_ReturnsWindowsMsi()
    {
        var environment = new InstallEnvironment(true, @"c:\users\ME\appdata\local\programs\RW\RW.EXE", @"C:\Users\me", @"C:\Users\me\AppData\Local");

        Assert.Equal(InstallKind.WindowsMsi, InstallDetector.Detect(environment));
    }

    [Fact]
    public void Detect_WindowsExeInBuildOutput_ReturnsUnknown()
    {
        var environment = new InstallEnvironment(true, @"C:\src\rw\dist\win-x64\rw.exe", @"C:\Users\me", @"C:\Users\me\AppData\Local");

        Assert.Equal(InstallKind.Unknown, InstallDetector.Detect(environment));
    }

    [Fact]
    public void Detect_LinuxUsrBin_ReturnsDebPackage()
    {
        var environment = new InstallEnvironment(false, "/usr/bin/rw", "/home/me", "/home/me/.local/share");

        Assert.Equal(InstallKind.DebPackage, InstallDetector.Detect(environment));
    }

    [Fact]
    public void Detect_LinuxHomeLocalBin_ReturnsLocalBinary()
    {
        var environment = new InstallEnvironment(false, "/home/me/.local/bin/rw", "/home/me", "/home/me/.local/share");

        Assert.Equal(InstallKind.LocalBinary, InstallDetector.Detect(environment));
    }

    [Fact]
    public void Detect_LinuxOtherLocation_ReturnsUnknown()
    {
        var environment = new InstallEnvironment(false, "/home/me/src/rw/dist/linux-x64/rw", "/home/me", "/home/me/.local/share");

        Assert.Equal(InstallKind.Unknown, InstallDetector.Detect(environment));
    }
}
