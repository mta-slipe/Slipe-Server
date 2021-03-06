using MoonSharp.Interpreter;
using System;
using System.Runtime.Serialization;

namespace SlipeServer.Lua
{
    public class LuaArgumentException : LuaException
    {
        public string Name { get; }
        public Type ExpectedType { get; }
        public int Index { get; }
        public DataType ActualType { get; }

        public LuaArgumentException(string name, Type expectedType, int index, DataType actualType)
            : base($"Bad argument expected {expectedType.Name} got {actualType}")
        {
            Name = name;
            ExpectedType = expectedType;
            Index = index;
            ActualType = actualType;
        }
    }
}
