using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsible for handling element events and sending corresponding packets
    /// </summary>
    public class ElementPacketBehaviour
    {
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;

        public ElementPacketBehaviour(MtaServer server, IElementRepository elementRepository)
        {
            this.server = server;
            this.elementRepository = elementRepository;
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
            element.Destroyed += RelayElementDestroy;

            this.server.BroadcastPacket(AddEntityPacketFactory.CreateAddEntityPacket(new Element[] { element }));
        }

        private void RelayCollisionEnabledhanged(Element sender, ElementChangedEventArgs<bool> args)
        {
            this.server.BroadcastPacket(ElementPacketFactory.CreateSetCallPropagationEnabledPacket(args.Source, args.NewValue));
        }
        
        private void RelayCallPropagationChanged(Element sender, ElementChangedEventArgs<bool> args)
        {
            this.server.BroadcastPacket(ElementPacketFactory.CreateSetCollisionsEnabledPacket(args.Source, args.NewValue));
        }

        private void RelayElementDestroy(Element element)
        {
            if (!(element is Player))
            {
                var packet = new RemoveEntityPacket();
                packet.AddEntity(element.Id);

                var players = this.elementRepository
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
}
