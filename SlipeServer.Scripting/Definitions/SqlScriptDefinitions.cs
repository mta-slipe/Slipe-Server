using Microsoft.Data.Sqlite;
using MySqlConnector;
using SlipeServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Scripting.Definitions;

public class SqlScriptDefinitions : IDisposable
{
    private SqliteConnection? internalConnection;
    private readonly Lock internalLock = new();

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

    [ScriptFunctionDefinition("dbConnect")]
    public DbConnectionHandle DbConnect(string databaseType, string host, string username = "", string password = "", string options = "")
    {
        var connectionString = BuildConnectionString(databaseType.ToLowerInvariant(), host, username, password, options);
        return new DbConnectionHandle(databaseType.ToLowerInvariant(), connectionString);
    }

    [ScriptFunctionDefinition("dbQuery")]
    public DbQueryHandle DbQuery(ScriptCallbackDelegateWrapper? callback, DbConnectionHandle connection, string query, params LuaValue[] queryParams)
    {
        var (preparedQuery, parameters) = PrepareParameterizedQuery(query, queryParams);
        var task = Task.Run(() => ExecuteQuery(connection, preparedQuery, parameters));
        var handle = new DbQueryHandle(task);

        if (callback != null)
        {
            _ = Task.Run(async () =>
            {
                await task;
                callback.CallbackDelegate(handle);
            });
        }

        return handle;
    }

