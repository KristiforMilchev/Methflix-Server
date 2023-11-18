using Npgsql;

namespace Application.Repositories;

public class BaseRepository
{
    protected readonly NpgsqlConnection _connection;

    protected BaseRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }
    
    protected NpgsqlCommand CreateCommand(string sql, params NpgsqlParameter[] parameters)
    {
        var command = new NpgsqlCommand(sql, _connection);
        command.Parameters.AddRange(parameters);
        return command;
    }
}
