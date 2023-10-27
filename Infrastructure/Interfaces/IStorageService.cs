namespace Infrastructure.Interfaces;

public interface IStorageService
{
    public string GetFilePath(string movie);
    public string GetFileName(string movie);
    public string GetFileExtension(string movie);
    string LoadBytes(string filePath);
}
