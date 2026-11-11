using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

// the 'name' is basically referring to a new element ID
public sealed class SetElementNameRpcPacket(ElementId theElement, ElementId newName) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability  Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId theElement { get; } = theElement;
    public ElementId newName { get; } = newName;
    
    public override void Read(byte[] bytes) => throw new System.NotImplementedException();

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        ushort idLength = (ushort)this.newName.ToString().Length;
        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_NAME);
        builder.Write(this.theElement);
        builder.Write(idLength);
        builder.WriteCapped(this.newName.Value, idLength);
        
        return builder.Build();
    }
}
