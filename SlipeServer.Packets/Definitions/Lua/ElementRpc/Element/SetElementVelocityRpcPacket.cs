using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetElementVelocityRpcPacket(ElementId theElement, Vector3 velocity) : Packet
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
        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_VELOCITY);
        builder.Write(theElement);
        builder.Write(velocity.X);
        builder.Write(velocity.Y);
        builder.Write(velocity.Z);
        
        return builder.Build();
    }
}
