using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Mappers;

/// <summary>
/// Maps Lua values to any arbitrary C# class.
/// Support registring additional mappings for any specified type.
/// </summary>
public class FromLuaValueMapper
{
    private readonly Dictionary<Type, Func<LuaValue, object>> strictlyDefinedMappers;
    private readonly Dictionary<Type, Func<LuaValue, object>> implicitlyCastableTypes;
    private readonly IElementCollection elementCollection;

    public FromLuaValueMapper(IElementCollection elementCollection)
    {
        this.strictlyDefinedMappers = [];
        this.implicitlyCastableTypes = [];

        this.elementCollection = elementCollection;

        IndexImplicitlyCastableTypes();
    }

    private void IndexImplicitlyCastableTypes()
    {
        foreach (var method in typeof(LuaValue).GetMethods().Where(x => x.Name == "op_Explicit"))
            this.implicitlyCastableTypes[method.ReturnType] = (value) => method.Invoke(null, [value])!;
    }

    public void DefineMapper(Type type, Func<LuaValue, object> mapper)
    {
        this.strictlyDefinedMappers[type] = mapper;
    }

    public void DefineMapper<T>(Func<LuaValue, T> mapper) where T : class
    {
        this.strictlyDefinedMappers[typeof(T)] = mapper;
    }

    public void DefineMapper(Func<LuaValue, object> mapper, Type type)
    {
        this.strictlyDefinedMappers[type] = mapper;
    }

    public T? Map<T>(LuaValue luaValue)
    {
        return (T?)Map(typeof(T), luaValue);
    }

    public object? Map(Type type, LuaValue value)
    {
        if (this.strictlyDefinedMappers.TryGetValue(type, out var mapper))
            return mapper(value);

        else if (type.IsAssignableTo(typeof(ILuaValue)))
        {
            var instance = (ILuaValue)Activator.CreateInstance(type)!;
            instance.Parse(value);
            return instance;
        } 
        
        else if (type.IsAssignableTo(typeof(Element)) && value.ElementId.HasValue)
            return this.elementCollection.Get(value.ElementId!.Value);

        else if (this.implicitlyCastableTypes.ContainsKey(type))
            return this.implicitlyCastableTypes[type](value);

        else if (IsAssignableToGenericType(type, typeof(Dictionary<,>)) && value.TableValue != null)
            return value.TableValue.ToDictionary(
                x => Map(type.GenericTypeArguments.First(), x.Key) ?? new object(),
                x => Map(type.GenericTypeArguments.ElementAt(1), x.Value));

        else if (IsAssignableToGenericType(type, typeof(IEnumerable<>)) && value.TableValue != null)
            return value.TableValue.Values.Select(x => Map(type.GenericTypeArguments.First(), x));

        else if (type.IsEnum && value.IntegerValue.HasValue)
            return Enum.ToObject(type, Convert.ChangeType(value.IntegerValue.Value, Enum.GetUnderlyingType(type) ?? typeof(int)));

        else if (value.TableValue != null)
            return MapBasedOnReflection(type, value);

        else
            return null;
    }

    private object? MapBasedOnReflection(Type type, LuaValue value)
    {
        if (value.TableValue == null)
            return null;

        var instance = Activator.CreateInstance(type);
        if (instance == null)
            return null;

        var properties = instance.GetType().GetProperties().ToDictionary(x => x.Name, x => x);

        foreach (var (key, val) in value.TableValue)
        {
            if (key.StringValue != null && properties.TryGetValue(key.StringValue, out var property))
            {
                property.SetValue(instance, Map(property.PropertyType, val));
            }
        }

        return instance;
    }

    private bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;

        var baseType = givenType?.BaseType;
        if (baseType == null) 
            return false;

        return IsAssignableToGenericType(baseType, genericType);
    }
}
