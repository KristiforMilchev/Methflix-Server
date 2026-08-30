using Domain.Models;
using Infrastructure.Interfaces;

namespace Application.Services;

public class HttpPageFetcher : IPageFetcher
{
    private readonly HttpClient _httpClient;

    public HttpPageFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public PageFetchStrategy Strategy => PageFetchStrategy.Http;

    public async Task<string> FetchAsync(string url, CancellationToken token = default)
    {
        using var response = await _httpClient.GetAsync(url, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(token);
    }
}
