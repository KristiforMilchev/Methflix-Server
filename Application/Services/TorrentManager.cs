using Infrastucture.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Connections.Peer;

namespace Application.Services;

public class TorrentManager : ITorrentManager
{
    private readonly string _downloadDirectory;

    public TorrentManager(IConfiguration configuration, )
    {
        _downloadDirectory = configuration["movies"] ?? string.Empty;
    }

    public async Task<bool> StartDownloadFromUri(string url)
    {
        var torrent = await Torrent.LoadAsync(new HttpClient(), new Uri(url), _downloadDirectory);

        var engineSettings = new EngineSettings();

        ;
        MonoTorrent.Client.TorrentManager torrentManager;
        engineSettings.MaximumUploadRate = 30;

        var torrentSettings = new TorrentSettings();

        torrentSettings.MaximumUploadRate = 10;

        torrentSettings.UploadSlots = 4;

        var engine = new ClientEngine(engineSettings, new Factories());

        await engine.AddAsync(MagnetLink.Parse(url), _downloadDirectory);
        await engine.UpdateSettingsAsync(engineSettings);

        torrentManager = engine.
            _torrentManager.+=new EventHandler<PeersAddedEventArgs>(PeersAdded);

        torrentManager.OnPieceHashed += new EventHandler<PieceHashedEventArgs>(PieceHashed);

        torrentManager.OnTorrentStateChanged += new EventHandler<TorrentStateChangedEventArgs>(torrentStateChanged);

        torrentManager.PieceManager.OnPieceChanged += new EventHandler<PieceEventArgs>(pieceStateChanged);

        ClientEngine.connectionManager.OnPeerConnectionChanged +=
            new EventHandler<PeerConnectionEventArgs>(peerConnectionChanged);

        ClientEngine.connectionManager.OnPeerMessages +=
            new EventHandler<PeerMessageEventArgs>(peerMessageSentOrRecieved);

        while (torrentManager.State != TorrentState.Stopped &&
               torrentManager.State != TorrentState.Paused)
        {
            Console.WriteLine(torrentManager.Progress());
            Thread.Sleep(1000);

            if (torrentManager.Progress() == 100.0)
            {
                WaitHandle[] handles = engine.Stop();
                WaitHandle.WaitAll(handles);
                return;
            }
        }
    }

    public Task<bool> PauseDownload(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ResumeDownload(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CancelDownload(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StartDownloadFromFile(IFormFile path)
    {
        throw new NotImplementedException();
    }
}