    [ScriptFunctionDefinition("dbExec")]
    public bool DbExec(DbConnectionHandle connection, string query, params LuaValue[] queryParams)
    {
        var (preparedQuery, parameters) = PrepareParameterizedQuery(query, queryParams);
        connection.Execute(conn =>
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = preparedQuery;
            AddParameters(cmd, parameters);
            cmd.ExecuteNonQuery();
            return true;
        });
        return true;
    }

    [ScriptFunctionDefinition("dbPoll")]
    public DbQueryResult? DbPoll(DbQueryHandle queryHandle, int timeout, bool multipleResults = false)
    {
        var task = queryHandle.Task;

        if (timeout == -1)
        {
            task.Wait();
            return task.Result;
        }
        else if (timeout == 0)
        {
            return task.IsCompleted ? task.Result : null;
        }
        else
        {
            var completed = task.Wait(timeout);
            return completed ? task.Result : null;
        }
    }

    [ScriptFunctionDefinition("dbFree")]
    public bool DbFree(DbQueryHandle queryHandle)
    {
        return true;
    }

    [ScriptFunctionDefinition("dbPrepareString")]
    public string DbPrepareString(DbConnectionHandle connection, string query, params LuaValue[] queryParams)
    {
        return SubstituteLiterals(query, queryParams, connection.DatabaseType);
    }

    [ScriptFunctionDefinition("executeSQLQuery")]
    public DbQueryResult ExecuteSQLQuery(string query, params LuaValue[] queryParams)
    {
        var (preparedQuery, parameters) = PrepareParameterizedQuery(query, queryParams);
        var conn = GetInternalConnection();
        lock (this.internalLock)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = preparedQuery;
            AddParameters(cmd, parameters);
            return ReadQueryResult(cmd, "sqlite");
        }
    }

    private static DbQueryResult ExecuteQuery(DbConnectionHandle connectionHandle, string query, List<(string Name, object? Value)> parameters)
    {
        return connectionHandle.Execute(conn =>
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            AddParameters(cmd, parameters);
            return ReadQueryResult(cmd, connectionHandle.DatabaseType);
        });
    }

    private static DbQueryResult ReadQueryResult(IDbCommand cmd, string dbType)
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

    private static (string Query, List<(string Name, object? Value)> Parameters) PrepareParameterizedQuery(string query, LuaValue[] args)
    {
        var sb = new StringBuilder();
        var parameters = new List<(string, object?)>();
        var argIndex = 0;
        var i = 0;

        while (i < query.Length)
        {
            if (i + 1 < query.Length && query[i] == '?' && query[i + 1] == '?')
            {
                if (argIndex < args.Length)
                {
                    var identifier = args[argIndex++].StringValue ?? "";
                    sb.Append('`');
                    sb.Append(identifier.Replace("`", "``"));
                    sb.Append('`');
                }
                i += 2;
            }
            else if (query[i] == '?')
            {
                var paramName = $"@p{parameters.Count}";
                sb.Append(paramName);
                parameters.Add((paramName, argIndex < args.Length ? LuaValueToObject(args[argIndex++]) : null));
                i++;
            }
            else
            {
                sb.Append(query[i]);
                i++;
            }
        }

        return (sb.ToString(), parameters);
    }

    private static string SubstituteLiterals(string query, LuaValue[] args, string dbType)
    {
        var sb = new StringBuilder();
        var argIndex = 0;
        var i = 0;

        while (i < query.Length)
        {
            if (i + 1 < query.Length && query[i] == '?' && query[i + 1] == '?')
            {
                if (argIndex < args.Length)
                {
                    var identifier = args[argIndex++].StringValue ?? "";
                    sb.Append('`');
                    sb.Append(identifier.Replace("`", "``"));
                    sb.Append('`');
                }
                i += 2;
            }
            else if (query[i] == '?')
            {
                if (argIndex < args.Length)
                    sb.Append(EscapeLiteral(args[argIndex++], dbType));
                i++;
            }
            else
            {
                sb.Append(query[i]);
                i++;
            }
        }

        return sb.ToString();
    }

    private static string EscapeLiteral(LuaValue value, string dbType)
    {
        if (value.IsNil)
            return "NULL";
        if (value.BoolValue.HasValue)
            return value.BoolValue.Value ? "1" : "0";
        if (value.IntegerValue.HasValue)
            return value.IntegerValue.Value.ToString();
        if (value.FloatValue.HasValue)
            return value.FloatValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (value.DoubleValue.HasValue)
            return value.DoubleValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (value.StringValue != null)
        {
            var escaped = value.StringValue
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\0", "\\0")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\x1a", "\\Z");
            return $"'{escaped}'";
        }
        return "NULL";
    }

    private static object? LuaValueToObject(LuaValue value)
    {
        if (value.IsNil) return null;
        if (value.BoolValue.HasValue) return value.BoolValue.Value;
        if (value.IntegerValue.HasValue) return value.IntegerValue.Value;
        if (value.FloatValue.HasValue) return (double)value.FloatValue.Value;
        if (value.DoubleValue.HasValue) return value.DoubleValue.Value;
        if (value.StringValue != null) return value.StringValue;
        return null;
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

    private static string BuildConnectionString(string dbType, string host, string username, string password, string options)
    {
        return dbType switch
        {
            "sqlite" => BuildSqliteConnectionString(host, options),
            "mysql" => BuildMySqlConnectionString(host, username, password, options),
            _ => throw new NotSupportedException($"Database type '{dbType}' is not supported. Supported types: sqlite, mysql")
        };
    }

    private static string BuildSqliteConnectionString(string host, string options)
    {
        string dataSource;
        if (host == ":memory:" || host.StartsWith(":memory:", StringComparison.OrdinalIgnoreCase))
            dataSource = ":memory:";
        else if (host.StartsWith(":", StringComparison.Ordinal))
            dataSource = host.TrimStart(':').TrimStart('/');
        else
            dataSource = host;

        var builder = new SqliteConnectionStringBuilder { DataSource = dataSource };

        foreach (var option in ParseOptions(options))
        {
            if (option.Key == "mode")
                builder.Mode = option.Value.ToLower() == "memory" ? SqliteOpenMode.Memory : SqliteOpenMode.ReadWriteCreate;
        }

        return builder.ToString();
    }

    private static string BuildMySqlConnectionString(string host, string username, string password, string options)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            UserID = username,
            Password = password
        };

        foreach (var option in ParseOptions(host))
        {
            switch (option.Key.ToLower())
            {
                case "dbname":
                case "database":
                    builder.Database = option.Value;
                    break;
                case "host":
                case "server":
                    builder.Server = option.Value;
                    break;
                case "port":
                    builder.Port = uint.Parse(option.Value);
                    break;
                case "charset":
                    builder.CharacterSet = option.Value;
                    break;
                case "unix_socket":
                    builder.ConnectionProtocol = MySqlConnectionProtocol.UnixSocket;
                    builder.Server = option.Value;
                    break;
            }
        }

        foreach (var option in ParseOptions(options))
        {
            switch (option.Key.ToLower())
            {
                case "multi_statements":
                    builder.AllowUserVariables = option.Value == "1";
                    break;
            }
        }

        return builder.ToString();
    }

    private static IEnumerable<(string Key, string Value)> ParseOptions(string options)
    {
        foreach (var part in options.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2)
                yield return (kv[0].Trim(), kv[1].Trim());
        }
    }

    public void Dispose()
    {
        this.internalConnection?.Dispose();
    }
}
