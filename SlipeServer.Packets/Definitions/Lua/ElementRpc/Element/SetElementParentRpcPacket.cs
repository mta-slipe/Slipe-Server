using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class SetElementParentRpcPacket(ElementId theElement, ElementId theParent) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId theElement { get; } = theElement;
    public ElementId theParent { get; } = theParent;
    public override void Read(byte[] bytes) => throw new System.NotImplementedException();

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_PARENT);
        // in the c++ codebase from mtasa-blue, we have some checks regarding the new parent element is valid or not
        // Also there is a check that relates to issue #8766 from mtasa-blue repo which indicated that there was a problem
        // resetting the element parent of player element, i think those checks can't be done here and need to be done
        // by the layer that wants to invoke and send this packet
        builder.Write(theElement);
        builder.Write(theParent);

        return builder.Build();
    }
}
