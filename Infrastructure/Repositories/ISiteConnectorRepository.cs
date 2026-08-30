using Domain.Models;

namespace Infrastructure.Repositories;

public interface ISiteConnectorRepository
{
    public Task<List<SiteConnector>> GetSiteConnectors();
    public Task<SiteConnector?> GetSiteConnector(int id);
    public Task<bool> AddSiteConnector(SiteConnector connector);
    public Task<List<SiteCategoryRoute>> GetCategoryRoutes(int siteConnectorId);
    public Task<SiteCategoryRoute?> GetCategoryRoute(int routeId);
    public Task<bool> AddCategoryRoute(SiteCategoryRoute route);
}
