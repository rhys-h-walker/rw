using Rw.Core.Interfaces;

namespace Rw.Core.IO;

public sealed class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public void DeleteFile(string path) => File.Delete(path);
}
