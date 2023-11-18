using Application;
using Domain.Context;
using Infrastructure.Repositories;
using MonoTorrent.Client;

namespace API;

public class Common :IDisposable
{
    private readonly ITorrentRepository _torrentRepository;
    public Common(ITorrentRepository torrentRepository)
    {
        Notifier.Subscribe("on_torrent_downloaded", Downloaded);
        _torrentRepository = torrentRepository;
    }

    void Downloaded(object manager)
    {
        if (manager is not TorrentManager torrent)
        {
            return;
        }
        
        if(torrent.Progress < 100) return;
        
        _torrentRepository.UpdateTorrentDownloadComplete(torrent);
        Notifier.Dispose(torrent.Name);
    }

    public void Dispose()
    {
        Notifier.Dispose("on_torrent_downloaded");
    }
}
