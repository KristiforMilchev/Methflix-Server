namespace Infrastructure.Interfaces;

// Optional extra capability a page fetcher can implement when magnet links aren't a
// static href on the detail page (e.g. sites that swap the real magnet: URI in via
// JS on click, to deter plain scraping). IndexerService uses this when the resolved
// IPageFetcher supports it, falling back to a static href lookup otherwise.
public interface IMagnetLinkFetcher
{
    Task<string?> ResolveMagnetAsync(string detailUrl, string linkSelector, CancellationToken token = default);
}
