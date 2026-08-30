using Domain.Models;

namespace Infrastructure.Interfaces;

public interface IPageFetcher
{
    PageFetchStrategy Strategy { get; }
    Task<string> FetchAsync(string url, CancellationToken token = default);
}
