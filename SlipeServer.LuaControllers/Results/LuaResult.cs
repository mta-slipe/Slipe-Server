namespace SlipeServer.LuaControllers.Results;

public class LuaResult
{
    public string EventSuffix { get; }

    public LuaResult(string eventSuffix)
    {
        this.EventSuffix = eventSuffix;
    }

    public static LuaResult Success() => new(".Success");
    public static LuaResult Warning() => new(".Warning");
    public static LuaResult Invalid() => new(".Invalid");
    public static LuaResult Error() => new(".Error");
}


public class LuaResult<T> : LuaResult
{
    public T Data { get; }

    public LuaResult(string eventSuffix, T data) : base(eventSuffix)
    {
        this.Data = data;
    }

    public static LuaResult<T> Success(T data) => new(".Success", data);
    public static LuaResult<T> Warning(T data) => new(".Warning", data);
    public static LuaResult<T> Invalid(T data) => new(".Invalid", data);
    public static LuaResult<T> Error(T data) => new(".Error", data);
}
