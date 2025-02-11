using Npgsql;

namespace Persistence;

public class DapperConnectionFactory(string connectionString)
{
    public NpgsqlConnection Create()
    {
        return new NpgsqlConnection(connectionString);
    }
}