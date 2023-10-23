using FFmpeg.Net.Enums;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces;

public interface IFfmpegService
{
    public string GetChunk(int start, int end, string file);
    public Task<string> ConvertTo(IFormFile data,VideoType type, string outputFile = "output_video.mp4");
    public TimeSpan GetMovieLenght(string file);
}
