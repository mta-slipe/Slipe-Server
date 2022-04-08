using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle;

public class SetVehicleDamageState : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint ElementId { get; set; }
    public byte Part { get; set; }
    public byte Door { get; set; }
    public byte State { get; set; }
    public bool SpawnFlyingComponent { get; set; }

    public SetVehicleDamageState(uint elementId, byte part, byte door, byte state, bool spawnFlyingComponent = false)
    {
        this.ElementId = elementId;
        this.Part = part;
        this.Door = door;
        this.State = state;
        this.SpawnFlyingComponent = spawnFlyingComponent;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_VEHICLE_DAMAGE_STATE);
        builder.WriteElementId(this.ElementId);
        builder.Write(this.Part);
        builder.Write(this.Door);
        builder.Write(this.State);
        builder.Write(this.SpawnFlyingComponent);
        return builder.Build();
    }
}
