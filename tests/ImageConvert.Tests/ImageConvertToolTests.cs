using ImageConvert.Tests.Support;
using ImageMagick;
using Rw.Core.StaticValues;

namespace ImageConvert.Tests;

public class ImageConvertToolTests : IDisposable
{
    private readonly TempFolder _folder = new();

    private static readonly string[] WildcardOutputs = ["one.jpg", "two.jpg"];
    private static readonly string[] FolderOutputs = ["a.png", "b.png"];

    private string OutputFolder => _folder.Combine("output");

    public void Dispose()
    {
        _folder.Dispose();
        GC.SuppressFinalize(this);
    }

    // ---- Command definition -----------------------------------------------------------

    [Fact]
    public void CreateCommand_Always_IsNamedImageConvertWithADescription()
    {
        var command = new ImageConvertTool().CreateCommand();

        Assert.Equal("image-convert", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    // ---- Conversions that work today ----------------------------------------------------

    [Fact]
    public async Task Invoke_PngToJpg_WritesAJpegNamedAfterTheSource()
    {
        var source = _folder.CreateImage("sample.png");

        var result = await ToolRunner.RunAsync("-s", source, "-f", "jpg", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        var converted = Path.Combine(OutputFolder, "sample.jpg");
        Assert.True(File.Exists(converted));
        Assert.Equal(MagickFormat.Jpeg, new MagickImageInfo(converted).Format);
    }

    [Fact]
    public async Task Invoke_SameFormat_WritesACopyWithTheSameName()
    {
        var source = _folder.CreateImage("sample.png");

        var result = await ToolRunner.RunAsync("-s", source, "-f", "png", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.True(File.Exists(Path.Combine(OutputFolder, "sample.png")));
    }

    [Fact]
    public async Task Invoke_OutputFileAlreadyExists_KeepsTheExistingFileAndWritesANumberedCopy()
    {
        var source = _folder.CreateImage("sample.png");
        Directory.CreateDirectory(OutputFolder);
        var existing = Path.Combine(OutputFolder, "sample.png");
        File.WriteAllText(existing, "keep me");

        var result = await ToolRunner.RunAsync("-s", source, "-f", "png", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal("keep me", File.ReadAllText(existing));
        Assert.True(File.Exists(Path.Combine(OutputFolder, "sample(0).png")));
    }

    [Fact]
    public async Task Invoke_CommaSeparatedSources_ConvertsEachOne()
    {
        var first = _folder.CreateImage("first.png");
        var second = _folder.CreateImage("second.png");

        var result = await ToolRunner.RunAsync("-s", $"{first},{second}", "-f", "jpg", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.True(File.Exists(Path.Combine(OutputFolder, "first.jpg")));
        Assert.True(File.Exists(Path.Combine(OutputFolder, "second.jpg")));
    }

    [Fact]
    public async Task Invoke_SpaceSeparatedSources_ConvertsEachOne()
    {
        var first = _folder.CreateImage("first.png");
        var second = _folder.CreateImage("second.png");

        var result = await ToolRunner.RunAsync("-s", first, second, "-f", "jpg", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.True(File.Exists(Path.Combine(OutputFolder, "first.jpg")));
        Assert.True(File.Exists(Path.Combine(OutputFolder, "second.jpg")));
    }

    [Fact]
    public async Task Invoke_WildcardInput_ConvertsEveryMatchingFile()
    {
        _folder.CreateImage("photos/one.png");
        _folder.CreateImage("photos/two.png");
        _folder.CreateImage("photos/three.jpg", MagickFormat.Jpeg);
        var pattern = _folder.Combine("photos", "*.png");

        var result = await ToolRunner.RunAsync("-s", pattern, "-f", "jpg", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(WildcardOutputs, FileNames(OutputFolder));
    }

    [Fact]
    public async Task Invoke_FolderInput_ConvertsOnlySupportedImages()
    {
        _folder.CreateImage("photos/a.png");
        _folder.CreateImage("photos/b.jpg", MagickFormat.Jpeg);
        File.WriteAllText(_folder.Combine("photos", "notes.txt"), "not an image");

        var result = await ToolRunner.RunAsync("-s", _folder.Combine("photos"), "-f", "png", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(FolderOutputs, FileNames(OutputFolder));
    }

    [Fact]
    public async Task Invoke_OneCorruptFile_ConvertsTheOthersAndExitsWithFailure()
    {
        var good = _folder.CreateImage("good.png");
        var corrupt = _folder.Combine("corrupt.png");
        File.WriteAllText(corrupt, "this is not a png");

        var result = await ToolRunner.RunAsync("-s", good, corrupt, "-f", "jpg", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Failure, result.ExitCode);
        Assert.True(File.Exists(Path.Combine(OutputFolder, "good.jpg")));
        Assert.Contains("corrupt.png", result.Error);
    }

    // ---- Behaviour the spec asks for that is not implemented yet ------------------------
    // These are expected to fail until the known gaps are fixed.

    [Fact]
    public async Task Invoke_MissingInput_ReportsAnErrorAndDoesNotFail()
    {
        var missing = _folder.Combine("missing.png");

        var result = await ToolRunner.RunAsync("-s", missing, "-f", "jpg", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Contains("missing.png", result.Error);
    }

    [Fact]
    public async Task Invoke_ExplicitUnsupportedFile_ReportsAnErrorAndDoesNotFail()
    {
        var notes = _folder.Combine("notes.txt");
        File.WriteAllText(notes, "not an image");

        var result = await ToolRunner.RunAsync("-s", notes, "-f", "jpg", "-o", OutputFolder);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Contains("notes.txt", result.Error);
    }

    [Fact]
    public async Task Invoke_NoImagesFound_DoesNotCreateTheOutputFolder()
    {
        var empty = _folder.Combine("empty");
        Directory.CreateDirectory(empty);

        await ToolRunner.RunAsync("-s", empty, "-f", "png", "-o", OutputFolder);

        Assert.False(Directory.Exists(OutputFolder));
    }

    [Fact]
    public async Task Invoke_SupportedFlagWithoutFormat_ListsTheSupportedFormats()
    {
        var result = await ToolRunner.RunAsync("--supported");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Contains("SUPPORTED OPTIONS", result.Output);
    }

    [Fact]
    public async Task Invoke_UnknownFormatName_ExitsWithInvalidInput()
    {
        var source = _folder.CreateImage("sample.png");

        var result = await ToolRunner.RunAsync("-s", source, "-f", "nope", "-o", OutputFolder);

        Assert.Equal(ExitCodes.InvalidInput, result.ExitCode);
    }

    [Fact]
    public async Task Invoke_OutputPathCannotBeCreated_ReturnsAFailureWithoutThrowing()
    {
        var source = _folder.CreateImage("sample.png");
        var badOutput = _folder.Combine("bad" + Path.GetInvalidFileNameChars()[0] + "dir");

        var result = await ToolRunner.RunAsync("-s", source, "-f", "jpg", "-o", badOutput);

        Assert.Equal(ExitCodes.Failure, result.ExitCode);
        Assert.DoesNotContain("Unhandled exception", result.Error);
        Assert.DoesNotContain("Unhandled exception", result.Output);
    }

    private static string[] FileNames(string folder) =>
        Directory.GetFiles(folder).Select(file => Path.GetFileName(file)).Order().ToArray();
}
