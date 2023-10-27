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
    private readonly string _segmentFolder;
    private readonly IFfmpegService _ffmpegService;
    private readonly IStorageService _storage;
    private readonly IMovieRepository _movieRepository;
    public VideoController(IConfiguration configuration, IFfmpegService ffmpegService, IStorageService storageService,
        IMovieRepository movieRepository)
    {
        _segmentFolder = configuration["StorageManager:StreamSegments"] ?? string.Empty;
        _ffmpegService = ffmpegService;
        _storage = storageService;
        _movieRepository = movieRepository;
    }
    
    [HttpGet("/download/{videoName}")]
    public IActionResult Video(string videoName)
    {
        var exists = _storage.GetFilePath(videoName);
        
        if (!System.IO.File.Exists(exists)) return StatusCode(404);

        var stream = System.IO.File.OpenRead(exists);

        return File(stream, "video/mp4");
        
    }
    
    [HttpGet("/stream/segmented")]
    public async Task<IActionResult> GetSegmentedVideo(VideoStreamRequest request)
    {
        var movie = await _movieRepository.GetMovieById(request.MovieId);
        if (movie == null)
        {
            return StatusCode(500);
        }
        
        // Determine the next segment to serve.
        var nextSegment = request.LastSegment + 1;

        // Calculate the segment duration, e.g., 10 seconds per segment.
        var segmentDuration = 10;

        // Determine the segment range.
        var nextSegmentFrom = nextSegment * segmentDuration;
        var nextSegmentTo = (nextSegment + 1) * segmentDuration;

        // Use your existing GetChunk method to get the next segment.
        var chunk = _ffmpegService.GetChunk(nextSegmentFrom, nextSegmentTo, movie.Path, movie.Name);

        if (string.IsNullOrEmpty(chunk)) return BadRequest("No more segments available.");
        // Update the last segment.
        request.LastSegment = nextSegment;
        
        // Set the response headers.
        Response.Headers.Add("Content-Type", "video/mp4");
        Response.Headers.Add("Content-Disposition", $"inline; filename={chunk}");
        
        // Serve the next segment.
        return PhysicalFile(chunk, "video/mp4");
    }

    [HttpPost]
    [Route("/upload-chunk")]
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
    
    [HttpPost("/stream/initial_chunk")]
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
        {
            // Return the length of the file and the first chunk and the time of the movie.
            return Ok(new { FileLength = fileLength, FirstChunk = buffer, MovieLenght = movie.TimeData });
        }

        return BadRequest("Error reading the file.");
    }
}
