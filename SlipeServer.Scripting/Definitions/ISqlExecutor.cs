using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlipeServer.Scripting.Definitions;

public interface ISqlExecutor
{
    Task<DbQueryResult> ExecuteQueryAsync(DbConnectionHandle connection, string query, List<(string Name, object? Value)> parameters);
    DbQueryResult ExecuteNonConnectionQuery(string query, List<(string Name, object? Value)> parameters);
}
