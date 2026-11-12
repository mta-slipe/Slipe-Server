using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetElementAngularVelocityRpcPacket(ElementId element, Vector3 velocity) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability  Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId Element { get; } = element;
    public Vector3 Velocity { get; } = velocity;
    
    public override void Read(byte[] bytes) => throw new System.NotImplementedException();
    
    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_ANGULAR_VELOCITY);
        builder.Write(this.Element);
        builder.Write(this.Velocity);

        return builder.Build();
    }
}
