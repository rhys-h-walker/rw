using ImageConvert.Models;
using ImageConvert.Tests.Support;

namespace ImageConvert.Tests;

public class ImageToProcessTests : IDisposable
{
    private readonly TempFolder _folder = new();

    public void Dispose()
    {
        _folder.Dispose();
        GC.SuppressFinalize(this);
    }

    private ImageToProcess CreateModel() => new()
    {
        SourcePath = Path.Combine("photos", "a.HEIC"),
        TargetPath = _folder.Root,
        OutputType = "png",
    };

    [Fact]
    public void FinalOutputPath_NoFileWithTheSameName_UsesTheSourceNameWithTheNewExtension()
    {
        var model = CreateModel();

        Assert.Equal(Path.Combine(_folder.Root, "a.png"), model.FinalOutputPath);
    }

    [Fact]
    public void FinalOutputPath_FileWithTheSameNameExists_AppendsANumber()
    {
        File.WriteAllText(Path.Combine(_folder.Root, "a.png"), "taken");
        var model = CreateModel();

        Assert.Equal(Path.Combine(_folder.Root, "a(0).png"), model.FinalOutputPath);
    }

    [Fact]
    public void FinalOutputPath_NumberedNameAlsoExists_SkipsToTheNextNumber()
    {
        File.WriteAllText(Path.Combine(_folder.Root, "a.png"), "taken");
        File.WriteAllText(Path.Combine(_folder.Root, "a(0).png"), "taken");
        var model = CreateModel();

        Assert.Equal(Path.Combine(_folder.Root, "a(1).png"), model.FinalOutputPath);
    }
}
