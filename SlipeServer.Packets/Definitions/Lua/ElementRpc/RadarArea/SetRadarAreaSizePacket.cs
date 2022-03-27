using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;

public class SetRadarAreaSizePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; }
    public Vector2 Size { get; }

    public SetRadarAreaSizePacket(uint elementId, Vector2 size)
    {
        this.ElementId = elementId;
        this.Size = size;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_RADAR_AREA_SIZE);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.Size);

        return builder.Build();
    }
}
