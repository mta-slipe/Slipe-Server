using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SlipeServer.Server.Mappers;

public interface ILuaValueMapper
{
    void DefineMapper(Type type, Func<object, LuaValue> mapper);
    void DefineMapper<T>(Func<T, LuaValue> mapper) where T : class;
    void DefineStructMapper<T>(Func<T, LuaValue> mapper) where T : struct;
    LuaValue Map(Element element);
    LuaValue Map(IDictionary source);
    LuaValue Map(IEnumerable? values);
    LuaValue Map(IEnumerable<object> values);
    LuaValue Map(ILuaMappable value);
    LuaValue Map(object? value);
}