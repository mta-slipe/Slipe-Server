using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Scripting.Definitions;

public class SqlExecutor : ISqlExecutor, IDisposable
{
    private SqliteConnection? internalConnection;
    private readonly Lock internalLock = new();

    public Task<DbQueryResult> ExecuteQueryAsync(DbConnectionHandle connection, string query, List<(string Name, object? Value)> parameters)
    {
        return connection.EnqueueAsync(conn =>
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            AddParameters(cmd, parameters);
            return ReadQueryResult(cmd, connection.DatabaseType);
        });
    }

    public DbQueryResult ExecuteNonConnectionQuery(string query, List<(string Name, object? Value)> parameters)
    {
        var conn = GetInternalConnection();
        lock (this.internalLock)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            AddParameters(cmd, parameters);
            return ReadQueryResult(cmd, "sqlite");
        }
    }

    private SqliteConnection GetInternalConnection()
    {
        if (this.internalConnection == null)
        {
            lock (this.internalLock)
            {
                if (this.internalConnection == null)
                {
                    this.internalConnection = new SqliteConnection("Data Source=internal.db");
                    this.internalConnection.Open();
                }
            }
        }
        return this.internalConnection;
    }

    internal static DbQueryResult ReadQueryResult(IDbCommand cmd, string dbType)
    {
        var rows = new List<Dictionary<string, object?>>();
        long affectedRows;
        long lastInsertId = 0;

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
            affectedRows = reader.RecordsAffected;
        }

        if (affectedRows > 0)
        {
            using var lastIdCmd = cmd.Connection!.CreateCommand();
            lastIdCmd.CommandText = dbType == "mysql" ? "SELECT LAST_INSERT_ID()" : "SELECT last_insert_rowid()";
            lastInsertId = Convert.ToInt64(lastIdCmd.ExecuteScalar() ?? 0L);
        }

        return new DbQueryResult(rows, affectedRows < 0 ? 0 : affectedRows, lastInsertId);
    }

    private static void AddParameters(IDbCommand cmd, List<(string Name, object? Value)> parameters)
    {
        foreach (var item in parameters)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = item.Name;
            param.Value = item.Value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }
    }

    public void Dispose()
    {
        this.internalConnection?.Dispose();
    }
}
