using Domain.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Controller]
public class StorageController : ControllerBase
{
    [HttpGet("/v1/get-torrents")]
    public async Task<IActionResult> GetTorrents()
    {
        return StatusCode(200);
    }

    [HttpPost("/v1/schedule-torrent")]
    public async Task<IActionResult> ScheduleTorrent([FromBody] ScheduleTorrentDownloadRequest request)
    {
        return StatusCode(200);
    }

    [HttpPost("/v1/stop-torrent-download")]
    public async Task<IActionResult> StopTorrentDownload([FromBody] ScheduleTorrentDownloadRequest request)
    {
        return StatusCode(200);
    }

    [HttpPost("/v1/resume-torrent-download")]
    public async Task<IActionResult> ResumeTorrentDownload([FromBody] ScheduleTorrentDownloadRequest request)
    {
        return StatusCode(200);
    }
    
    [HttpPost("/v1/cancel-torrent-download")]
    public async Task<IActionResult> CancelTorrentDownload([FromBody] ScheduleTorrentDownloadRequest request)
    {
        return StatusCode(200);
    }
}
