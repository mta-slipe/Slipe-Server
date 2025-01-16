using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Lua.Event;

public sealed class LuaEventPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_EVENT;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string Name { get; set; } = string.Empty;
    public ElementId ElementId { get; set; }
    public IEnumerable<LuaValue> LuaValues { get; set; } = Array.Empty<LuaValue>();

    public LuaEventPacket()
    {

    }

    public LuaEventPacket(string name, ElementId elementId, IEnumerable<LuaValue> luaValues)
    {
        this.Name = name;
        this.ElementId = elementId;
        this.LuaValues = luaValues;
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();

        builder.WriteCompressed((ushort)this.Name.Length);
        builder.WriteStringWithoutLength(this.Name);
        builder.Write(this.ElementId);

        builder.Write(this.LuaValues);

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {
        PacketReader reader = new PacketReader(bytes);

        ushort length = reader.GetCompressedUint16();
        this.Name = reader.GetStringCharacters(length);
        this.ElementId = reader.GetElementId();

        this.LuaValues = reader.GetLuaValues();
    }
}
