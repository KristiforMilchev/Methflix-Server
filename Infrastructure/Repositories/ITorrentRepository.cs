namespace Infrastructure.Repositories;

public interface ITorrentRepository
{
    public Task CreateTorrent(string name);
    public Task UpdateTorrentDownloadComplete(string name, int category = 1);
    public Task<bool> DeleteTorrent(int id);
}
