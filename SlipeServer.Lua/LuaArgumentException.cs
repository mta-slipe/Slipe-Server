using MoonSharp.Interpreter;
using System;

namespace SlipeServer.Lua;

public class LuaArgumentException(string name, Type expectedType, int index, DataType actualType) : LuaException($"Bad argument expected {expectedType.Name} got {actualType}")
{
    public string Name { get; } = name;
    public Type ExpectedType { get; } = expectedType;
    public int Index { get; } = index;
    public DataType ActualType { get; } = actualType;
}
