using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.PacketHandling.QueueHandlers;

public class PedTaskPacketHandler : IPacketHandler<PedTaskPacket>
{
    private readonly ISyncHandlerMiddleware<PedTaskPacket?> middleware;

    public PacketId PacketId => PacketId.PACKET_ID_PED_TASK;

    public PedTaskPacketHandler(ISyncHandlerMiddleware<PedTaskPacket?> middleware)
    {
        this.middleware = middleware;
    }

    public void HandlePacket(IClient client, PedTaskPacket packet)
    {
        var players = this.middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(players);
    }
}
