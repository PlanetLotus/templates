using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;

namespace postgresmvc.Data
{
    public static class PostgresConnectionHelper
    {
        public static async Task<IDbConnection> GetConnection()
        {
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

            var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            return conn;
        }
    }
}
