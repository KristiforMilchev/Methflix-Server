using System.Web;
using Application;
using Domain.Dtos;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("/API/V1/[controller]")]
[ApiController]
public class IndexerController : ControllerBase
{
    private readonly ISiteConnectorRepository _siteConnectorRepository;
    private readonly IIndexerService _indexerService;
    private readonly ITorrentService _torrentService;

    public IndexerController(ISiteConnectorRepository siteConnectorRepository, IIndexerService indexerService,
        ITorrentService torrentService)
    {
        _siteConnectorRepository = siteConnectorRepository;
        _indexerService = indexerService;
        _torrentService = torrentService;
    }

    [HttpGet("Sources")]
    public async Task<IActionResult> GetSources()
    {
        var connectors = await _siteConnectorRepository.GetSiteConnectors();
        return Ok(connectors);
    }

    [HttpGet("Sources/{id}")]
    public async Task<IActionResult> GetSource(int id)
    {
        var connector = await _siteConnectorRepository.GetSiteConnector(id);
        return connector == null ? NotFound() : Ok(connector);
    }

    [HttpPost("Sources")]
    public async Task<IActionResult> AddSource([FromBody] SiteConnector connector)
    {
        var result = await _siteConnectorRepository.AddSiteConnector(connector);
        return result ? Ok() : StatusCode(500);
    }

    [HttpGet("Sources/{id}/Routes")]
    public async Task<IActionResult> GetRoutes(int id)
    {
        var routes = await _siteConnectorRepository.GetCategoryRoutes(id);
        return Ok(routes);
    }

    [HttpPost("Sources/{id}/Routes")]
    public async Task<IActionResult> AddRoute(int id, [FromBody] SiteCategoryRoute route)
    {
        route.SiteConnectorId = id;
        var result = await _siteConnectorRepository.AddCategoryRoute(route);
        return result ? Ok() : StatusCode(500);
    }

    [HttpGet("Sources/{id}/Browse")]
    public async Task<IActionResult> BrowseTop(int id, [FromQuery] int page = 1)
    {
        var connector = await _siteConnectorRepository.GetSiteConnector(id);
        if (connector == null) return NotFound();

        var items = await _indexerService.BrowseAsync(connector, connector.TopListingPath, page, HttpContext.RequestAborted);
        return Ok(items);
    }

    [HttpGet("Sources/{id}/Browse/{routeId}")]
    public async Task<IActionResult> BrowseCategory(int id, int routeId, [FromQuery] int page = 1)
    {
        var connector = await _siteConnectorRepository.GetSiteConnector(id);
        if (connector == null) return NotFound();

        var route = await _siteConnectorRepository.GetCategoryRoute(routeId);
        if (route == null || route.SiteConnectorId != id) return NotFound();

        var items = await _indexerService.BrowseAsync(connector, route.PathTemplate, page, HttpContext.RequestAborted);
        return Ok(items);
    }

    [HttpPost("Import")]
    public async Task<IActionResult> Import([FromBody] ImportListingRequest request)
    {
        var connector = await _siteConnectorRepository.GetSiteConnector(request.SiteConnectorId);
        if (connector == null) return NotFound();

        var magnet = await _indexerService.ResolveMagnetAsync(connector, request.DetailUrl, HttpContext.RequestAborted);
        if (magnet == null)
        {
            return Ok(new ImportListingResult
            {
                Started = false,
                DetailUrl = request.DetailUrl,
                Message = "Couldn't resolve a magnet link automatically for this listing. Open the detail page, " +
                          "copy the magnet link yourself, and POST it to /API/V1/Storage/Schedule-Torrent."
            });
        }

        var name = HttpUtility.ParseQueryString(new Uri(magnet).Query)["dn"];
        if (string.IsNullOrEmpty(name)) return StatusCode(500);

        if (request.CategoryId.HasValue)
            PendingCategoryAssignments.Set(name, request.CategoryId.Value);

        var started = await _torrentService.StartDownloadFromUri(magnet);
        if (!started) return StatusCode(500);

        return Ok(new ImportListingResult
        {
            Started = true,
            Magnet = magnet,
            DetailUrl = request.DetailUrl,
            Message = "Download started."
        });
    }
}
