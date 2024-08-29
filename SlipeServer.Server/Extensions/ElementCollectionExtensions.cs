using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.Factories;
using System.Collections.Generic;

namespace SlipeServer.Server.Extensions;

public static class ElementCollectionExtensions
{
    /// <summary>
    /// Sends packets to create a set of elements to a set of players
    /// Do note that elements will be required to have an id assigned for this to work properly
    /// </summary>
    public static void CreateFor(this IEnumerable<Element> elements, IEnumerable<Player> players)
    {
        AddEntityPacketFactory.CreateAddEntityPacket(elements).SendTo(players);
    }

    /// <summary>
    /// Sends packets to create a set of elements to a set of players
    /// Do note that elements will be required to have an id assigned for this to work properly
    /// </summary>
    public static void CreateFor(this IEnumerable<Element> elements, Player player)
    {
        CreateFor(elements, [player]);
    }

    /// <summary>
    /// Sends packets to destroy a set of elements to a set of players
    /// Do note that elements will be required to have an id assigned for this to work properly
    /// </summary>
    public static void DestroyFor(this IEnumerable<Element> elements, IEnumerable<Player> players)
    {
        RemoveEntityPacketFactory.CreateRemoveEntityPacket(elements).SendTo(players);
    }

    /// <summary>
    /// Sends packets to destroy a set of elements to a set of players
    /// Do note that elements will be required to have an id assigned for this to work properly
    /// </summary>
    public static void DestroyFor(this IEnumerable<Element> elements, Player player)
    {
        DestroyFor(elements, [player]);
    }

    /// <summary>
    /// Sends packets to create a set of elements to a set of players
    /// Do note that elements will be required to have an id assigned for this to work properly
    /// </summary>
    public static void AssociateWith(this IEnumerable<Element> elements, IEnumerable<Player> players)
    {
        foreach (var element in elements)
            foreach (var player in players)
                element.AssociateWith(player);
    }

    /// <summary>
    /// Sends packets to create a set of elements to a set of players
    /// Do note that elements will be required to have an id assigned for this to work properly
    /// </summary>
    public static void AssociateWith(this IEnumerable<Element> elements, Player player)
    {
        foreach (var element in elements)
            element.AssociateWith(player);
    }

    /// <summary>
    /// Sends packets to destroy a set of elements to a set of players
    /// Do note that elements will be required to have an id assigned for this to work properly
    /// </summary>
    public static void RemoveFrom(this IEnumerable<Element> elements, IEnumerable<Player> players)
    {
        foreach (var element in elements)
            foreach (var player in players)
                element.RemoveFrom(player);
    }

    /// <summary>
    /// Sends packets to destroy a set of elements to a set of players
    /// Do note that elements will be required to have an id assigned for this to work properly
    /// </summary>
    public static void RemoveFrom(this IEnumerable<Element> elements, Player player)
    {
        foreach (var element in elements)
            element.RemoveFrom(player);
    }
}
