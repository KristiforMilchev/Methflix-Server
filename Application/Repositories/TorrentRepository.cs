using Domain.Context;
using Domain.Models;
using FFmpeg.Net.Enums;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MonoTorrent.Client;
using Npgsql;

namespace Application.Repositories;

public class TorrentRepository : BaseRepository, ITorrentRepository
{

    private readonly string _downloadFolder;
    private readonly string _torrentFolder;
    private readonly IFfmpegService _ffmpegService;
    private readonly IStorageService _storage;

    public TorrentRepository(NpgsqlConnection connection, IConfiguration configuration, IFfmpegService ffmpegService,
        IStorageService storageService) : base(connection)
    {
         _downloadFolder = configuration["StorageManager:Internal"] ?? string.Empty;
        _torrentFolder = configuration["StorageManager:TorrentStorage"] ?? string.Empty;
        _ffmpegService = ffmpegService;
        _storage = storageService;
    }

    public async Task CreateTorrent(string name)
    {
        const string sql = """
                           INSERT INTO DTorrents (Name, CreatedAt, CreatedBy, IsDownloaded, IsSeeding, Location, 
                           IsDeleted)
                           VALUES (@Name, @CreatedAt, @CreatedBy, @IsDownloaded, @IsSeeding, @Location, @IsDeleted)
                           """;

        var parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@Name", name),
            new NpgsqlParameter("@CreatedAt", DateTime.UtcNow.Ticks.ToString()),
            new NpgsqlParameter("@CreatedBy", -1),
            new NpgsqlParameter("@IsDownloaded", false),
            new NpgsqlParameter("@IsSeeding", true),
            new NpgsqlParameter("@Location", $"{_downloadFolder}/{name}"),
            new NpgsqlParameter("@IsDeleted", false),
        };

        await using var command = CreateCommand(sql, parameters);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateTorrentDownloadComplete(TorrentManager torrentManager, int category = 1)
    {
        var sql =
            "INSERT INTO \"DTorrents\" (Name, CreatedAt, CreatedBy, IsDownloaded, IsSeeding, RequestedBy) " +
            "VALUES (@Name, @CreatedAt, @CreatedBy, @IsDownloaded, @IsSeeding, @RequestedBy) " +
            "ON CONFLICT (Name) DO UPDATE " +
            "SET IsDownloaded = EXCLUDED.IsDownloaded";

        var parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@Name", torrentManager.Name),
            new NpgsqlParameter("@CreatedAt", DateTime.UtcNow.Ticks.ToString()),
            new NpgsqlParameter("@CreatedBy", 1),
            new NpgsqlParameter("@IsDownloaded", true),
            new NpgsqlParameter("@IsSeeding", true),
            new NpgsqlParameter("@RequestedBy", 1),
        };

        await using var command = CreateCommand(sql, parameters);
        await command.ExecuteNonQueryAsync();

        foreach (var torrentManagerFile in torrentManager.Files)
        {
            var filePath = torrentManagerFile.DownloadCompleteFullPath;
            await AddMovie(filePath, category, torrentManager.Name);
        }
    }

    private async Task<bool> AddMovie(string filePath, int category, string torrentName)
    {
        var extension = _storage.GetFileExtension(filePath);
        if (!VideoFileFormats.Formats.Contains(extension.ToLower())) return false;

        var length = _ffmpegService.GetMovieLenght(filePath);

        const string sql = """
                           INSERT INTO Movies (CategoryId, Name, Path, TimeData, TorrentId, Thumbnail) 
                           VALUES (@CategoryId, @Name, @Path, @TimeData, @TorrentId, @Thumbnail)
                           """;


        var thumbnail = Convert.ToBase64String(
            await _ffmpegService.GenerateThumbnailAsync(filePath, TimeSpan.FromMinutes(10))
        );
        await using var command = CreateCommand(sql, 
            new NpgsqlParameter("@CategoryId", category),
            new NpgsqlParameter("@Name", torrentName),
            new NpgsqlParameter("@Path", filePath),
            new NpgsqlParameter("@TimeData", length),
            new NpgsqlParameter("@TorrentId", await GetTorrentIdByName(torrentName)),
            new NpgsqlParameter("@Thumbnail", thumbnail),
            new NpgsqlParameter("@CategoryId", category)
        );
        await command.ExecuteNonQueryAsync();

        return true;
    }

    private async Task<int> GetTorrentIdByName(string name)
    {
        var sql = "SELECT Id FROM \"DTorrents\" WHERE Name = @Name";

        

        await using var command = CreateCommand(sql,new NpgsqlParameter("@Name", name));
        var result = await command.ExecuteScalarAsync();

        return result != null ? Convert.ToInt32(result) : -1;
    }

    public async Task<bool> DeleteTorrent(int id)
    {
        var sql = "DELETE FROM \"DTorrents\" WHERE Id = @Id";

        var parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@Id", id),
        };

        await using var command = CreateCommand(sql, parameters);
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
}
