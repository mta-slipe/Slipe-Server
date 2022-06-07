using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class RemoveEntityPacketFactory
{
    public static RemoveEntityPacket CreateRemoveEntityPacket(IEnumerable<Element> elements)
    {
        var packet = new RemoveEntityPacket();

        foreach (var element in elements)
            packet.AddEntity(element.Id);

        return packet;
    }
}
