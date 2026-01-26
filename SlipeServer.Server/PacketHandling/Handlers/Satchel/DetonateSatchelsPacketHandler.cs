using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.Satchel;

public class DetonateSatchelsPacketHandler(
    ISyncHandlerMiddleware<DetonateSatchelsPacket> middleware
    ) : IPacketHandler<DetonateSatchelsPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_DETONATE_SATCHELS;

    public void HandlePacket(IClient client, DetonateSatchelsPacket packet)
    {
        packet.ElementId = client.Player.Id;
        packet.Latency = (ushort)client.Ping;

        var otherPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);
    }
}
