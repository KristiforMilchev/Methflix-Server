using Domain.Models;
using FFmpeg.Net.Enums;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Repositories;

public class TorrentRepository : ITorrentRepository
{
    private readonly MethflixContext _context;
    private readonly string _downloadFolder;
    private readonly IFfmpegService _ffmpegService;
    private readonly IStorageService _storage;

    public TorrentRepository(MethflixContext context, IConfiguration configuration, IFfmpegService ffmpegService,
        IStorageService storageService)
    {
        _context = context;
        _downloadFolder = configuration["StorageManager:Internal"] ?? string.Empty;
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
                    CreatedAt = DateTime.UtcNow,
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

    public async Task UpdateTorrentDownloadComplete(string name, int category = 1)
    {
        var torrent = await _context.Dtorrents.FirstOrDefaultAsync(x => x.Name == name);
        if (torrent == null) return; 
        
        torrent.IsDownloaded = true;
        _context.Attach(torrent);
        _context.Update(torrent);
        
        var extension = _storage.GetFileExtension(name);
        var result = await _ffmpegService.ConvertToBinary(name, VideoType.MP4, name);
        var lenght =  _ffmpegService.GetMovieLenght($"{_downloadFolder}/{result}");
        _context.Movies.Add(
            new Movie
            {
                CategoryId = category,
                Torrent = torrent,
                Name = name,
                Path = $"{_downloadFolder}/{result}",
                TimeData = lenght,
                TorrentId = torrent.Id 
            }
        );
        
        await _context.SaveChangesAsync();
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
