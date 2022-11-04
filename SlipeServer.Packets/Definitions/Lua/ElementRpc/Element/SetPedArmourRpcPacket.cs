using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public class SetPedArmourRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public byte TimeContext { get; set; }
    public float Armour { get; set; }

    public SetPedArmourRpcPacket()
    {

    }

    public SetPedArmourRpcPacket(uint elementId, byte timeContext, float armour)
    {
        this.ElementId = elementId;
        this.TimeContext = timeContext;
        this.Armour = armour;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.SET_PED_ARMOR);
        builder.WriteElementId(this.ElementId);

        builder.Write((byte)(MathF.Min(this.Armour, 100) * 1.25f));

        builder.Write(this.TimeContext);

        return builder.Build();
    }
}
