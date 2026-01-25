using SlipeServer.Packets.Definitions.Lua;
using System;

namespace SlipeServer.Server.Mappers;

public interface IFromLuaValueMapper
{
    void DefineMapper(Func<LuaValue, object> mapper, Type type);
    void DefineMapper(Type type, Func<LuaValue, object> mapper);
    void DefineMapper<T>(Func<LuaValue, T> mapper) where T : class;
    object? Map(Type type, LuaValue value);
    T? Map<T>(LuaValue luaValue);
}