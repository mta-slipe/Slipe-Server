using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.Factories;

namespace SlipeServer.Server.Behaviour
{
    public class ElementUpdateBehaviour
    {
        private readonly MtaServer server;

        public ElementUpdateBehaviour(MtaServer server)
        {
            this.server = server;

            server.ElementCreated += OnElementCreate;
        }

        private void OnElementCreate(Element element)
        {
            element.PositionChanged += (sender, args) =>
            {
                if (!args.IsSync)
                {
                    this.server.BroadcastPacket(ElementPacketFactory.CreateSetPositionPacket(args.Source, args.NewValue));
                }
            };
            element.RotationChanged += (sender, args) =>
            {
                if (!args.IsSync && (args.Source is Ped || args.Source is Vehicle || args.Source is WorldObject))
                {
                    this.server.BroadcastPacket(ElementPacketFactory.CreateSetRotationPacket(args.Source, args.NewValue));
                }
            };
        }
    }
}
