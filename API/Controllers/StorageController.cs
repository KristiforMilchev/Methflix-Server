using Domain.Dtos;
using Infrastucture.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Controller]
public class StorageController : ControllerBase
{
    private readonly ITorrentService _torrentService;

    public StorageController(ITorrentService torrentService)
    {
        _torrentService = torrentService;
    }
    
    [HttpGet("/v1/get-torrents")]
    public IActionResult GetTorrents()
    {
        var torrents = _torrentService.GetAllTorrents();
        if (torrents.Count == 0) return StatusCode(404);
        
        return Ok(torrents);
    }

    [HttpGet("/v1/get-torrent/{name}")]
    public IActionResult GetTorrent(string name)
    {
        var torrent = _torrentService.GetTorrentData(name);
        if (torrent == null) return StatusCode(404);
        
        return Ok(torrent);
    }

    [HttpPost("/v1/schedule-torrent")]
    public async Task<IActionResult> ScheduleTorrent([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var startDownloadFromUri = await _torrentService.StartDownloadFromUri(request.Url);
        return !startDownloadFromUri ? StatusCode(500) : Ok();
    }

    [HttpPost("/v1/upload-torrent-file")]
    public async Task<IActionResult> ScheduleTorrentFile([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var startDownloadFromFile = await _torrentService.StartDownloadFromFile(request.File);
        return !startDownloadFromFile ? StatusCode(500) : Ok();
    }
    
    [HttpPost("/v1/stop-torrent-download")]
    public async Task<IActionResult> StopTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var pauseDownload = await _torrentService.PauseDownload(request.Name);
        return !pauseDownload ? StatusCode(500) : Ok();
    }

    [HttpPost("/v1/resume-torrent-download")]
    public async Task<IActionResult> ResumeTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var resumeDownload = await _torrentService.ResumeDownload(request.Name);
        return !resumeDownload ? StatusCode(500) : Ok();
    }

    [HttpPost("/v1/cancel-torrent-download")]
    public async Task<IActionResult> CancelTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var cancelDownload = await _torrentService.CancelDownload(request.Name);
        return !cancelDownload ? StatusCode(500) : Ok();
    }
}
