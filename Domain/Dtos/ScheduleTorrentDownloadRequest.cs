using Microsoft.AspNetCore.Http;

namespace Domain.Dtos;

public class ScheduleTorrentDownloadRequest
{
    public int Id { get; set; }
    public string Url { get; set; }
    public IFormFile File { get; set; }
}
