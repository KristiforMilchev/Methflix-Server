using System.Diagnostics;
using Domain.Dtos;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("/api/v1/[controller]")]
[ApiController]
public class VideoController : ControllerBase
{
    private readonly ICdnService _cdnService;
    private readonly IFfmpegService _ffmpegService;
    private readonly IMovieRepository _movieRepository;
    private readonly string _segmentFolder;
    private readonly IStorageService _storage;

    public VideoController(IConfiguration configuration, IFfmpegService ffmpegService, IStorageService storageService,
        IMovieRepository movieRepository, ICdnService cdnService)
    {
        _segmentFolder = configuration["StorageManager:StreamSegments"] ?? string.Empty;
        _ffmpegService = ffmpegService;
        _storage = storageService;
        _movieRepository = movieRepository;
        _cdnService = cdnService;
    }

    [HttpGet]
    [Route("stream/{video}")]
    public async Task<IActionResult> StreamVideo(int video)
    {
        var cdnPath = await _cdnService.PathExists(video);
        var path = cdnPath.Path;

        if (!System.IO.File.Exists(path)) return NotFound();

        var ffmpegPath = "ffmpeg"; // Path to the FFmpeg executable
        var arguments = $"-i \"{path}\" -f mp4 -movflags frag_keyframe+empty_moov pipe:1";

        Response.Headers.Add("Content-Disposition", $"inline; filename={cdnPath.Name}");
        Response.Headers.Add("Content-Type", "video/mp4");

        var processStartInfo = new ProcessStartInfo(ffmpegPath)
        {
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };

        var process = new Process { StartInfo = processStartInfo };

        Response.OnStarting(
            async () =>
            {
                process.Start();
                var outputStream = process.StandardOutput.BaseStream;

                await using (outputStream)
                {
                    await outputStream.CopyToAsync(Response.Body);
                    await outputStream.FlushAsync();
                    await process.WaitForExitAsync();
                }
            }
        );

        return new EmptyResult();
    }

    [HttpPost]
    [Route("/v1/video/upload-chunk")]
    public async Task<IActionResult> UploadChunk()
    {
        try
        {
            string contentRange = Request.Headers["Content-Range"];
            long? startByte = null;
            long? endByte = null;
            long? totalSize = null;

            if (!string.IsNullOrEmpty(contentRange))
            {
                // Parse the Content-Range header to get the start, end, and total size.
                var range = contentRange.Replace("bytes ", "").Split('/');
                var bytes = range[0].Split('-');
                startByte = long.Parse(bytes[0]);
                endByte = long.Parse(bytes[1]);
                totalSize = long.Parse(range[1]);
            }

            using (var stream = new MemoryStream())
            {
                // Read the chunk from the request body.
                await Request.Body.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                // Process the chunk as needed (e.g., save it to a file).
                ProcessChunk(stream, startByte, endByte, totalSize);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    private void ProcessChunk(Stream chunkStream, long? startByte, long? endByte, long? totalSize)
    {
        // You can implement logic here to process and save the chunk.
        // For example, save it to a file, append to an in-memory buffer, etc.

        // Example: Save the chunk to a file.
        using var fileStream = new FileStream("uploaded_file.zip", FileMode.Append);
        chunkStream.CopyTo(fileStream);

        if (endByte == totalSize - 1)
        {
        }
        // When you receive the last chunk (endByte == totalSize - 1), 
        // you have received the complete file, and you can do further processing.
    }

    [HttpPost("/v1/video/stream/initial_chunk")]
    public async Task<IActionResult> GetInitialChunk([FromBody] StreamRequest fileRequest)
    {
        var movie = await _movieRepository.GetMovieById(fileRequest.FileId);
        if (movie == null) return StatusCode(500);
        // Read the file length.
        var fileLength = new FileInfo(movie.Path).Length;

        // Read the first chunk from the file. Adjust the chunk size as needed.
        const int chunkSize = 1024 * 1024; // 1 MB
        var buffer = new byte[chunkSize];
        await using var fs = new FileStream(movie.Path, FileMode.Open, FileAccess.Read);
        var bytesRead = fs.Read(buffer, 0, chunkSize);

        if (bytesRead > 0)
            // Return the length of the file and the first chunk and the time of the movie.
            return Ok(new { FileLength = fileLength, FirstChunk = buffer, MovieLenght = movie.TimeData });

        return BadRequest("Error reading the file.");
    }
}
