using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class RemoveElementDataRpcPacket(ElementId theElement, string key) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId theElement { get; } = theElement;
    public string key { get; } = key;

    public override void Read(byte[] bytes) => throw new System.NotImplementedException();

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.REMOVE_ELEMENT_DATA);
        // We should check that if the server cancelled the remove element data in the "onElementDataChange" event
        // and thus cancel this (in the logic layer)
        ushort keyLength = (ushort)this.key.Length;
        builder.WriteCompressed(keyLength);
        builder.WriteStringWithoutLength(this.key);
        // in the mtasa repo it had a comment regarding this field is unused and was recursive flag
        // i'll check later to see if client even tries to read this, if it doesn't, i'll remove this one
        builder.Write(false);
        
        return builder.Build();
    }
}
