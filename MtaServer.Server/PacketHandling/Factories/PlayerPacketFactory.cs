using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Player;
using MtaServer.Server.Elements;
using MtaServer.Server.Elements.Enums;
using MtaServer.Server.Enums;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace MtaServer.Server.PacketHandling.Factories
{
    public static class PlayerPacketFactory
    {
        public static HudComponentVisiblePacket CreateShowHudComponentPacket(HudComponent hudComponent, bool show)
        {
            return new HudComponentVisiblePacket((byte)hudComponent, show);
        }

        public static PlayerListPacket CreatePlayerListPacket(Player[] players, bool showInChat = false)
        {
            var packet = new PlayerListPacket(showInChat);

            foreach(var player in players)
            {
                packet.AddPlayer(
                    playerId: player.Id,
                    timeContext: player.TimeContext,
                    nickname: player.Name ?? "???",
                    bitsreamVersion: 343,
                    buildNumber: 0,

                    isDead: false,
                    isInVehicle: false,
                    hasJetpack: false,
                    isNametagShowing: true,
                    isNametagColorOverriden: true,
                    isHeadless: false,
                    isFrozen: false,

                    nametagText: player.Name ?? "???",
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

        public static SetFPSLimitPacket CreateSetFPSLimitPacket(ushort limit)
        {
            return new SetFPSLimitPacket(limit);
        }
        
        public static PlaySoundPacket CreatePlaySoundPacket(byte sound)
        {
            return new PlaySoundPacket(sound);
        }
        
        public static SetWantedLevelPacket CreateSetWantedLevelPacket(byte level)
        {
            return new SetWantedLevelPacket(level);
        }
        
        public static ToggleDebuggerPacket CreateToggleDebuggerPacket(bool visible)
        {
            return new ToggleDebuggerPacket(visible);
        }
        
        public static DebugEchoPacket CreateDebugEchoPacket(string message, byte level)
        {
            return new DebugEchoPacket(message, level, Color.White);
        }

        public static DebugEchoPacket CreateDebugEchoPacket(string message, byte level, Color color)
        {
            return new DebugEchoPacket(message, level, color);
        }
        
        public static ForcePlayerMapPacket CreateForcePlayerMapPacket(bool visible)
        {
            return new ForcePlayerMapPacket(visible);
        }
        
        public static ToggleAllControlsPacket CreateToggleAllControlsPacket(bool enabled, bool gtaControls = true, bool mtaControls = true)
        {
            return new ToggleAllControlsPacket(enabled, gtaControls, mtaControls);
        }

        public static PlayerQuitPacket CreateQuitPacket(Player player, QuitReason reason = QuitReason.Quit)
        {
            return new PlayerQuitPacket(player.Id, (byte)reason);
        }
    }
}
