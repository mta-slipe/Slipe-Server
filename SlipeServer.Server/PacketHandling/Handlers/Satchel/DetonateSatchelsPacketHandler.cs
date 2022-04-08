using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.Satchel;

public class DetonateSatchelsPacketHandler : IPacketHandler<DetonateSatchelsPacket>
{
    private readonly ISyncHandlerMiddleware<DetonateSatchelsPacket> middleware;

    public PacketId PacketId => PacketId.PACKET_ID_DETONATE_SATCHELS;

    public DetonateSatchelsPacketHandler(
        ISyncHandlerMiddleware<DetonateSatchelsPacket> middleware
    )
    {
        this.middleware = middleware;
    }

    public void HandlePacket(Client client, DetonateSatchelsPacket packet)
    {
        packet.ElementId = client.Player.Id;
        packet.Latency = (ushort)client.Ping;

        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);
    }
}
