using Domain.Models;
using Infrastructure.Interfaces;
using PuppeteerSharp;

namespace Application.Services;

// Some indexer sites (e.g. 1337x) sit behind a Cloudflare JS challenge that a plain
// HttpClient GET can't pass (returns 403). This drives a real headless Chromium
// instead, which executes the challenge like a normal browser would.
public class HeadlessBrowserPageFetcher : IPageFetcher, IMagnetLinkFetcher, IAsyncDisposable
{
    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

    private static readonly string[] LocalBrowserCandidates =
    {
        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        "/usr/bin/chromium",
        "/usr/bin/chromium-browser",
        "/usr/bin/google-chrome"
    };

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private IBrowser? _browser;

    public PageFetchStrategy Strategy => PageFetchStrategy.HeadlessBrowser;

    public async Task<string> FetchAsync(string url, CancellationToken token = default)
    {
        var browser = await GetBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await page.SetUserAgentAsync(UserAgent);

        await page.GoToAsync(url, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle2 },
            Timeout = 30000
        });

        await WaitPastCloudflareChallenge(page, token);

        return await page.GetContentAsync();
    }

    // Some sites (e.g. 1337x) don't put the real magnet: URI in the link's href -
    // their own JS swaps it in (often via a popup window, id="openPopup" on 1337x)
    // when the link is clicked, as a scraping deterrent. This clicks for real and
    // intercepts requests on both the original page and any popup it opens, aborting
    // the resulting magnet: navigation (Chrome can't natively handle that scheme)
    // instead of letting it fail, so it can be captured regardless of which
    // page/window it fires from.
    public async Task<string?> ResolveMagnetAsync(string detailUrl, string linkSelector, CancellationToken token = default)
    {
        var browser = await GetBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await page.SetUserAgentAsync(UserAgent);

        string? magnetUrl = null;

        async void OnRequest(object? _, RequestEventArgs e)
        {
            if (e.Request.Url.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase))
            {
                magnetUrl ??= e.Request.Url;
                await e.Request.AbortAsync();
            }
            else
            {
                await e.Request.ContinueAsync();
            }
        }

        async Task AttachInterception(IPage target)
        {
            await target.SetRequestInterceptionAsync(true);
            target.Request += OnRequest;
        }

        await AttachInterception(page);

        async void OnTargetCreated(object? _, TargetChangedArgs e)
        {
            if (e.Target.Type != TargetType.Page) return;
            var popup = await e.Target.PageAsync();
            if (popup != null) await AttachInterception(popup);
        }

        browser.TargetCreated += OnTargetCreated;

        try
        {
            await page.GoToAsync(detailUrl, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle2 },
                Timeout = 30000
            });

            await WaitPastCloudflareChallenge(page, token);

            var link = await page.QuerySelectorAsync(linkSelector);
            if (link == null) return null;

            await link.ClickAsync();

            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (magnetUrl == null && DateTime.UtcNow < deadline)
                await Task.Delay(250, token);

            return magnetUrl;
        }
        finally
        {
            browser.TargetCreated -= OnTargetCreated;
        }
    }

    // Cloudflare's interstitial is titled "Just a moment..." while its challenge runs.
    // Poll briefly until the real page has taken over.
    private static async Task WaitPastCloudflareChallenge(IPage page, CancellationToken token)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var title = await page.GetTitleAsync();
            if (!title.Contains("Just a moment", StringComparison.OrdinalIgnoreCase)) break;
            await Task.Delay(1000, token);
        }
    }

    private async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser is { IsClosed: false }) return _browser;

        await _initLock.WaitAsync();
        try
        {
            if (_browser is { IsClosed: false }) return _browser;

            var executablePath = ResolveExecutablePath();
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            if (executablePath != null)
            {
                launchOptions.ExecutablePath = executablePath;
            }
            else
            {
                await new BrowserFetcher().DownloadAsync();
            }

            _browser = await Puppeteer.LaunchAsync(launchOptions);
            return _browser;
        }
        finally
        {
            _initLock.Release();
        }
    }

    // Prefer a browser that's already on disk (an apt-installed chromium in Docker via
    // PUPPETEER_EXECUTABLE_PATH, or a local Chrome/Edge install) over downloading one.
    private static string? ResolveExecutablePath()
    {
        var configured = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");
        if (!string.IsNullOrEmpty(configured) && File.Exists(configured)) return configured;

        return LocalBrowserCandidates.FirstOrDefault(File.Exists);
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null) await _browser.CloseAsync();
        _initLock.Dispose();
    }
}
