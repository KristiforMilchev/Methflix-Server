using System.Runtime.CompilerServices;
using Domain.Models;
using Infrastructure.Repositories;
using Npgsql;

namespace Application.Repositories;

public class CategoryRepository : BaseRepository, ICategoryRepository 
{
    private readonly IMovieRepository _movieRepository;
    public CategoryRepository(NpgsqlConnection connection, IMovieRepository movieRepository) : base(connection)
    {
        _movieRepository = movieRepository; 
    }

    public async Task<Category?> GetCategory(int id)
    {
        var sql = """SELECT * FROM "Category" WHERE "Id" = @Id""";
        await OpenConnection();

        await using var command = CreateCommand(sql, new NpgsqlParameter("@Id", id));
        
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Category
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                CreatedAt = reader.GetString(reader.GetOrdinal("CreatedAt")),
                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetString(reader.GetOrdinal("UpdatedAt")),
                IsDeleted = reader.IsDBNull(reader.GetOrdinal("IsDeleted")) ? null : reader.GetInt32(reader.GetOrdinal("IsDeleted"))
            };
        }

        return null;
    }

    public async Task<List<Category>> GetCategories()
    {
        var categories = new List<Category>();
        var sql = """SELECT * FROM "Category" """;
        await OpenConnection();

        await using var command = CreateCommand(sql);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                CreatedAt = reader.GetString(reader.GetOrdinal("CreatedAt")),
                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetString(reader.GetOrdinal("UpdatedAt")),
                IsDeleted = reader.IsDBNull(reader.GetOrdinal("IsDeleted")) ? null : reader.GetInt32(reader.GetOrdinal("IsDeleted"))
            });
        }

        return categories;
    }

    public async Task<bool> UpdateCategory(Category category)
    {
        var sql = """
                  UPDATE "Category" SET "Name" = @Name, "CreatedBy" = @CreatedBy, "CreatedAt" = @CreatedAt, "UpdatedBy" =
                  @UpdatedBy, "UpdatedAt" = @UpdatedAt, "IsDeleted" = @IsDeleted WHERE "Id" = @Id
                  """;
        await OpenConnection();

        await using var command = CreateCommand(sql,
            new NpgsqlParameter("@Id", category.Id),
            new NpgsqlParameter("@Name", category.Name),
            new NpgsqlParameter("@CreatedBy", category.CreatedBy),
            new NpgsqlParameter("@CreatedAt", category.CreatedAt),
            new NpgsqlParameter("@UpdatedBy", category.UpdatedBy ?? (object)DBNull.Value),
            new NpgsqlParameter("@UpdatedAt", category.UpdatedAt ?? (object)DBNull.Value),
            new NpgsqlParameter("@IsDeleted", category.IsDeleted ?? (object)DBNull.Value)
        );

        await command.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<bool> AddCategory(Category category)
    {
        var sql = """
                  INSERT INTO "Category" ("Name", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted")
                  VALUES (@Name, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt, @IsDeleted)
                  """;
        await OpenConnection();

        await using var command = CreateCommand(sql,
            new NpgsqlParameter("@Name", category.Name),
            new NpgsqlParameter("@CreatedBy", category.CreatedBy),
            new NpgsqlParameter("@CreatedAt", category.CreatedAt),
            new NpgsqlParameter("@UpdatedBy", category.UpdatedBy ?? (object)DBNull.Value),
            new NpgsqlParameter("@UpdatedAt", category.UpdatedAt ?? (object)DBNull.Value),
            new NpgsqlParameter("@IsDeleted", category.IsDeleted ?? (object)DBNull.Value)
        );

        await command.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<bool> RemoveCategory(int id)
    {
        var categoryMovies = await _movieRepository.GetCategoryMovies(id);
        if (categoryMovies == null) return false;
        await OpenConnection();

        await using var transaction = await _connection.BeginTransactionAsync();
        try
        {
            foreach (var movie in categoryMovies.Movies)
            {
                await _movieRepository.UpdateMovie(movie);
            }

            var sqlDelete = """DELETE FROM "Category" WHERE "Id" = @Id""";
            await using var command = CreateCommand(sqlDelete, new NpgsqlParameter("@Id", id));

            await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}