using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetElementAngularVelocityRpcPacket(ElementId theElement, Vector3 velocity) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability  Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId theElement { get; } = theElement;
    public Vector3 velocity { get; } = velocity;
    
    public override void Read(byte[] bytes) => throw new System.NotImplementedException();
    
    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_ANGULAR_VELOCITY);
        builder.Write(this.theElement);
        builder.Write(this.velocity.X);
        builder.Write(this.velocity.Y);
        builder.Write(this.velocity.Z);

        return builder.Build();
    }
}
