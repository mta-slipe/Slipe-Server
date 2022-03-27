using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.Handlers.Explosions;

public class ExplosionPacketHandler : IPacketHandler<ExplosionPacket>
{
    private readonly ISyncHandlerMiddleware<ExplosionPacket> middleware;
    private readonly IElementRepository elementRepository;

    public PacketId PacketId => PacketId.PACKET_ID_EXPLOSION;

    public ExplosionPacketHandler(
        ISyncHandlerMiddleware<ExplosionPacket> middleware,
        IElementRepository elementRepository
    )
    {
        this.middleware = middleware;
        this.elementRepository = elementRepository;
    }

    public void HandlePacket(Client client, ExplosionPacket packet)
    {
        var player = client.Player;
        packet.PlayerSource = player.Id;

        if (packet.OriginId != null)
        {
            var explosionorigin = this.elementRepository.Get(packet.OriginId.Value);
            if (explosionorigin != null)
            {
                if (explosionorigin is Elements.Vehicle vehicle)
                {
                    vehicle.BlowUp();
                }
            }
        }

        var nearbyPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);

        packet.SendTo(nearbyPlayers);
    }
}
