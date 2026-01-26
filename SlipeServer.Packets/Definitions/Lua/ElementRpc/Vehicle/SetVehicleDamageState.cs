using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public sealed class SetVehicleDamageState(ElementId elementId, byte part, byte door, byte state, bool spawnFlyingComponent = false) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ElementId { get; set; } = elementId;
    public byte Part { get; set; } = part;
    public byte Door { get; set; } = door;
    public byte State { get; set; } = state;
    public bool SpawnFlyingComponent { get; set; } = spawnFlyingComponent;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_DAMAGE_STATE);
        builder.Write(this.ElementId);
        builder.Write(this.Part);
        builder.Write(this.Door);
        builder.Write(this.State);
        builder.Write(this.SpawnFlyingComponent);
        return builder.Build();
    }
}
