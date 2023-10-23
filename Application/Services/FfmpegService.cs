using System.Diagnostics;
using FFmpeg.Net;
using FFmpeg.Net.Data;
using FFmpeg.Net.Enums;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class FfmpegService : IFfmpegService
{
    private readonly IStorageService _storage;
    private readonly string _moviesFolder;
    public FfmpegService(IStorageService storageService, IConfiguration configuration)
    {
        _storage = storageService;
        _moviesFolder = configuration["StorageManager:Internal"] ?? string.Empty;
    }
    
    public string GetChunk(int start, int end, string file)
    {
        var movie = _storage.GetFileName(file);
        // Validate and adjust the startTime and duration as needed.
        if (start < 0) start = 0;
        if (end <= 0) end = 10; // Default duration in seconds.

        var outputSegmentPath = $"segment_{file}_{start}_{end}.mp4";
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-ss {start} -i {movie} -t {end} -c copy {outputSegmentPath}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        return file;
    }

    public async Task<string> ConvertTo(IFormFile data, VideoType type, string outputFile = "output_video.mp4")
    {
        if (data.Length == 0) return string.Empty;
     
        var uniqueFileName = _storage.GetFileName(data.FileName);

        await using (var stream = new FileStream(uniqueFileName, FileMode.Create))
        {
            await data.CopyToAsync(stream);
        }

        var conversion = new FFmpegClient(
            new FFmpegClientOptions(
                "/home/kristifor/software",
                true
            )
        );
        await conversion.ConvertAsync(new MediaFile(uniqueFileName, false), type, _moviesFolder);
        return $"{_moviesFolder}/{uniqueFileName}{type.ToString()}";
    }

    public TimeSpan GetMovieLenght(string file)
    {
        var path = _storage.GetFilePath(file);
        if (!System.IO.File.Exists(path))
        {
            return TimeSpan.Zero;
        }
        
        using var process = new Process();
        process.StartInfo.FileName = "ffprobe"; // Assumes ffprobe is in your system's PATH.
        process.StartInfo.Arguments =
            $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        var durationStr = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        var videoDuration = default(TimeSpan);

        if (!double.TryParse(durationStr, out var durationSeconds)) return videoDuration;
        videoDuration = TimeSpan.FromSeconds(durationSeconds);
        Console.WriteLine($"Duration of the video: {videoDuration}");

        return videoDuration;
    }
}
