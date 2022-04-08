using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.Factories;
using System.Collections.Generic;

namespace SlipeServer.Server.Extensions;

public static class ElementCollectionExtensions
{
    public static void CreateFor(this IEnumerable<Element> elements, IEnumerable<Player> players)
    {
        AddEntityPacketFactory.CreateAddEntityPacket(elements).SendTo(players);
    }

    public static void DestroyFor(this IEnumerable<Element> elements, IEnumerable<Player> players)
    {
        RemoveEntityPacketFactory.CreateRemoveEntityPacket(elements).SendTo(players);
    }
}
