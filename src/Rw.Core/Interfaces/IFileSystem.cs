namespace Rw.Core.Interfaces;

public interface IFileSystem
{
    bool FileExists(string path);

    void DeleteFile(string path);
}
