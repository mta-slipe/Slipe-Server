using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Mappers;
public class FromLuaValueMapper
{
    private readonly Dictionary<Type, Func<LuaValue, object>> strictlyDefinedClassMappers;
    private readonly Dictionary<Type, Func<LuaValue, object>> strictlyDefinedStructMappers;
    private readonly Dictionary<Type, Func<LuaValue, object>> implicitlyCastableTypes;
    private readonly IElementCollection elementCollection;

    public FromLuaValueMapper(IElementCollection elementCollection)
    {
        this.strictlyDefinedClassMappers = new();
        this.strictlyDefinedStructMappers = new();
        this.implicitlyCastableTypes = new();

        this.elementCollection = elementCollection;

        IndexImplicitlyCastableTypes();
    }

    private void IndexImplicitlyCastableTypes()
    {
        foreach (var method in typeof(LuaValue).GetMethods().Where(x => x.Name == "op_Explicit"))
            this.implicitlyCastableTypes[method.ReturnType] = (value) => method.Invoke(null, new object[] { value })!;
    }

    public void DefineMapper(Type type, Func<LuaValue, object> mapper)
    {
        this.strictlyDefinedClassMappers[type] = mapper;
    }

    public void DefineMapper<T>(Func<LuaValue, T> mapper) where T : class
    {
        this.strictlyDefinedClassMappers[typeof(T)] = (Func<LuaValue, object>)mapper;
    }

    public void DefineStructMapper<T>(Func<LuaValue, T> mapper) where T : struct
    {
        this.strictlyDefinedStructMappers[typeof(T)] = (x) => mapper(x);
    }

    public object? Map(Type type, LuaValue value)
    {
        if (type.IsAssignableTo(typeof(ILuaValue)))
        {
            var instance = (ILuaValue)Activator.CreateInstance(type)!;
            instance.Parse(value);
            return instance;
        } else if (type.IsAssignableTo(typeof(Element)) && value.ElementId.HasValue)
            return this.elementCollection.Get(value.ElementId!.Value);

        else if (this.implicitlyCastableTypes.ContainsKey(type))
            return this.implicitlyCastableTypes[type](value);

        else if (type.IsAssignableTo(typeof(Dictionary<,>)) && value.TableValue != null)
            return value.TableValue.ToDictionary(
                x => Map(type.GenericTypeArguments.First(), x.Key) ?? new object(),
                x => Map(type.GenericTypeArguments.ElementAt(1), x.Value));

        else if (type.IsAssignableTo(typeof(IEnumerable<>)) && value.TableValue != null)
            return value.TableValue.Values.Select(x => Map(type.GenericTypeArguments.First(), value));

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
}
