using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehicleEngineStateRpcPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public bool State { get; set; }

    public SetVehicleEngineStateRpcPacket(uint elementId, bool state)
    {
        this.ElementId = elementId;
        this.State = state;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_VEHICLE_ENGINE_STATE);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.State);

        return builder.Build();
    }
}
