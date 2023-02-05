using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class SetElementDataRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; }
    public string Key { get; }
    public LuaValue LuaValue { get; }

    public SetElementDataRpcPacket()
    {
        this.Key = "";
        this.LuaValue = new();
    }

    public SetElementDataRpcPacket(ElementId elementId, string key, LuaValue luaValue)
    {
        this.ElementId = elementId;
        this.Key = key;
        this.LuaValue = luaValue;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_DATA);
        builder.Write(this.ElementId);
        builder.WriteCompressed((ushort)this.Key.Length);
        builder.WriteStringWithoutLength(this.Key);
        builder.Write(this.LuaValue);

        return builder.Build();
    }
}
