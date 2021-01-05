using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsible for handling player events and sending corresponding packets
    /// </summary>
    public class PlayerPacketBehaviour
    {
        private readonly MtaServer server;

        public PlayerPacketBehaviour(MtaServer server)
        {
            this.server = server;
            server.PlayerJoined += OnPlayerJoin;
        }

        private void OnPlayerJoin(Player player)
        {
            player.Spawned += RelayPlayerSpawn;
            player.WantedLevelChanged += WantedLevelChanged;
        }

        private void RelayPlayerSpawn(Player player)
        {
            var packet = PlayerPacketFactory.CreateSpawnPacket(player);
            this.server.BroadcastPacket(packet);
        }

        private void WantedLevelChanged(object sender, ElementChangedEventArgs<Player, byte> args)
        {
            var packet = PlayerPacketFactory.CreateSetWantedLevelPacket(args.NewValue);
            args.Source.Client.SendPacket(packet);
        }
    }
}
