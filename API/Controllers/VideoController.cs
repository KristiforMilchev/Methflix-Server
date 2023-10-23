using System.Diagnostics;
using Domain.Dtos;
using FFmpeg.Net;
using FFmpeg.Net.Data;
using FFmpeg.Net.Enums;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class VideoController : ControllerBase
{
    private readonly string _segmentFolder;
    private readonly IFfmpegService _ffmpegService;
    private readonly IStorageService _storage;
    public VideoController(IConfiguration configuration, IFfmpegService ffmpegService, IStorageService storageService)
    {
        _segmentFolder = configuration["StorageManager:StreamSegments"] ?? string.Empty;
        _ffmpegService = ffmpegService;
        _storage = storageService;
    }
    
    [HttpGet("/v1/download/{videoName}")]
    public IActionResult Video(string videoName)
    {
        var exists = _storage.GetFilePath(videoName);
        
        if (!System.IO.File.Exists(exists)) return StatusCode(404);

        var stream = System.IO.File.OpenRead(exists);

        return File(stream, "video/mp4");
        
    }
    
    [HttpGet("v1/segmented")]
    public IActionResult GetSegmentedVideo(VideoStreamRequest request)
    {
        var chunk = _ffmpegService.GetChunk(request.SegmentFrom, request.SegmentTo, request.Movie);   
        
        Response.Headers.Add("Content-Type", "video/mp4");
        Response.Headers.Add("Content-Disposition", $"inline; filename={chunk}");

        return PhysicalFile($"{_segmentFolder}{chunk}", "video/mp4");
    }

    

    [HttpPost]
    [Route("v1/upload-chunk")]
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

        // When you receive the last chunk (endByte == totalSize - 1), 
        // you have received the complete file, and you can do further processing.
    }

}
