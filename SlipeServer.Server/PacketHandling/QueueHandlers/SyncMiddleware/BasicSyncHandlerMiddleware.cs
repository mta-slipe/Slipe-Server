using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware
{
    public class BasicSyncHandlerMiddleware<TPacket> : ISyncHandlerMiddleware<TPacket> where TPacket: Packet
    {
        private readonly IElementRepository elementRepository;

        public BasicSyncHandlerMiddleware(IElementRepository elementRepository)
        {
            this.elementRepository = elementRepository;
        }

        public IEnumerable<Player> GetPlayersToSyncTo(Player player, TPacket packet)
        {
            return this.elementRepository
                .GetByType<Player>(ElementType.Player)
                .Where(x => x != player);
        }
    }
}
