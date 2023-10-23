using Domain.Dtos;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces;

public interface ITorrentService
{
    
    public Task StartServer(CancellationToken token);
    public Task StopServer();
    public Task<bool> StartDownloadFromUri(string url);
    public Task<bool> StartDownloadFromFile(IFormFile path);
    public Task<bool> PauseDownload(string name);
    public Task<bool> ResumeDownload(string name);
    public Task<bool> CancelDownload(string name);
    public ActiveTorrent? GetTorrentData(string name);
    public List<ActiveTorrent> GetAllTorrents();
}
