using API.Controllers;
using Application;
using Domain.Dtos;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Tests.API.Controllers;

public class IndexerControllerTests
{
    private Mock<ISiteConnectorRepository> _siteConnectorRepository;
    private Mock<IIndexerService> _indexerService;
    private Mock<ITorrentService> _torrentService;
    private IndexerController _controller;
    private SiteConnector _connector;

    [SetUp]
    public void Setup()
    {
        _siteConnectorRepository = new Mock<ISiteConnectorRepository>();
        _indexerService = new Mock<IIndexerService>();
        _torrentService = new Mock<ITorrentService>();
        _controller = new IndexerController(_siteConnectorRepository.Object, _indexerService.Object, _torrentService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        _connector = new SiteConnector
        {
            Id = 1,
            Name = "Test Site",
            BaseUrl = "https://example-torrents.test",
            TopListingPath = "/latest/",
            FetchStrategy = PageFetchStrategy.Http,
            RowSelector = "tr",
            TitleSelector = "a",
            DetailLinkSelector = "a",
            MagnetSelector = "a.magnet",
            CreatedBy = 1
        };

        _siteConnectorRepository.Setup(r => r.GetSiteConnector(1)).ReturnsAsync(_connector);
    }

    [Test]
    public async Task Import_MagnetResolved_StartsDownloadAndAssignsCategory()
    {
        const string magnet = "magnet:?xt=urn:btih:ABC123&dn=Some.Movie.2024";
        _indexerService
            .Setup(s => s.ResolveMagnetAsync(_connector, "https://example-torrents.test/detail/1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(magnet);
        _torrentService.Setup(s => s.StartDownloadFromUri(magnet)).ReturnsAsync(true);

        var result = await _controller.Import(new ImportListingRequest
        {
            SiteConnectorId = 1,
            DetailUrl = "https://example-torrents.test/detail/1",
            CategoryId = 5
        });

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var body = ok!.Value as ImportListingResult;
        Assert.That(body!.Started, Is.True);
        Assert.That(body.Magnet, Is.EqualTo(magnet));

        _torrentService.Verify(s => s.StartDownloadFromUri(magnet), Times.Once);
        Assert.That(PendingCategoryAssignments.TakeOrDefault("Some.Movie.2024"), Is.EqualTo(5));
    }

    [Test]
    public async Task Import_MagnetNotResolved_ReturnsManualFallback()
    {
        _indexerService
            .Setup(s => s.ResolveMagnetAsync(_connector, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _controller.Import(new ImportListingRequest
        {
            SiteConnectorId = 1,
            DetailUrl = "https://example-torrents.test/detail/2"
        });

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var body = ok!.Value as ImportListingResult;
        Assert.That(body!.Started, Is.False);
        Assert.That(body.DetailUrl, Is.EqualTo("https://example-torrents.test/detail/2"));

        _torrentService.Verify(s => s.StartDownloadFromUri(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Import_UnknownSiteConnector_ReturnsNotFound()
    {
        _siteConnectorRepository.Setup(r => r.GetSiteConnector(999)).ReturnsAsync((SiteConnector?)null);

        var result = await _controller.Import(new ImportListingRequest { SiteConnectorId = 999, DetailUrl = "https://x.test/1" });

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
