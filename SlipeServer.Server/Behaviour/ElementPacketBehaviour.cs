using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.ElementCollections;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for handling element events and sending corresponding packets
/// </summary>
public class ElementPacketBehaviour
{
    private readonly MtaServer server;
    private readonly IElementCollection elementCollection;

    public ElementPacketBehaviour(MtaServer server, IElementCollection elementCollection)
    {
        this.server = server;
        this.elementCollection = elementCollection;
        server.ElementCreated += OnElementCreate;
    }

    private void OnElementCreate(Element element)
    {
        element.PositionChanged += RelayPositionChange;
        element.RotationChanged += RelayRotationChange;
        element.AlphaChanged += RelayAlphaChange;
        element.DimensionChanged += RelayDimensionChange;
        element.InteriorChanged += RelayInteriorChange;
        element.CallPropagationChanged += RelayCallPropagationChanged;
        element.CollisionEnabledhanged += RelayCollisionEnabledhanged;
        element.FrozenChanged += RelayElementFrozenChanged;
        element.Destroyed += RelayElementDestroy;
        element.Attached += RelayAttached;
        element.Detached += RelayDetached;
        element.AttachedOffsetChanged += RelayAttachedOffsetChanged;

        //if (element.ExistsForAllPlayers)
            this.server.BroadcastPacket(AddEntityPacketFactory.CreateAddEntityPacket(new Element[] { element }));
    }

    private void RelayElementFrozenChanged(Element sender, ElementChangedEventArgs<bool> args)
    {
        this.server.BroadcastPacket(ElementPacketFactory.CreateSetElementFrozen(args.Source, args.NewValue));
    }

    private void RelayCollisionEnabledhanged(Element sender, ElementChangedEventArgs<bool> args)
    {
        this.server.BroadcastPacket(ElementPacketFactory.CreateSetCollisionsEnabledPacket(args.Source, args.NewValue));
    }

    private void RelayCallPropagationChanged(Element sender, ElementChangedEventArgs<bool> args)
    {
        this.server.BroadcastPacket(ElementPacketFactory.CreateSetCallPropagationEnabledPacket(args.Source, args.NewValue));
    }

    private void RelayAttached(Element sender, ElementAttachedEventArgs args)
    {
        this.server.BroadcastPacket(ElementPacketFactory.CreateAttachElementPacket(args.Source, args.AttachedTo, args.OffsetPosition, args.OffsetRotation));
    }

    private void RelayDetached(Element sender, ElementDetachedEventArgs args)
    {
        this.server.BroadcastPacket(ElementPacketFactory.CreateDetachElementPacket(args.Source, args.Source.Position));
    }

    private void RelayAttachedOffsetChanged(Element sender, ElementAttachOffsetsChangedArgs args)
    {
        this.server.BroadcastPacket(ElementPacketFactory.CreateSetElementAttachedOffsetsPacket(args.Source, args.OffsetPosition, args.OffsetRotation));
    }

    private void RelayElementDestroy(Element element)
    {
        if (!(element is Player))
        {
            var packet = new RemoveEntityPacket();
            packet.AddEntity(element.Id);

            var players = this.elementCollection
                .GetByType<Player>(ElementType.Player)
                .Where(p => p != element);
            packet.SendTo(players);
        }
    }

    private void RelayPositionChange(object sender, ElementChangedEventArgs<Vector3> args)
    {
        if (!args.IsSync)
            this.server.BroadcastPacket(ElementPacketFactory.CreateSetPositionPacket(args.Source, args.NewValue));
    }

    private void RelayRotationChange(object sender, ElementChangedEventArgs<Vector3> args)
    {
        if (!args.IsSync)
            this.server.BroadcastPacket(ElementPacketFactory.CreateSetRotationPacket(args.Source, args.NewValue));
    }

    private void RelayAlphaChange(object sender, ElementChangedEventArgs<byte> args)
    {
        if (!args.IsSync)
            this.server.BroadcastPacket(ElementPacketFactory.CreateSetAlphaPacket(args.Source, args.NewValue));
    }

    private void RelayDimensionChange(object sender, ElementChangedEventArgs<ushort> args)
    {
        if (!args.IsSync)
            this.server.BroadcastPacket(ElementPacketFactory.CreateSetDimensionPacket(args.Source, args.NewValue));
    }

    private void RelayInteriorChange(object sender, ElementChangedEventArgs<byte> args)
    {
        if (!args.IsSync)
            this.server.BroadcastPacket(ElementPacketFactory.CreateSetInteriorPacket(args.Source, args.NewValue));
    }
}
