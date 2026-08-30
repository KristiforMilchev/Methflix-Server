using Domain.Models;
using Infrastructure.Repositories;
using Npgsql;

namespace Application.Repositories;

public class SiteConnectorRepository : BaseRepository, ISiteConnectorRepository
{
    public SiteConnectorRepository(NpgsqlConnection connection) : base(connection)
    {
    }

    public async Task<List<SiteConnector>> GetSiteConnectors()
    {
        var connectors = new List<SiteConnector>();
        const string sql = """SELECT * FROM "SiteConnectors" WHERE "IsDeleted" = false""";
        await OpenConnection();

        await using var command = CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            connectors.Add(ReadConnector(reader));

        return connectors;
    }

    public async Task<SiteConnector?> GetSiteConnector(int id)
    {
        const string sql = """SELECT * FROM "SiteConnectors" WHERE "Id" = @Id AND "IsDeleted" = false""";
        await OpenConnection();

        await using var command = CreateCommand(sql, new NpgsqlParameter("@Id", id));
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadConnector(reader) : null;
    }

    public async Task<bool> AddSiteConnector(SiteConnector connector)
    {
        const string sql = """
                            INSERT INTO "SiteConnectors" ("Name", "BaseUrl", "TopListingPath", "FetchStrategy",
                                "RowSelector", "TitleSelector", "DetailLinkSelector", "MagnetSelector", "CreatedBy",
                                "CreatedAt", "IsDeleted")
                            VALUES (@Name, @BaseUrl, @TopListingPath, @FetchStrategy, @RowSelector, @TitleSelector,
                                @DetailLinkSelector, @MagnetSelector, @CreatedBy, @CreatedAt, @IsDeleted)
                            """;
        await OpenConnection();

        await using var command = CreateCommand(sql,
            new NpgsqlParameter("@Name", connector.Name),
            new NpgsqlParameter("@BaseUrl", connector.BaseUrl),
            new NpgsqlParameter("@TopListingPath", connector.TopListingPath),
            new NpgsqlParameter("@FetchStrategy", (int)connector.FetchStrategy),
            new NpgsqlParameter("@RowSelector", connector.RowSelector),
            new NpgsqlParameter("@TitleSelector", connector.TitleSelector),
            new NpgsqlParameter("@DetailLinkSelector", connector.DetailLinkSelector),
            new NpgsqlParameter("@MagnetSelector", connector.MagnetSelector),
            new NpgsqlParameter("@CreatedBy", connector.CreatedBy),
            new NpgsqlParameter("@CreatedAt", DateTime.UtcNow),
            new NpgsqlParameter("@IsDeleted", false)
        );

        await command.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<List<SiteCategoryRoute>> GetCategoryRoutes(int siteConnectorId)
    {
        var routes = new List<SiteCategoryRoute>();
        const string sql = """SELECT * FROM "SiteCategoryRoutes" WHERE "SiteConnectorId" = @SiteConnectorId""";
        await OpenConnection();

        await using var command = CreateCommand(sql, new NpgsqlParameter("@SiteConnectorId", siteConnectorId));
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            routes.Add(ReadCategoryRoute(reader));

        return routes;
    }

    public async Task<SiteCategoryRoute?> GetCategoryRoute(int routeId)
    {
        const string sql = """SELECT * FROM "SiteCategoryRoutes" WHERE "Id" = @Id""";
        await OpenConnection();

        await using var command = CreateCommand(sql, new NpgsqlParameter("@Id", routeId));
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadCategoryRoute(reader) : null;
    }

    public async Task<bool> AddCategoryRoute(SiteCategoryRoute route)
    {
        const string sql = """
                            INSERT INTO "SiteCategoryRoutes" ("SiteConnectorId", "CategoryId", "Label",
                                "PathTemplate", "CreatedAt")
                            VALUES (@SiteConnectorId, @CategoryId, @Label, @PathTemplate, @CreatedAt)
                            """;
        await OpenConnection();

        await using var command = CreateCommand(sql,
            new NpgsqlParameter("@SiteConnectorId", route.SiteConnectorId),
            new NpgsqlParameter("@CategoryId", route.CategoryId),
            new NpgsqlParameter("@Label", route.Label),
            new NpgsqlParameter("@PathTemplate", route.PathTemplate),
            new NpgsqlParameter("@CreatedAt", DateTime.UtcNow)
        );

        await command.ExecuteNonQueryAsync();
        return true;
    }

    private static SiteConnector ReadConnector(NpgsqlDataReader reader)
    {
        return new SiteConnector
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            BaseUrl = reader.GetString(reader.GetOrdinal("BaseUrl")),
            TopListingPath = reader.GetString(reader.GetOrdinal("TopListingPath")),
            FetchStrategy = (PageFetchStrategy)reader.GetInt32(reader.GetOrdinal("FetchStrategy")),
            RowSelector = reader.GetString(reader.GetOrdinal("RowSelector")),
            TitleSelector = reader.GetString(reader.GetOrdinal("TitleSelector")),
            DetailLinkSelector = reader.GetString(reader.GetOrdinal("DetailLinkSelector")),
            MagnetSelector = reader.GetString(reader.GetOrdinal("MagnetSelector")),
            CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
            CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")).ToString("O"),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
        };
    }

    private static SiteCategoryRoute ReadCategoryRoute(NpgsqlDataReader reader)
    {
        return new SiteCategoryRoute
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            SiteConnectorId = reader.GetInt32(reader.GetOrdinal("SiteConnectorId")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            Label = reader.GetString(reader.GetOrdinal("Label")),
            PathTemplate = reader.GetString(reader.GetOrdinal("PathTemplate")),
            CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")).ToString("O")
        };
    }
}
