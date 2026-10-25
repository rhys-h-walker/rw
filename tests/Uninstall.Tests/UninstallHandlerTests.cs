using Rw.Core.Interfaces;

namespace Uninstall.Tests;

public sealed class UninstallHandlerTests : IDisposable
{
    private const string ProductCode = "{11111111-2222-3333-4444-555555555555}";

    private static readonly string[] AptRemoveArguments = ["apt-get", "remove", "-y", "rw"];

    private static readonly InstallEnvironment WindowsMsi =
        new(true, @"C:\Users\me\AppData\Local\Programs\rw\rw.exe", @"C:\Users\me", @"C:\Users\me\AppData\Local");

    private static readonly InstallEnvironment Deb =
        new(false, "/usr/bin/rw", "/home/me", "/home/me/.local/share");

    private static readonly InstallEnvironment LocalBinary =
        new(false, "/home/me/.local/bin/rw", "/home/me", "/home/me/.local/share");

    private readonly FakeProcessRunner _processes = new();
    private readonly FakeFileSystem _files = new();
    private readonly FakeMsiProductLocator _msi = new(ProductCode);
    private readonly StringWriter _output = new();
    private readonly StringWriter _error = new();

    public void Dispose()
    {
        _output.Dispose();
        _error.Dispose();
    }

    private UninstallHandler CreateHandler() => new(_processes, _files, _msi, _output, _error);

    [Fact]
    public async Task RunAsync_MsiWithoutConfirm_DoesNotStartAnything()
    {
        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(false, WindowsMsi), CancellationToken.None);

        Assert.Equal(0, exitCode);
        Assert.Empty(_processes.Detached);
        Assert.Contains("Dry run", _output.ToString());
        Assert.Contains(ProductCode, _output.ToString());
    }

    [Fact]
    public async Task RunAsync_MsiConfirmed_StartsDetachedMsiexecWithProductCode()
    {
        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(true, WindowsMsi), CancellationToken.None);

        Assert.Equal(0, exitCode);
        var started = Assert.Single(_processes.Detached);
        Assert.Equal("msiexec.exe", started.FileName);
        Assert.Equal(new[] { "/x", ProductCode, "/passive" }, started.Arguments);
    }

    [Fact]
    public async Task RunAsync_MsiProductNotFound_ReturnsErrorAndStartsNothing()
    {
        _msi.ProductCode = null;

        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(true, WindowsMsi), CancellationToken.None);

        Assert.Equal(1, exitCode);
        Assert.Empty(_processes.Detached);
        Assert.NotEmpty(_error.ToString());
    }

    [Fact]
    public async Task RunAsync_DebWithoutConfirm_DoesNotRunApt()
    {
        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(false, Deb), CancellationToken.None);

        Assert.Equal(0, exitCode);
        Assert.Empty(_processes.Runs);
    }

    [Fact]
    public async Task RunAsync_DebConfirmed_RunsSudoAptGetRemove()
    {
        _processes.ExitCode = 0;

        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(true, Deb), CancellationToken.None);

        Assert.Equal(0, exitCode);
        var run = Assert.Single(_processes.Runs);
        Assert.Equal("sudo", run.FileName);
        Assert.Equal(AptRemoveArguments, run.Arguments);
    }

    [Fact]
    public async Task RunAsync_DebConfirmedAptFails_ReturnsAptExitCode()
    {
        _processes.ExitCode = 100;

        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(true, Deb), CancellationToken.None);

        Assert.Equal(100, exitCode);
    }

    [Fact]
    public async Task RunAsync_LocalBinaryWithoutConfirm_KeepsFile()
    {
        _files.Existing.Add(LocalBinary.ExecutablePath);

        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(false, LocalBinary), CancellationToken.None);

        Assert.Equal(0, exitCode);
        Assert.Empty(_files.Deleted);
    }

    [Fact]
    public async Task RunAsync_LocalBinaryConfirmed_DeletesTheBinary()
    {
        _files.Existing.Add(LocalBinary.ExecutablePath);

        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(true, LocalBinary), CancellationToken.None);

        Assert.Equal(0, exitCode);
        Assert.Equal(new[] { LocalBinary.ExecutablePath }, _files.Deleted);
    }

    [Fact]
    public async Task RunAsync_UnknownLocation_ReturnsErrorEvenWhenConfirmed()
    {
        var devBuild = new InstallEnvironment(false, "/home/me/src/rw/dist/linux-x64/rw", "/home/me", "/home/me/.local/share");

        var exitCode = await CreateHandler().RunAsync(new UninstallRequest(true, devBuild), CancellationToken.None);

        Assert.Equal(1, exitCode);
        Assert.Empty(_processes.Runs);
        Assert.Empty(_processes.Detached);
        Assert.Empty(_files.Deleted);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public int ExitCode { get; set; }

        public List<(string FileName, IReadOnlyList<string> Arguments)> Runs { get; } = [];

        public List<(string FileName, IReadOnlyList<string> Arguments)> Detached { get; } = [];

        public Task<int> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
        {
            Runs.Add((fileName, arguments));
            return Task.FromResult(ExitCode);
        }

        public void StartDetached(string fileName, IReadOnlyList<string> arguments) => Detached.Add((fileName, arguments));
    }

    private sealed class FakeFileSystem : IFileSystem
    {
        public HashSet<string> Existing { get; } = [];

        public List<string> Deleted { get; } = [];

        public bool FileExists(string path) => Existing.Contains(path);

        public void DeleteFile(string path) => Deleted.Add(path);
    }

    private sealed class FakeMsiProductLocator(string? productCode) : IMsiProductLocator
    {
        public string? ProductCode { get; set; } = productCode;

        public string? FindProductCode() => ProductCode;
    }
}
