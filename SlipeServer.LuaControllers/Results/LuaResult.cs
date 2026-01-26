namespace SlipeServer.LuaControllers.Results;

public class LuaResult(string eventSuffix)
{
    public string EventSuffix { get; } = eventSuffix;

    public static LuaResult Success() => new(".Success");
    public static LuaResult Warning() => new(".Warning");
    public static LuaResult Invalid() => new(".Invalid");
    public static LuaResult Error() => new(".Error");
}


public class LuaResult<T>(string eventSuffix, T data) : LuaResult(eventSuffix)
{
    public T Data { get; } = data;

    public static LuaResult<T> Success(T data) => new(".Success", data);
    public static LuaResult<T> Warning(T data) => new(".Warning", data);
    public static LuaResult<T> Invalid(T data) => new(".Invalid", data);
    public static LuaResult<T> Error(T data) => new(".Error", data);
}
