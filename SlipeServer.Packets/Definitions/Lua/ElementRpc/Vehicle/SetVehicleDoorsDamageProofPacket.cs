using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehicleDoorsDamageProofPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public bool IsDamageProof { get; set; }

    public SetVehicleDoorsDamageProofPacket(uint elementId, bool isDamageProof)
    {
        this.ElementId = elementId;
        this.IsDamageProof = isDamageProof;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_DOORS_UNDAMAGEABLE);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.IsDamageProof ? (byte)1 : (byte)0);
        return builder.Build();
    }
}
