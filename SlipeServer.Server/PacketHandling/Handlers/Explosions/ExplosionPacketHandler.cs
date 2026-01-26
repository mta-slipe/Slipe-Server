using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers.Explosions;

public class ExplosionPacketHandler(
    ISyncHandlerMiddleware<ExplosionPacket> middleware,
    IElementCollection elementCollection
    ) : IPacketHandler<ExplosionPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_EXPLOSION;

    public void HandlePacket(IClient client, ExplosionPacket packet)
    {
        var player = client.Player;
        packet.PlayerSource = player.Id;

        if (packet.OriginId != null)
        {
            var explosionorigin = elementCollection.Get(packet.OriginId.Value);
            if (explosionorigin != null)
            {
                if (explosionorigin is Elements.Vehicle vehicle)
                {
                    vehicle.BlowUp();
                }
            }
        }

        var nearbyPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);

        packet.SendTo(nearbyPlayers);
    }
}
