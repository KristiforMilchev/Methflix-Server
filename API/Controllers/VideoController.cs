using FFmpeg.Net;
using FFmpeg.Net.Data;
using FFmpeg.Net.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
 
namespace API.Controllers;

public class VideoController : ControllerBase
{
    [HttpGet("{videoName}")]
    public IActionResult GetVideo(string videoName)
    {
        // Specify the path to your video files directory.
        var videoPath = "/run/media/kristifor/M2/movies/Ahsoka.S01.1008p.WEB.H265-RAW/unistudiosglobe.mp4";

        if (!System.IO.File.Exists(videoPath))
        {
            return StatusCode(404);
        }

        var stream = System.IO.File.OpenRead(videoPath);

        return File(stream, "video/mp4"); // Adjust the media type based on your video format.
    }

    
 
    
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadVideo([FromForm] IFormFile file)
    {
         var outputFile = "output_video.mp4";
        
        if (file == null || file.Length == 0)
        {
            throw new Exception("Invalid file");
        }

        var uniqueFileName = GetUniqueFileName(file.FileName);
        var filePath = uniqueFileName; // Save to wwwroot/uploads directory

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        var conversion = new FFmpegClient(new FFmpegClientOptions(
            "/home/kristifor/software", true
            ));
        await conversion.ConvertAsync(new MediaFile(filePath, isStream: false), VideoType.MP4, "converted");
        
        Console.WriteLine("Conversion completed.");
        return StatusCode(200);
    }
    
    
    [HttpPost]
    [Route("uploadChunk")]
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
    
    private string GetUniqueFileName(string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return Path.GetFileNameWithoutExtension(fileName)
               + "_"
               + Guid.NewGuid().ToString().Substring(0, 6)
               + Path.GetExtension(fileName);
    }
}
