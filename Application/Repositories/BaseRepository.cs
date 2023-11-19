using System.Data;
using Npgsql;

namespace Application.Repositories;

public class BaseRepository : IDisposable, IAsyncDisposable
{
    protected readonly NpgsqlConnection _connection;

    protected BaseRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    protected async Task<ConnectionState> OpenConnection()
    {
        switch (_connection.State)
        {
            case ConnectionState.Closed:
                await _connection.OpenAsync();
                return ConnectionState.Open;
            case ConnectionState.Open:
                return ConnectionState.Open;
            case ConnectionState.Connecting:
                return ConnectionState.Connecting;
            case ConnectionState.Executing:
                return ConnectionState.Executing;
            case ConnectionState.Fetching:
                return ConnectionState.Fetching;
            case ConnectionState.Broken:
                await _connection.CloseAsync();
                await _connection.OpenAsync();
                return ConnectionState.Broken;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    protected NpgsqlCommand CreateCommand(string sql, params NpgsqlParameter[] parameters)
    {
        var command = new NpgsqlCommand(sql, _connection);
        command.Parameters.AddRange(parameters);
        return command;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
