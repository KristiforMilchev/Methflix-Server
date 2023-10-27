using System.Web;
using Application;
using Domain.Dtos;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using MonoTorrent.Client;

namespace API.Controllers;

[Route("/api/v1/[controller]")]
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
    
    [HttpGet("/v1/storage/get-torrents")]
    public IActionResult GetTorrents()
    {
        var torrents = _torrentService.GetAllTorrents();
        if (torrents.Count == 0) return StatusCode(404);
        
        return Ok(torrents);
    }

    [HttpGet("/v1/storage/get-torrent/{name}")]
    public IActionResult GetTorrent(string name)
    {
        var torrent = _torrentService.GetTorrentData(name);
        if (torrent == null) return StatusCode(404);
        
        return Ok(torrent);
    }

    [HttpPost("/v1/storage/schedule-torrent")]
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

    [HttpPost("/v1/storage/upload-torrent-file")]
    public async Task<IActionResult> ScheduleTorrentFile([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var startDownloadFromFile = await _torrentService.StartDownloadFromFile(request.File);
        return !startDownloadFromFile ? StatusCode(500) : Ok();
    }
    
    [HttpPost("/v1/storage/stop-torrent-download")]
    public async Task<IActionResult> StopTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var pauseDownload = await _torrentService.PauseDownload(request.Name);
        return !pauseDownload ? StatusCode(500) : Ok();
    }

    [HttpPost("/v1/storage/resume-torrent-download")]
    public async Task<IActionResult> ResumeTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var resumeDownload = await _torrentService.ResumeDownload(request.Name);
        return !resumeDownload ? StatusCode(500) : Ok();
    }

    [HttpPost("/v1/storage/cancel-torrent-download")]
    public async Task<IActionResult> CancelTorrentDownload([FromForm] ScheduleTorrentDownloadRequest request)
    {
        var cancelDownload = await _torrentService.CancelDownload(request.Name);
        return !cancelDownload ? StatusCode(500) : Ok();
    }
}
