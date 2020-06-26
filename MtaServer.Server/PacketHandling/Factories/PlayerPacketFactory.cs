using MtaServer.Packets.Definitions.Join;
using MtaServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace MtaServer.Server.PacketHandling.Factories
{
    public static class PlayerPacketFactory
    {
        public static PlayerListPacket CreatePlayerListPacket(Player[] players, bool showInChat = false)
        {
            var packet = new PlayerListPacket(showInChat);

            foreach(var player in players)
            {
                packet.AddPlayer(
                    playerId: player.Id,
                    timeContext: player.TimeContext,
                    nickname: player.Name,
                    bitsreamVersion: 343,
                    buildNumber: 0,

                    isDead: false,
                    isInVehicle: false,
                    hasJetpack: false,
                    isNametagShowing: true,
                    isNametagColorOverriden: true,
                    isHeadless: false,
                    isFrozen: false,

                    nametagText: player.Name,
                    color: Color.FromArgb(255, 255, 0, 255),
                    moveAnimation: 0,

                    model: 9,
                    teamId: null,

                    vehicleId: null,
                    seat: null,

                    position: player.Position,
                    rotation: player.PedRotation,

                    dimension: 0,
                    fightingStyle: 0,
                    alpha: 255,
                    interior: 0,

                    weapons: new byte[16]
                );
            }

            return packet;
        }
    }
}
