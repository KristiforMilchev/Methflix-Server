using MonoTorrent.Client;

namespace Infrastructure.Repositories;

public interface ITorrentRepository
{
    public Task CreateTorrent(string name);
    public Task UpdateTorrentDownloadComplete(TorrentManager torrentManager, int category = 1);
    public Task<bool> DeleteTorrent(int id);
}
