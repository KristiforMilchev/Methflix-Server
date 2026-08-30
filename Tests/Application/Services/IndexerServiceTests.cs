using Application.Services;
using Domain.Models;
using Infrastructure.Interfaces;
using Moq;
using NUnit.Framework;

namespace Tests.Application.Services;

public class IndexerServiceTests
{
    private Mock<IPageFetcher> _mockFetcher;
    private IndexerService _indexerService;
    private SiteConnector _connector;

    [SetUp]
    public void Setup()
    {
        _mockFetcher = new Mock<IPageFetcher>();
        _mockFetcher.SetupGet(f => f.Strategy).Returns(PageFetchStrategy.Http);
        _indexerService = new IndexerService(new[] { _mockFetcher.Object });

        _connector = new SiteConnector
        {
            Id = 1,
            Name = "Test Site",
            BaseUrl = "https://example-torrents.test",
            TopListingPath = "/latest/{page}/",
            FetchStrategy = PageFetchStrategy.Http,
            RowSelector = "table.list tr.result",
            TitleSelector = "a.title",
            DetailLinkSelector = "a.title",
            MagnetSelector = "a.magnet-link",
            CreatedBy = 1
        };
    }

    private const string ListingHtml = """
        <html><body>
        <table class="list">
            <tr class="result"><td><a class="title" href="/detail/1/some-movie">Some Movie 2024</a></td></tr>
            <tr class="result"><td><a class="title" href="/detail/2/another-movie">Another Movie 2023</a></td></tr>
        </table>
        </body></html>
        """;

    private const string DetailHtml = """
        <html><body>
        <a class="magnet-link" href="magnet:?xt=urn:btih:ABC123&dn=Some+Movie+2024">Magnet</a>
        </body></html>
        """;

    [Test]
    public async Task BrowseAsync_ParsesRowsIntoListingItems()
    {
        _mockFetcher
            .Setup(f => f.FetchAsync(It.Is<string>(u => u.Contains("/latest/1/")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ListingHtml);

        var items = await _indexerService.BrowseAsync(_connector, _connector.TopListingPath, 1);

        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items[0].Title, Is.EqualTo("Some Movie 2024"));
        Assert.That(items[0].DetailUrl, Is.EqualTo("https://example-torrents.test/detail/1/some-movie"));
        Assert.That(items[1].Title, Is.EqualTo("Another Movie 2023"));
    }

    [Test]
    public async Task BrowseAsync_SubstitutesPageNumberIntoPathTemplate()
    {
        _mockFetcher
            .Setup(f => f.FetchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ListingHtml);

        await _indexerService.BrowseAsync(_connector, _connector.TopListingPath, 3);

        _mockFetcher.Verify(f => f.FetchAsync("https://example-torrents.test/latest/3/", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ResolveMagnetAsync_ExtractsMagnetLink()
    {
        _mockFetcher
            .Setup(f => f.FetchAsync("https://example-torrents.test/detail/1/some-movie", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DetailHtml);

        var magnet = await _indexerService.ResolveMagnetAsync(_connector, "https://example-torrents.test/detail/1/some-movie");

        Assert.That(magnet, Is.EqualTo("magnet:?xt=urn:btih:ABC123&dn=Some+Movie+2024"));
    }

    [Test]
    public async Task BrowseAsync_UnknownFetchStrategy_Throws()
    {
        _connector.FetchStrategy = PageFetchStrategy.HeadlessBrowser;

        Assert.ThrowsAsync<NotSupportedException>(async () =>
            await _indexerService.BrowseAsync(_connector, _connector.TopListingPath, 1));
    }
}
