using System.Web;
using Application;
using Domain.Dtos;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using MonoTorrent.Client;

namespace API.Controllers;

[Route("/API/V1/[controller]")]
[ApiController]
public class StorageController : ControllerBase
{
    private readonly ITorrentService _torrentService;
    private readonly ITorrentRepository _torrentRepository;

    public StorageController(ITorrentService torrentService, ITorrentRepository torrentRepository)
    {
        _torrentService = torrentService;
        _torrentRepository = torrentRepository;
    }
    
    [HttpGet("Get-Torrents")]
    public IActionResult GetTorrents()
    {
        var torrents = _torrentService.GetAllTorrents();
        if (torrents.Count == 0) return StatusCode(404);
        
        return Ok(torrents);
    }

    [HttpGet("Get-Torrent/{name}")]
    public IActionResult GetTorrent(string name)
    {
        var torrent = _torrentService.GetTorrentData(name);
        if (torrent == null) return StatusCode(404);
        
        return Ok(torrent);
    }

    [HttpPost("Schedule-Torrent")]
    public async Task<IActionResult> ScheduleTorrent([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var uri = new Uri(request.Url);

        var queryParameters = HttpUtility.ParseQueryString(uri.Query);
        var name = queryParameters["dn"];
        if (name == string.Empty) return StatusCode(500);
        
        Notifier.Subscribe(name!, OnTorrentDownloaded);
        var startDownloadFromUri = await _torrentService.StartDownloadFromUri(request.Url);
        return !startDownloadFromUri ? StatusCode(500) : Ok();
    }

    private void OnTorrentDownloaded(object data)
    {
        if (data is not TorrentManager torrent) return;
        
        if(torrent.Progress < 100) return;
        
        _torrentRepository.UpdateTorrentDownloadComplete(torrent);
        Notifier.Dispose(torrent.Name);
    }

    [HttpPost("Upload-Torrent-File")]
    public async Task<IActionResult> ScheduleTorrentFile([FromForm] ScheduleTorrentDownloadRequest request)
    {
    
        var startDownloadFromFile = await _torrentService.StartDownloadFromFile(request.File);
        return !startDownloadFromFile ? StatusCode(500) : Ok();
    }
    
    [HttpPost("Stop-Torrent-Download")]
    public async Task<IActionResult> StopTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var pauseDownload = await _torrentService.PauseDownload(request.Name);
        return !pauseDownload ? StatusCode(500) : Ok();
    }

    [HttpPost("Resume-Torrent-Download")]
    public async Task<IActionResult> ResumeTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var resumeDownload = await _torrentService.ResumeDownload(request.Name);
        return !resumeDownload ? StatusCode(500) : Ok();
    }

    [HttpPost("Cancel-Torrent-Download")]
    public async Task<IActionResult> CancelTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var cancelDownload = await _torrentService.CancelDownload(request.Name);
        return !cancelDownload ? StatusCode(500) : Ok();
    }
}
