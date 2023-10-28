using Domain.Context;
using Domain.Models;
using FFmpeg.Net.Enums;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MonoTorrent.Client;

namespace Application.Repositories;

public class TorrentRepository : ITorrentRepository
{
    private readonly MethflixContext _context;
    private readonly string _downloadFolder;
    private readonly string _torrentFolder;
    private readonly IFfmpegService _ffmpegService;
    private readonly IStorageService _storage;

    public TorrentRepository(MethflixContext context, IConfiguration configuration, IFfmpegService ffmpegService,
        IStorageService storageService)
    {
        _context = context;
        _downloadFolder = configuration["StorageManager:Internal"] ?? string.Empty;
        _torrentFolder = configuration["StorageManager:TorrentStorage"] ?? string.Empty;
        _ffmpegService = ffmpegService;
        _storage = storageService;
    }

    public async Task CreateTorrent(string name)
    {
        var torrent = _context.Dtorrents.FirstOrDefault(x => x.Name == name);
        if (torrent == null)
        {
            _context.Dtorrents.Add(
                new Dtorrent
                {
                    Name = name,
                    CreatedAt = DateTime.UtcNow.Ticks.ToString(),
                    CreatedBy = -1,
                    IsDownloaded = false,
                    IsSeeding = true,
                    Location = $"{_downloadFolder}/{name}",
                    IsDeleted = false,
                }
            );
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateTorrentDownloadComplete(TorrentManager  torrentManager, int category = 1)
    {
        var torrent = await _context.Dtorrents.FirstOrDefaultAsync(x => x.Name == torrentManager.Name);
        if (torrent == null)
        {
            var res = await _context.Dtorrents.AddAsync(
                new Dtorrent
                {
                    Name = torrentManager.Name,
                    CreatedAt = DateTime.UtcNow.Ticks.ToString(),
                    CreatedBy = 1,
                    IsDownloaded = true,
                    Location = $"{_torrentFolder}/{torrentManager.Name}.torrent",
                    IsSeeding = true,
                    RequestedBy = 1,
                }
            );
            torrent = res.Entity;
            await _context.SaveChangesAsync();
        }
        else
        {
            torrent.IsDownloaded = true;
            _context.Attach(torrent);
            _context.Update(torrent);
        }
        
        foreach (var torrentManagerFile in torrentManager.Files)
        {
            var filePath = torrentManagerFile.DownloadCompleteFullPath;
            await AddMovie(filePath, category, torrent);
        }
    }

    private async Task<bool> AddMovie(string filePath, int category, Dtorrent torrent)
    {
        var extension = _storage.GetFileExtension(filePath);
        if (!VideoFileFormats.Formats.Contains(extension.ToLower())) return false;
                
        // var result = await _ffmpegService.ConvertToBinary(name, VideoType.MP4, name);
        var lenght =  _ffmpegService.GetMovieLenght(filePath);

        var exists = await _context.Movies.FirstOrDefaultAsync(x => x.Name == torrent.Name);
        if (exists != null) return false;

        var thumbnail = await _ffmpegService.GenerateThumbnailAsync(filePath, TimeSpan.FromMinutes(10));
        _context.Movies.Add(
            new Movie
            {
                CategoryId = category,
                Torrent = torrent,
                Name = torrent.Name,
                Path = filePath,
                TimeData = lenght,
                TorrentId = torrent.Id,
                Thumbnail = Convert.ToBase64String(thumbnail)
            }
        );
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTorrent(int id)
    {
        var torrent = await _context.Dtorrents.FirstOrDefaultAsync(x => x.Id == id);
        if (torrent == null) return false;

        _context.Dtorrents.Remove(torrent);
        await _context.SaveChangesAsync();
        return true;
    }
}
