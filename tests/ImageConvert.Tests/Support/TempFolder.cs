using ImageMagick;

namespace ImageConvert.Tests.Support;

/// <summary>A fresh folder for each test, with helpers to create small real images in it.</summary>
internal sealed class TempFolder : IDisposable
{
    public TempFolder()
    {
        Root = Path.Combine(Path.GetTempPath(), "image-convert-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    public string Combine(params string[] parts) => Path.Combine([Root, .. parts]);

    /// <summary>Writes a 4x4 red image to the given path, relative to the root, and returns the full path.</summary>
    public string CreateImage(string relativePath, MagickFormat format = MagickFormat.Png)
    {
        var fullPath = Combine(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? Root);

        using var image = new MagickImage(MagickColors.Red, 4, 4);
        image.Write(fullPath, format);

        return fullPath;
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}
