using Microsoft.AspNetCore.Http;

namespace Infrastucture.Interfaces;

public interface ITorrentManager
{
    public Task<bool> StartDownloadFromUri(string url);
    public Task<bool> StartDownloadFromFile(IFormFile path);
    public Task<bool> PauseDownload(int id);
    public Task<bool> ResumeDownload(int id);
    public Task<bool> CancelDownload(int id);
}
