using Domain.Dtos;
using Npgsql;

namespace Application.Repositories;

public class TvShowsRepository
{
    private readonly NpgsqlConnection _connection;

    public TvShowsRepository(NpgsqlConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }
    
    public async Task<string> TvShowName(int id)
    {
        var result = "";
        var sql = "SELECT \"Name\" FROM \"TvShows\"  WHERE \"Id\" = @TvShowId";

        await using var command = CreateCommand(sql);
        command.Parameters.AddWithValue("@TvShowId", id);
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            result = reader.GetString(reader.GetOrdinal("Name"));
        }

        return result;
    }
    
    private NpgsqlCommand CreateCommand(string query)
    {
        return new NpgsqlCommand(query, _connection);
    }

}
