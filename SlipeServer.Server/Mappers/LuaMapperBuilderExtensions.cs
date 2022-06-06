using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.ServerBuilders;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Mappers;
public static class LuaMapperBuilderExtensions
{
    public static void AddLuaMapping<T>(this ServerBuilder builder, Func<T, LuaValue> mapper) where T: class
    {
        builder.AddBuildStep((x) =>
        {
            x.GetRequiredService<LuaValueMapper>().DefineMapper<T>(mapper);
        }, ServerBuildStepPriority.Low);
    }
    public static void AddStructLuaMapping<T>(this ServerBuilder builder, Func<T, LuaValue> mapper) where T: struct
    {
        builder.AddBuildStep((x) =>
        {
            x.GetRequiredService<LuaValueMapper>().DefineStructMapper<T>(mapper);
        }, ServerBuildStepPriority.Low);
    }

    public static void AddLuaMapping(this ServerBuilder builder, Type type, Func<object, LuaValue> mapper)
    {
        builder.AddBuildStep((x) =>
        {
            x.GetRequiredService<LuaValueMapper>().DefineMapper(type, mapper);
        }, ServerBuildStepPriority.Low);
    }

    public static void AddVectorMappings(this ServerBuilder builder)
    {
        builder.AddStructLuaMapping<Vector2>(x => new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = x.X,
            ["Y"] = x.Y,
        });
        builder.AddStructLuaMapping<Vector3>(x => new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = x.X,
            ["Y"] = x.Y,
            ["Z"] = x.Z,
        });
        builder.AddStructLuaMapping<Vector4>(x => new Dictionary<LuaValue, LuaValue>()
        {
            ["X"] = x.X,
            ["Y"] = x.Y,
            ["Z"] = x.Z,
            ["W"] = x.W,
        });
    }

    public static void AddDefaultLuaMappings(this ServerBuilder builder)
    {
        builder.AddVectorMappings();
    }
}
