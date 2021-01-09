using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Services
{
    public class ExplosionService
    {
        private readonly MtaServer server;

        public ExplosionService(MtaServer server)
        {
            this.server = server;
        }

        public void CreateExplosion(Vector3 position, ExplosionType type, Player? responsiblePlayer = null)
        {
            var packet = new ExplosionPacket(responsiblePlayer?.Id, null, position, (byte)type, 0);
            server.BroadcastPacket(packet);
        }

        public void CreateExplosionFor(
            IEnumerable<Player> players, 
            Vector3 position, 
            ExplosionType type, 
            Player? responsiblePlayer = null
        )
        {
            var packet = new ExplosionPacket(responsiblePlayer?.Id, null, position, (byte)type, 0);
            packet.SendTo(players);
        }
    }
}
