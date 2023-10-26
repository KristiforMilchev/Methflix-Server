using Domain.Context;
using Infrastructure.Repositories;
using MonoTorrent.Client;

namespace API;

public class Common :IDisposable
{
    private readonly ITorrentRepository _torrentRepository;
    private readonly MethflixContext _context;
    public Common(ITorrentRepository torrentRepository, MethflixContext context)
    {
        Notifier.Subscribe("on_torrent_downloaded", Downloaded);
        _torrentRepository = torrentRepository;
        _context = context;
    }

    void Downloaded(object manager)
    {
        if (manager is not TorrentManager torrent)
        {
            return;
        }
        
        _torrentRepository.UpdateTorrentDownloadComplete(torrent.Name);
        Notifier.Dispose(torrent.Name);
    }

    public void Dispose()
    {
        Notifier.Dispose("on_torrent_downloaded");
    }
}
