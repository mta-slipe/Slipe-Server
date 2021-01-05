using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Numerics;

namespace SlipeServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsible for handling element events and sending corresponding packets
    /// </summary>
    public class ElementPacketBehaviour
    {
        private readonly MtaServer server;

        public ElementPacketBehaviour(MtaServer server)
        {
            this.server = server;

            server.ElementCreated += OnElementCreate;
        }

        private void OnElementCreate(Element element)
        {
            element.PositionChanged += RelayPositionChange;
            element.RotationChanged += RelayRotationChange;
            element.AlphaChanged += RelayAlphaChange;
            element.DimensionChanged += RelayDimensionChange;
            element.InteriorChanged += RelayInteriorChange;
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
