using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.QueueHandlers;

public class PedTaskPacketHandler(ISyncHandlerMiddleware<PedTaskPacket?> middleware) : IPacketHandler<PedTaskPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PED_TASK;

    public void HandlePacket(IClient client, PedTaskPacket packet)
    {
        var players = middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(players);
    }
}
