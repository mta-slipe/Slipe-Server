using Microsoft.Data.Sqlite;
using MySqlConnector;
using System;
using System.Data;
using System.Threading;

namespace SlipeServer.Scripting.Definitions;

public class DbConnectionHandle(string databaseType, string connectionString) : IDisposable
{
    public string DatabaseType { get; } = databaseType;
    public string ConnectionString { get; } = connectionString;

    private IDbConnection? connection;
    private readonly Lock connectionLock = new();

    public T Execute<T>(Func<IDbConnection, T> action)
    {
        lock (this.connectionLock)
        {
            if (this.connection == null || this.connection.State == ConnectionState.Closed || this.connection.State == ConnectionState.Broken)
            {
                this.connection?.Dispose();
                this.connection = this.CreateRawConnection();
                this.connection.Open();
            }
            return action(this.connection);
        }
    }

    private IDbConnection CreateRawConnection()
    {
        return this.DatabaseType switch
        {
            "sqlite" => new SqliteConnection(this.ConnectionString),
            "mysql" => new MySqlConnection(this.ConnectionString),
            _ => throw new NotSupportedException($"Database type '{this.DatabaseType}' is not supported. Supported types: sqlite, mysql")
        };
    }

    public void Dispose()
    {
        lock (this.connectionLock)
        {
            this.connection?.Dispose();
            this.connection = null;

            GC.SuppressFinalize(this);
        }
    }
}
