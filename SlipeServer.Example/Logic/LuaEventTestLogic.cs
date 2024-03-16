using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;
using System.Numerics;

namespace SlipeServer.Example.Logic;

internal struct DataStruct
{
    public string Foo { get; set; } = "Foo";
    public int Bar { get; set; } = 1;

    public DataStruct()
    {

    }
}

internal struct LuaMappableStruct : ILuaMappable
{
    public LuaValue ToLuaValue()
    {
        return new LuaValue("Lua mapped!");
    }
}

internal struct ExplicitStruct
{

}

public class LuaEventTestLogic
{
    private readonly LuaEventService luaEventService;

    public LuaEventTestLogic(
        CommandService commandService,
        LuaEventService luaEventService,
        LuaValueMapper mapper)
    {
        this.luaEventService = luaEventService;

        mapper.DefineStructMapper<ExplicitStruct>(x => new LuaValue("Explicit!"));

        commandService.AddCommand("sendvector").Triggered += SendLuaVector;
        commandService.AddCommand("sendvectorlist").Triggered += SendLuaVectorList;
        commandService.AddCommand("senddata").Triggered += SendDataStruct;
        commandService.AddCommand("sendmapped").Triggered += SendLuaMappable;
        commandService.AddCommand("sendexplicit").Triggered += SendExplicitStruct;
    }

    private void SendLuaVector(object? sender, CommandTriggeredEventArgs e)
    {
        this.luaEventService.TriggerEventFor(
            e.Player,
            "Slipe.Test.ClientEvent",
            e.Player,
            new Vector3(0, 1, 2));
    }

    private void SendLuaVectorList(object? sender, CommandTriggeredEventArgs e)
    {
        this.luaEventService.TriggerEventFor(
            e.Player,
            "Slipe.Test.ClientEvent",
            e.Player,
            new Vector3[]
            {
                e.Player.Position,
                e.Player.Rotation,
                e.Player.Forward,
                e.Player.Right,
                e.Player.Up
            });
    }

    private void SendLuaMappable(object? sender, CommandTriggeredEventArgs e)
    {
        this.luaEventService.TriggerEventFor(
            e.Player,
            "Slipe.Test.ClientEvent",
            e.Player,
            new LuaMappableStruct());
    }

    private void SendDataStruct(object? sender, CommandTriggeredEventArgs e)
    {
        this.luaEventService.TriggerEventFor(
            e.Player,
            "Slipe.Test.ClientEvent",
            e.Player,
            new DataStruct());
    }

    private void SendExplicitStruct(object? sender, CommandTriggeredEventArgs e)
    {
        this.luaEventService.TriggerEventFor(
            e.Player,
            "Slipe.Test.ClientEvent",
            e.Player,
            new ExplicitStruct());
    }
}
