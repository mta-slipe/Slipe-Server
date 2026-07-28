using SlipeServer.Scripting.Definitions;

namespace SlipeServer.Scripting.Lua.Tests.Mocks;

/// <summary>
/// A mock ISqlExecutor that returns empty results by default.
/// Specific query responses can be set up via <see cref="SetupQuery"/>.
/// </summary>
public class MockSqlExecutor : ISqlExecutor
{
    private static readonly DbQueryResult EmptyResult = new([], 0, 0);

    private readonly Dictionary<string, Func<List<(string Name, object? Value)>, DbQueryResult>> querySetups = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>All queries that were executed, in order.</summary>
    public List<string> ExecutedQueries { get; } = [];

    /// <summary>
    /// Register a specific result for queries that contain <paramref name="querySubstring"/>.
    /// </summary>
    public MockSqlExecutor SetupQuery(string querySubstring, DbQueryResult result)
    {
        this.querySetups[querySubstring] = _ => result;
        return this;
    }

    /// <summary>
    /// Register a factory for queries that contain <paramref name="querySubstring"/>.
    /// The factory receives the bound parameters and returns the result.
    /// </summary>
    public MockSqlExecutor SetupQuery(string querySubstring, Func<List<(string Name, object? Value)>, DbQueryResult> factory)
    {
        this.querySetups[querySubstring] = factory;
        return this;
    }

    public Task<DbQueryResult> ExecuteQueryAsync(DbConnectionHandle connection, string query, List<(string Name, object? Value)> parameters)
    {
        return Task.FromResult(Resolve(query, parameters));
    }

    public DbQueryResult ExecuteNonConnectionQuery(string query, List<(string Name, object? Value)> parameters)
    {
        return Resolve(query, parameters);
    }

    private DbQueryResult Resolve(string query, List<(string Name, object? Value)> parameters)
    {
        ExecutedQueries.Add(query);
        foreach (var (substring, factory) in this.querySetups)
        {
            if (query.Contains(substring, StringComparison.OrdinalIgnoreCase))
                return factory(parameters);
        }
        return EmptyResult;
    }
}
