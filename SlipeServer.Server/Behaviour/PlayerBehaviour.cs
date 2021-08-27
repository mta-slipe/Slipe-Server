using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour
{
    public class PlayerBehaviour
    {
        private readonly IElementRepository elementRepository;
        private readonly MtaServer server;
        private readonly ILogger logger;

        public PlayerBehaviour(IElementRepository elementRepository, MtaServer server, ILogger logger)
        {
            this.elementRepository = elementRepository;
            this.server = server;
            this.logger = logger;
            server.PlayerJoined += OnPlayerJoin;
        }

        private void OnPlayerJoin(Player player)
        {
            player.KeyBound += RelayKeyBinded;
        }

        private void RelayKeyBinded(Player sender, PlayerBindKeyArgs e)
        {
            if (e.KeyState == KeyState.Down || e.KeyState == KeyState.Both)
            {
                var packet = new BindKeyPacket(e.Player.Id, e.Key, true);
                this.server.BroadcastPacket(packet);
            }
            if (e.KeyState == KeyState.Up || e.KeyState == KeyState.Both)
            {
                var packet = new BindKeyPacket(e.Player.Id, e.Key, false);
                this.server.BroadcastPacket(packet);
            }
        }
    }
}
