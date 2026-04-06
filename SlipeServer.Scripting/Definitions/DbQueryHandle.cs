using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlipeServer.Scripting.Definitions;

public class DbQueryResult(List<Dictionary<string, object?>> rows, long affectedRows, long lastInsertId)
{
    public List<Dictionary<string, object?>> Rows { get; } = rows;
    public long AffectedRows { get; } = affectedRows;
    public long LastInsertId { get; } = lastInsertId;
}

public class DbQueryHandle(Task<DbQueryResult> task)
{
    public Task<DbQueryResult> Task { get; } = task;
}
