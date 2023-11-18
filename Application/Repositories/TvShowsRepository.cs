using Infrastructure.Interfaces;
using Npgsql;

namespace Application.Repositories;

public class TvShowsRepository : BaseRepository, ITvShowsRepository
{
    private readonly NpgsqlConnection _connection;

    public TvShowsRepository(NpgsqlConnection connection): base(connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }
    
    public async Task<string> TvShowName(int id)
    {
        var result = "";
        var sql = "SELECT \"Name\" FROM \"TvShows\"  WHERE \"Id\" = @TvShowId";

        await using var command = CreateCommand(sql, new NpgsqlParameter("@TvShowId", id));
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            result = reader.GetString(reader.GetOrdinal("Name"));
        }

        return result;
    }
    
    

}
