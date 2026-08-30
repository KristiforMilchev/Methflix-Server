using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Domain.Dtos;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MonoTorrent;
using MonoTorrent.Client;

namespace Application.Services;

public class TorrentService : ITorrentService
{
    private static readonly Dictionary<string, string> StreamContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".mp4"] = "video/mp4",
        [".m4v"] = "video/mp4",
        [".mov"] = "video/quicktime",
        [".mkv"] = "video/x-matroska",
        [".webm"] = "video/webm",
        [".avi"] = "video/x-msvideo",
    };

    private readonly string _downloadDirectory;
    private readonly string _torrentPath;
    private readonly ITorrentNotifier _torrentNotifier;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _streamLocks = new();
    private bool IsRunning { get; set; }
    private List<TorrentManager> ActiveTorrents { get; set; }
    private ClientEngine Engine { get; }

    public TorrentService(IConfiguration configuration,  ITorrentNotifier notifier)
    {
        ActiveTorrents = new List<TorrentManager>();
        _downloadDirectory = configuration["StorageManager:Internal"] ?? string.Empty;
        _torrentPath = configuration["StorageManager:TorrentStorage"] ?? string.Empty;
        Engine = new ClientEngine();
        IsRunning = false;
        _torrentNotifier = notifier;
        LoadTorrentsFromFolder().ConfigureAwait(true).GetAwaiter().GetResult();
    }
    
    public async Task StartServer(CancellationToken token)
    {
        // If we loaded no torrents, just exist. The user can put files in the torrents directory and start
        // the client again
        if (Engine.Torrents.Count == 0)
        {
            Console.WriteLine($"No torrents found in '{_torrentPath}' or loaded in the system");
            Console.WriteLine("Exiting...");
            IsRunning = false;
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
            manager.TrackerManager.AnnounceComplete += (sender, args) =>  _torrentNotifier
                .OnTrackerManagerOnAnnounceComplete(sender, args, manager);

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
            IsRunning = true;
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

        IsRunning = false;
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
            var uri = new Uri(url);
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);
            var name = queryParameters["dn"];
            var announceUrls = queryParameters.GetValues("tr");
            var xt = queryParameters["xt"].Split(":");
            var infoHash = InfoHash.FromHex(xt.Last());
            var magnetLink = new MagnetLink(
                infoHash,
                name,
                announceUrls,
                webSeeds: null, // webSeeds and size are not provided in the magnet link
                size: null
            );
             var torrentManager = await Engine.AddStreamingAsync(magnetLink, _downloadDirectory);
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
            
            await Engine.Torrents.FirstOrDefault(x => x.Torrent?.Name == name)!.PauseAsync();
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
            await exists.StartAsync();
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
            if (File.Exists($"{_downloadDirectory}/{exists.Name}"))
            {
                File.Delete($"{_downloadDirectory}/{exists.Name}");
            }

            if (Directory.Exists($"{_downloadDirectory}/{exists.Name}"))
            {
                Directory.Delete($"{_downloadDirectory}/{exists.Name}",true);
            }
            
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
            var manager = await Engine.AddStreamingAsync(filePath, _downloadDirectory, settingsBuilder.ToSettings());
            await manager.StartAsync();
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

    public async Task<TorrentStreamResult?> OpenStreamAsync(string name, CancellationToken token)
    {
        var manager = Engine.Torrents.FirstOrDefault(x => x.Name == name);
        if (manager?.StreamProvider == null) return null;

        var file = manager.Files
            .Where(f => VideoFileFormats.Formats.Contains(Path.GetExtension(f.Path), StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(f => f.Length)
            .FirstOrDefault();
        if (file == null) return null;

        // MonoTorrent only allows one active stream per torrent at a time - the previous one
        // must be disposed before a new one is created. Serialize access per-torrent so that
        // e.g. seeking (which opens a new range request) doesn't throw on the old stream.
        var streamLock = _streamLocks.GetOrAdd(name, _ => new SemaphoreSlim(1, 1));
        await streamLock.WaitAsync(token);

        Stream stream;
        try
        {
            stream = await manager.StreamProvider.CreateStreamAsync(file, prebuffer: true, token);
        }
        catch
        {
            streamLock.Release();
            throw;
        }

        var extension = Path.GetExtension(file.Path);
        var contentType = StreamContentTypes.GetValueOrDefault(extension, "video/mp4");
        return new TorrentStreamResult(new ReleasingStream(stream, streamLock), Path.GetFileName(file.Path), contentType);
    }

    // Releases the per-torrent stream lock once the caller disposes the stream, so the
    // next request for the same torrent can open a new StreamProvider stream.
    private sealed class ReleasingStream : Stream
    {
        private readonly Stream _inner;
        private readonly SemaphoreSlim _lock;
        private bool _released;

        public ReleasingStream(Stream inner, SemaphoreSlim @lock)
        {
            _inner = inner;
            _lock = @lock;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _inner.ReadAsync(buffer, offset, count, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                Release();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await _inner.DisposeAsync();
            Release();
            await base.DisposeAsync();
        }

        private void Release()
        {
            if (_released) return;
            _released = true;
            _lock.Release();
        }
    }

    private async Task LoadTorrentsFromFolder()
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
                var manager = await Engine.AddStreamingAsync(file, _downloadDirectory, settingsBuilder.ToSettings());

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
