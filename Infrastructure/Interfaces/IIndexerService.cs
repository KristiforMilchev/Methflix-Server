using Domain.Dtos;
using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IIndexerService
{
    Task<List<IndexerListingItem>> BrowseAsync(SiteConnector connector, string pathTemplate, int page, CancellationToken token = default);
    Task<string?> ResolveMagnetAsync(SiteConnector connector, string detailUrl, CancellationToken token = default);
}
