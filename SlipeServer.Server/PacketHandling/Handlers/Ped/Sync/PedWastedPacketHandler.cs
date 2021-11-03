using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class PedWastedPacketHandler : IPacketHandler<PedWastedPacket>
    {
        private readonly IElementRepository elementRepository;
        private readonly ISyncHandlerMiddleware<PedWastedPacket?> middleware;

        public PacketId PacketId => PacketId.PACKET_ID_PED_WASTED;

        public PedWastedPacketHandler(IElementRepository elementRepository, ISyncHandlerMiddleware<PedWastedPacket?> middleware)
        {
            this.elementRepository = elementRepository;
            this.middleware = middleware;
        }

        public void HandlePacket(Client client, PedWastedPacket packet)
        {
            if (this.elementRepository.Get(packet.SourceElementId) is not Ped ped)
                return;

            ped.RunAsSync(() =>
            {
                ped.Kill(this.elementRepository.Get(packet.KillerId), (WeaponType)packet.KillerWeapon, (BodyPart)packet.BodyPart, packet.AnimGroup, packet.AnimId);
            });

            var players = this.middleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(players);
        }
    }
}
