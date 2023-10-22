using System.Text;
using Domain.Dtos;
using Infrastucture.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MonoTorrent;
using MonoTorrent.Client;

namespace Application.Services;

public class TorrentService : ITorrentService
{
    private readonly string _downloadDirectory;
    private readonly string _torrentPath;
    private readonly ITorrentNotifier _torrentNotifier;
    private List<TorrentManager> ActiveTorrents { get; set; }
    private ClientEngine Engine { get; }
    public TorrentService(IConfiguration configuration, ClientEngine engine, ITorrentNotifier notifier)
    {
        ActiveTorrents = new List<TorrentManager>();
        _downloadDirectory = configuration["StorageManager:Internal"] ?? string.Empty;
        _torrentPath = configuration["StorageManager:TorrentStorage"] ?? string.Empty;
        Engine = engine;
        _torrentNotifier = notifier;
        LoadTorrentsFromFlder().ConfigureAwait(true).GetAwaiter().GetResult();
    }



     public async Task StartServer(CancellationToken token)
    {
        // If we loaded no torrents, just exist. The user can put files in the torrents directory and start
        // the client again
        if (Engine.Torrents.Count == 0)
        {
            Console.WriteLine($"No torrents found in '{_torrentPath}' or loaded in the system");
            Console.WriteLine("Exiting...");
            return;
        }

        // For each torrent manager we loaded and stored in our list, hook into the events
        // in the torrent manager and start the engine.
        foreach (var manager in Engine.Torrents)
        {
            manager.PeerConnected += _torrentNotifier.OnManagerOnPeerConnected;
            manager.ConnectionAttemptFailed += _torrentNotifier.OnManagerOnConnectionAttemptFailed;
            // Every time a piece is hashed, this is fired.
            manager.PieceHashed +=_torrentNotifier. OnManagerOnPieceHashed;

            // Every time the state changes (Stopped -> Seeding -> Downloading -> Hashing) this is fired
            manager.TorrentStateChanged += _torrentNotifier.OnManagerOnTorrentStateChanged;

            // Every time the tracker's state changes, this is fired
            manager.TrackerManager.AnnounceComplete += _torrentNotifier.OnTrackerManagerOnAnnounceComplete;

            // Start the torrentmanager. The file will then hash (if required) and begin downloading/seeding.
            // As EngineSettings.AutoSaveLoadDhtCache is enabled, any cached data will be loaded into the
            // Dht engine when the first torrent is started, enabling it to bootstrap more rapidly.
            await manager.StartAsync();
        }

        // While the torrents are still running, print out some stats to the screen.
        // Details for all the loaded torrent managers are shown.
        var sb = new StringBuilder(1024);
        while (Engine.IsRunning)
        {
            sb.Remove(0, sb.Length);

            _torrentNotifier.AppendFormat(
                sb,
                $"Transfer Rate:      {Engine.TotalDownloadRate / 1024.0:0.00}kB/sec ↓ / {
                    Engine.TotalUploadRate / 1024.0:0.00}kB/sec ↑"
            );
            _torrentNotifier.AppendFormat(
                sb,
                $"Memory Cache:       {Engine.DiskManager.CacheBytesUsed / 1024.0:0.00}/{
                    Engine.Settings.DiskCacheBytes / 1024.0:0.00} kB"
            );
            _torrentNotifier.AppendFormat(
                sb,
                $"Disk IO Rate:       {Engine.DiskManager.ReadRate / 1024.0:0.00} kB/s read / {
                    Engine.DiskManager.WriteRate / 1024.0:0.00} kB/s write"
            );
            _torrentNotifier.AppendFormat(
                sb,
                $"Disk IO Total:      {Engine.DiskManager.TotalBytesRead / 1024.0:0.00} kB read / {
                    Engine.DiskManager.TotalBytesWritten / 1024.0:0.00} kB written"
            );
            _torrentNotifier.AppendFormat(
                sb, $"Open Files:         {Engine.DiskManager.OpenFiles} / {Engine.DiskManager.MaximumOpenFiles}"
            );
            _torrentNotifier.AppendFormat(sb, $"Open Connections:   {Engine.ConnectionManager.OpenConnections}");

            // Print out the port mappings
            foreach (var mapping in Engine.PortMappings.Created)
            {
                _torrentNotifier.AppendFormat(
                    sb, $"Successful Mapping    {mapping.PublicPort}:{mapping.PrivatePort} ({mapping.Protocol})"
                );
            }

            foreach (var mapping in Engine.PortMappings.Failed)
            {
                _torrentNotifier.AppendFormat(
                    sb, $"Failed mapping:       {mapping.PublicPort}:{mapping.PrivatePort} ({mapping.Protocol})"
                );
            }

            foreach (var mapping in Engine.PortMappings.Pending)
            {
                _torrentNotifier.AppendFormat(
                    sb, $"Pending mapping:      {mapping.PublicPort}:{mapping.PrivatePort} ({mapping.Protocol})"
                );
            }

            foreach (var manager in Engine.Torrents)
            {
                _torrentNotifier.AppendSeparator(sb);
                _torrentNotifier.AppendFormat(sb, $"State:              {manager.State}");
                _torrentNotifier.AppendFormat(
                    sb, $"Name:               {(manager.Torrent == null ? "MetaDataMode" : manager.Torrent.Name)}"
                );
                _torrentNotifier.AppendFormat(sb, $"Progress:           {manager.Progress:0.00}");
                _torrentNotifier.AppendFormat(
                    sb,
                    $"Transferred:        {manager.Monitor.DataBytesReceived / 1024.0 / 1024.0:0.00} MB ↓ / {
                        manager.Monitor.DataBytesSent / 1024.0 / 1024.0:0.00} MB ↑"
                );
                _torrentNotifier.AppendFormat(sb, "Tracker Status");
                foreach (var tier in manager.TrackerManager.Tiers)
                {
                    _torrentNotifier.AppendFormat(
                        sb,
                        $"\t{tier.ActiveTracker} : Announce Succeeded: {tier.LastAnnounceSucceeded}. Scrape Succeeded: {
                            tier.LastScrapeSucceeded}."
                    );
                }

                _torrentNotifier.AppendFormat(sb, "Current Requests:   {0}", await manager.PieceManager.CurrentRequestCountAsync());

                var peers = await manager.GetPeersAsync();
                _torrentNotifier.AppendFormat(sb, "Outgoing:");
                foreach (var p in peers.Where(t => t.ConnectionDirection == Direction.Outgoing))
                {
                    _torrentNotifier.AppendFormat(
                        sb, "\t{2} - {1:0.00}/{3:0.00}kB/sec - {0} - {4} ({5})", p.Uri,
                        p.Monitor.DownloadRate / 1024.0,
                        p.AmRequestingPiecesCount,
                        p.Monitor.UploadRate / 1024.0,
                        p.EncryptionType,
                        string.Join("|", p.SupportedEncryptionTypes.Select(t => t.ToString()).ToArray())
                    );
                }

                _torrentNotifier.AppendFormat(sb, "");
                _torrentNotifier.AppendFormat(sb, "Incoming:");
                foreach (var p in peers.Where(t => t.ConnectionDirection == Direction.Incoming))
                {
                    _torrentNotifier.AppendFormat(
                        sb, "\t{2} - {1:0.00}/{3:0.00}kB/sec - {0} - {4} ({5})", p.Uri,
                        p.Monitor.DownloadRate / 1024.0,
                        p.AmRequestingPiecesCount,
                        p.Monitor.UploadRate / 1024.0,
                        p.EncryptionType,
                        string.Join("|", p.SupportedEncryptionTypes.Select(t => t.ToString()).ToArray())
                    );
                }

                _torrentNotifier.AppendFormat(sb, "", null);
                if (manager.Torrent == null) continue;
                foreach (var file in manager.Files)
                    _torrentNotifier.AppendFormat(sb, "{1:0.00}% - {0}", file.Path, file.BitField.PercentComplete);
            }

            Console.Clear();
            Console.WriteLine(sb.ToString());
            _torrentNotifier.ExportListener();

            await Task.Delay(5000, token);
        }
    }

 

    public async Task StopServer()
    {
        try
        {
            await Engine.StopAllAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public async Task<bool> StartDownloadFromUri(string url)
    {
        try
        {
            var magnet = new MagnetLink(InfoHash.FromHex(url));
            var torrentManager = await Engine.AddAsync(magnet, _downloadDirectory);
            await torrentManager.StartAsync();
            ActiveTorrents.Add(torrentManager);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> PauseDownload(string name)
    {
        try
        {
            var exists = Engine.Torrents.FirstOrDefault(x => x.Torrent?.Name == name);
            if (exists == null) return false;
            await exists.PauseAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> ResumeDownload(string name)
    {
        try
        {
            var exists = Engine.Torrents.FirstOrDefault(x => x.Torrent?.Name == name);
            if (exists == null) return false;
            await exists.SaveFastResumeAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> CancelDownload(string name)
    {
        try
        {
            var exists = Engine.Torrents.FirstOrDefault(x => x.Torrent?.Name == name);
            if (exists == null) return false;
            await exists.StopAsync();
            File.Delete($"{_downloadDirectory}/{exists.Name}");
            File.Delete($"{_torrentPath}/{exists.Name}");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> StartDownloadFromFile(IFormFile file)
    {
        try
        {
            var uniqueFileName = file.FileName;
            var filePath = Path.Combine(_torrentPath, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var settingsBuilder = new TorrentSettingsBuilder
            {
                MaximumConnections = 60
            };
            var manager = await Engine.AddAsync(filePath, _downloadDirectory, settingsBuilder.ToSettings());
            ActiveTorrents.Add(manager);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

   

    public ActiveTorrent? GetTorrentData(string name)
    {
        return Engine.Torrents.Select(
                torrentManager => new ActiveTorrent
                {
                    State = torrentManager.State, 
                    Name = torrentManager.Name,
                    Percentage = torrentManager.Progress,
                    IsInitialSeeding = torrentManager.IsInitialSeeding,
                    Seeds = torrentManager.Peers.Seeds,
                    Peers = torrentManager.Peers.Available ,
                    CurrentDownloadSpeed = Engine.TotalDownloadRate,
                    UploadSpeed =  Engine.TotalUploadRate,
                }
            ).
            FirstOrDefault(x=>x.Name == name);
    }

    public List<ActiveTorrent> GetAllTorrents()
    {
        return Engine.Torrents.Select(
            torrentManager => new ActiveTorrent
            {
                State = torrentManager.State,
                Name = torrentManager.Name,
                Percentage = torrentManager.Progress,
                IsInitialSeeding = torrentManager.IsInitialSeeding,
                Seeds = torrentManager.Peers.Seeds,
                Peers = torrentManager.Peers.Available,
                CurrentDownloadSpeed = Engine.TotalDownloadRate,
                UploadSpeed = Engine.TotalUploadRate,
            }
        ).ToList();

    }

    private async Task LoadTorrentsFromFlder()
    {
        // If the torrentsPath does not exist, we want to create it
        if (!Directory.Exists(_torrentPath))
            Directory.CreateDirectory(_torrentPath);

        // For each file in the torrents path that is a .torrent file, load it into the engine.
        foreach (var file in Directory.GetFiles(_torrentPath))
        {
            if (!file.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                var settingsBuilder = new TorrentSettingsBuilder
                {
                    MaximumConnections = 60
                };
                var manager = await Engine.AddAsync(file, _downloadDirectory, settingsBuilder.ToSettings());
                ActiveTorrents.Add(manager);
                Console.WriteLine(manager.InfoHashes.V1OrV2.ToHex());
            }
            catch (Exception e)
            {
                Console.Write("Couldn't decode {0}: ", file);
                Console.WriteLine(e.Message);
            }
        }
    }
}
