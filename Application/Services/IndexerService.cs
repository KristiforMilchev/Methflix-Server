using AngleSharp.Html.Parser;
using Domain.Dtos;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Application.Services;

public class IndexerService : IIndexerService
{
    private readonly IEnumerable<IPageFetcher> _fetchers;
    private readonly HtmlParser _parser = new();

    public IndexerService(IEnumerable<IPageFetcher> fetchers)
    {
        _fetchers = fetchers;
    }

    public async Task<List<IndexerListingItem>> BrowseAsync(SiteConnector connector, string pathTemplate, int page, CancellationToken token = default)
    {
        var fetcher = ResolveFetcher(connector.FetchStrategy);
        var url = ResolveUrl(connector.BaseUrl, ApplyPage(pathTemplate, page));
        var html = await fetcher.FetchAsync(url, token);

        using var document = await _parser.ParseDocumentAsync(html, token);

        var items = new List<IndexerListingItem>();
        foreach (var row in document.QuerySelectorAll(connector.RowSelector))
        {
            var title = row.QuerySelector(connector.TitleSelector)?.TextContent.Trim();
            var href = row.QuerySelector(connector.DetailLinkSelector)?.GetAttribute("href");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(href)) continue;

            items.Add(new IndexerListingItem(title, ResolveUrl(connector.BaseUrl, href)));
        }

        return items;
    }

    public async Task<string?> ResolveMagnetAsync(SiteConnector connector, string detailUrl, CancellationToken token = default)
    {
        var fetcher = ResolveFetcher(connector.FetchStrategy);

        if (fetcher is IMagnetLinkFetcher magnetLinkFetcher)
            return await magnetLinkFetcher.ResolveMagnetAsync(detailUrl, connector.MagnetSelector, token);

        var html = await fetcher.FetchAsync(detailUrl, token);

        using var document = await _parser.ParseDocumentAsync(html, token);

        var href = document.QuerySelector(connector.MagnetSelector)?.GetAttribute("href");
        return href != null && href.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase) ? href : null;
    }

    private IPageFetcher ResolveFetcher(PageFetchStrategy strategy)
    {
        var fetcher = _fetchers.FirstOrDefault(f => f.Strategy == strategy);
        if (fetcher == null)
            throw new NotSupportedException($"No page fetcher registered for strategy '{strategy}'.");

        return fetcher;
    }

    private static string ApplyPage(string pathTemplate, int page)
    {
        return pathTemplate.Contains("{page}")
            ? pathTemplate.Replace("{page}", page.ToString())
            : pathTemplate;
    }

    private static string ResolveUrl(string baseUrl, string path)
    {
        return Uri.TryCreate(path, UriKind.Absolute, out var absolute) &&
               (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps)
            ? absolute.ToString()
            : new Uri(new Uri(baseUrl), path).ToString();
    }
}
