using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling.Factories;

namespace MtaServer.Server.Behaviour
{
    public class ElementUpdateBehaviour
    {
        private readonly MtaServer server;

        public ElementUpdateBehaviour(MtaServer server)
        {
            this.server = server;

            Element.Created += OnElementCreate;
        }

        private void OnElementCreate(Element element)
        {
            element.PositionChange += (args) =>
            {
                if (!args.IsSync)
                {
                    this.server.BroadcastPacket(ElementPacketFactory.CreateSetPositionPacket(args.Source, args.NewValue));
                }
            };
        }
    }
}
