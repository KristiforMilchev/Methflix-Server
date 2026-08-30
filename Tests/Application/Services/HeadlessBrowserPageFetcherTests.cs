using Application.Services;
using NUnit.Framework;

namespace Tests.Application.Services;

// Live-network sanity check for the headless-browser fetch strategy: launches a real
// headless Chromium (downloading one via PuppeteerSharp's BrowserFetcher if no local
// browser/PUPPETEER_EXECUTABLE_PATH is found) and fetches a real page.
public class HeadlessBrowserPageFetcherTests
{
    [Test]
    [Timeout(120000)]
    public async Task FetchAsync_RealPage_ReturnsRenderedHtml()
    {
        await using var fetcher = new HeadlessBrowserPageFetcher();

        var html = await fetcher.FetchAsync("https://example.com/");

        Assert.That(html, Does.Contain("Example Domain"));
    }
}
