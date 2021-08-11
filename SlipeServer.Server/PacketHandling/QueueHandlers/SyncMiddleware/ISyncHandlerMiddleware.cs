using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware
{
    public interface ISyncHandlerMiddleware<TPacket> where TPacket: Packet
    {
        IEnumerable<Player> GetPlayersToSyncTo(Player player, TPacket packet);
    }
}
