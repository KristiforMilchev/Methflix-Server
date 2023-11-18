using Domain.Dtos;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Npgsql;

namespace Application.Repositories;

public class MovieRepository : BaseRepository, IMovieRepository
{
    private readonly ITvShowsRepository _tvShowsRepository;

    public MovieRepository(NpgsqlConnection connection, ITvShowsRepository tvShowsRepository) : base(connection)
    {
        _tvShowsRepository = tvShowsRepository;
    }

    public async Task<List<Movie>> GetAllMovies()
    {
        await using var command = CreateCommand("SELECT * FROM Movies", Array.Empty<NpgsqlParameter>());
        await using var reader = await command.ExecuteReaderAsync();
        var movies = new List<Movie>();
        while (await reader.ReadAsync())
            movies.Add(
                new Movie
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    TimeData = reader.GetTimeSpan(reader.GetOrdinal("TimeData")),
                    Path = reader.GetString(reader.GetOrdinal("Path")),
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    TorrentId = reader.IsDBNull(reader.GetOrdinal("TorrentId"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("TorrentId")),
                    DownloadId = reader.IsDBNull(reader.GetOrdinal("DownloadId"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("DownloadId")),
                    Extension = reader.IsDBNull(reader.GetOrdinal("Extension"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("Extension")),
                    Thumbnail = reader.IsDBNull(reader.GetOrdinal("Thumbnail"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Thumbnail")),
                    TvShowId = reader.IsDBNull(reader.GetOrdinal("TvShowId"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("TvShowId"))
                }
            );
        return movies;
    }

    public async Task<Category?> GetCategoryMovies(int categoryId)
    {
        var sql = "SELECT * FROM Categories WHERE Id = @CategoryId";
        var parameters = new NpgsqlParameter[]
        {
            new("@CategoryId", categoryId)
        };
        await using var command = CreateCommand(sql, parameters);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var category = new Category
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                CreatedAt = reader.GetString(reader.GetOrdinal("CreatedAt")),
                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("UpdatedBy")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("UpdatedAt")),
                IsDeleted = reader.IsDBNull(reader.GetOrdinal("IsDeleted"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("IsDeleted"))
            };

            return category;
        }

        return null;
    }

    public async Task<List<Category>> GetCategoryWithMovies()
    {
        await using var command = CreateCommand(
            "SELECT * FROM Categories WHERE Id IN (SELECT DISTINCT CategoryId FROM Movies WHERE TvShowId IS NULL)",
            Array.Empty<NpgsqlParameter>()
        );
        await using var reader = await command.ExecuteReaderAsync();
        var categories = new List<Category>();
        while (await reader.ReadAsync())
            categories.Add(
                new Category
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                    CreatedAt = reader.GetString(reader.GetOrdinal("CreatedAt")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("UpdatedBy")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("UpdatedAt")),
                    IsDeleted = reader.IsDBNull(reader.GetOrdinal("IsDeleted"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("IsDeleted"))
                }
            );
        return categories;
    }

    public async Task<bool> UpdateMovie(Movie movie)
    {
        var query = @"UPDATE Movies SET Name = @Name, TimeData = @TimeData, Path = @Path, CategoryId = @CategoryId,
                 TorrentId = @TorrentId, DownloadId = @DownloadId, Extension = @Extension, Thumbnail = @Thumbnail,
                 TvShowId = @TvShowId WHERE Id = @Id";

        var parameters = new NpgsqlParameter[]
        {
            new("@Id", movie.Id),
            new("@Name", movie.Name),
            new("@TimeData", movie.TimeData),
            new("@Path", movie.Path),
            new("@CategoryId", movie.CategoryId),
            new("@TorrentId", movie.TorrentId ?? (object)DBNull.Value),
            new("@DownloadId", movie.DownloadId ?? (object)DBNull.Value),
            new("@Extension", movie.Extension ?? (object)DBNull.Value),
            new("@Thumbnail", movie.Thumbnail ?? (object)DBNull.Value),
            new("@TvShowId", movie.TvShowId ?? (object)DBNull.Value)
        };

        await using var command = CreateCommand(
            query,
            parameters
        );

        await command.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<List<TvShow>> GetCategoryTvShows(int id)
    {
        const string sql = "SELECT ts.Id, ts.Name, ts.Season, ts.CreatedBy, ts.CreatedAt " +
                           "FROM TvShows ts " +
                           "WHERE ts.Id IN (SELECT DISTINCT TvShowId FROM Movies WHERE CategoryId = @CategoryId)";

        var parameters = new NpgsqlParameter[]
        {
            new("@CategoryId", id)
        };
        await using var command = CreateCommand(sql, parameters);

        await using var reader = await command.ExecuteReaderAsync();
        var tvShows = new List<TvShow>();

        while (await reader.ReadAsync())
            tvShows.Add(
                new TvShow
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.IsDBNull(reader.GetOrdinal("Name"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Name")),
                    Season = reader.IsDBNull(reader.GetOrdinal("Season"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("Season")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("CreatedAt"))
                }
            );

        return tvShows;
    }

    public async Task<TvShowSeasonDto> GetTvShowEpisodesById(int id)
    {
        var sql =
            "SELECT ASE.\"Season\", m.\"TvShowId\", m.\"Id\", m.\"Name\" " +
            "FROM \"AssociatedSeasonEpisodes\" ASE " +
            "JOIN \"Movies\" m ON m.\"Id\" = ASE.\"MovieId\" " +
            "WHERE ASE.\"TvShowId\" = @TvShowId";

        var parameters = new NpgsqlParameter[]
        {
            new("@TvShowId", id)
        };
        await using var command = CreateCommand(sql, parameters);

        await using var reader = await command.ExecuteReaderAsync();
        var tvShowSeasonData = new TvShowSeasonDto();
        var showName = await _tvShowsRepository.TvShowName(id);
        if (showName == string.Empty) return new TvShowSeasonDto();

        tvShowSeasonData.Name = showName;
        while (await reader.ReadAsync())
        {
            var season = reader.GetInt32(reader.GetOrdinal("Season"));
            var movieId = reader.GetInt32(reader.GetOrdinal("Id"));
            var movieName = reader.GetString(reader.GetOrdinal("Name"));
            var movies = new SeasonData
            {
                Season = season,
                Movies = new List<SeasonMovie>
                {
                    new()
                    {
                        Id = movieId,
                        Name = movieName
                    }
                }
            };
            tvShowSeasonData.Seasons.Add(movies);
        }

        return tvShowSeasonData;
    }

    public async Task<Movie?> GetMovieById(int id)
    {
        var sql = "SELECT * FROM Movies WHERE Id = @Id";

        var parameters = new NpgsqlParameter[]
        {
            new("@Id", id)
        };

        await using var command = CreateCommand(sql, parameters);
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return new Movie
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                TimeData = reader.GetTimeSpan(reader.GetOrdinal("TimeData")),
                Path = reader.GetString(reader.GetOrdinal("Path")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                TorrentId = reader.IsDBNull(reader.GetOrdinal("TorrentId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("TorrentId")),
                DownloadId = reader.IsDBNull(reader.GetOrdinal("DownloadId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("DownloadId")),
                Extension = reader.IsDBNull(reader.GetOrdinal("Extension"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("Extension")),
                Thumbnail = reader.IsDBNull(reader.GetOrdinal("Thumbnail"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Thumbnail")),
                TvShowId = reader.IsDBNull(reader.GetOrdinal("TvShowId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("TvShowId"))
            };

        return null;
    }
}
