using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Mappers;
public class LuaValueMapper
{
    private readonly Dictionary<Type, Func<object, LuaValue>> strictlyDefinedClassMappers;
    private readonly Dictionary<Type, Func<object, LuaValue>> strictlyDefinedStructMappers;

    public LuaValueMapper()
    {
        this.strictlyDefinedClassMappers = new();
        this.strictlyDefinedStructMappers = new();
    }

    public void DefineMapper(Type type, Func<object, LuaValue> mapper)
    {
        this.strictlyDefinedClassMappers[type] = mapper;
    }

    public void DefineMapper<T>(Func<T, LuaValue> mapper) where T : class
    {
        this.strictlyDefinedClassMappers[typeof(T)] = (Func<object, LuaValue>)mapper;
    }

    public void DefineStructMapper<T>(Func<T, LuaValue> mapper) where T : struct
    {
        this.strictlyDefinedStructMappers[typeof(T)] = (x) => mapper((T)x);
    }

    public LuaValue Map(object? value)
    {
        if (value == null)
            return new LuaValue();

        if (this.strictlyDefinedClassMappers.TryGetValue(value.GetType(), out var mapper))
            return mapper.Invoke(value);

        if (this.strictlyDefinedStructMappers.TryGetValue(value.GetType(), out var structMapper))
            return structMapper.Invoke(value);

        if (value is ILuaMappable luaMappable)
            return Map(luaMappable);

        switch (value)
        {
            case int int32:
                return int32;
            case uint uint32:
                return uint32;
            case float single:
                return single;
            case double doublefloat:
                return doublefloat;
            case bool boolean:
                return boolean;
            case string text:
                return text;
            case Element element:
                return Map(element);
            case sbyte:
            case byte:
            case short:
            case ushort:
                return (int)value;
            case ulong:
            case long:
                throw new NotSupportedException($"Type {value.GetType()} not supported for Lua mapping");
        }

        if (value is IEnumerable<object> genericEnumerable)
            return Map(genericEnumerable);

        if (value is IEnumerable enumerable)
            return Map(enumerable);

        return MapBasedOnReflection(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LuaValue Map(Element element)
    {
        return new LuaValue(element.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LuaValue Map(ILuaMappable value)
    {
        return value.ToLuaValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LuaValue Map(IEnumerable<object> values)
    {
        return new LuaValue(values.Select(x => Map(x)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LuaValue Map(IEnumerable values)
    {
        var luaValues = new List<LuaValue>();
        foreach (var value in values)
            luaValues.Add(Map(value));

        return new LuaValue(luaValues);
    }

    private LuaValue MapBasedOnReflection(object value)
    {
        var dictionary = new Dictionary<LuaValue, LuaValue>();

        var type = value.GetType();

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(value);
            dictionary[property.Name] = propertyValue == null ? new LuaValue() : Map(propertyValue);
        }

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(value);
            dictionary[field.Name] = fieldValue == null ? new LuaValue() : Map(fieldValue);
        }

        return new LuaValue(dictionary);
    }
}
