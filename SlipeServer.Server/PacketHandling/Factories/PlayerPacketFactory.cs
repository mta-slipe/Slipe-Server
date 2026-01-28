using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System.Drawing;
using SlipeServer.Packets.Definitions.Player;
using System.Linq;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class PlayerPacketFactory
{
    public static HudComponentVisiblePacket CreateShowHudComponentPacket(HudComponent hudComponent, bool show)
    {
        return new HudComponentVisiblePacket((byte)hudComponent, show);
    }

    public static PlayerListPacket CreatePlayerListPacket(Player[] players, bool showInChat = false)
    {
        var packet = new PlayerListPacket(showInChat);

        foreach (var player in players.Where(x => x.Client.ConnectionState == ClientConnectionState.Joined))
        {
            packet.AddPlayer(
                playerId: player.Id,
                timeContext: player.TimeContext,
                nickname: player.Name,
                bitsreamVersion: 343,
                buildNumber: 0,

                isDead: !player.IsAlive,
                isInVehicle: player.Vehicle != null,
                hasJetpack: player.HasJetpack,
                isNametagShowing: player.IsNametagShowing,
                isHeadless: player.IsHeadless,
                isFrozen: player.IsFrozen,

                nametagText: player.NametagText,
                nametagColor: player.NametagColor,
                moveAnimation: 0,

                model: player.Model,
                teamId: player.Team?.Id,

                vehicleId: player.Vehicle?.Id,
                seat: player.Seat,

                position: player.Position,
                rotation: player.PedRotation,

                dimension: player.Dimension,
                fightingStyle: (byte)player.FightingStyle,
                alpha: player.Alpha,
                interior: player.Interior,

                weapons: Enumerable.Range(0, 16).Select((value, index) =>
                {
                    return (byte)(player.Weapons.Get((WeaponSlot)index)?.Type ?? 0);
                }).ToArray()
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

    public static SetMoneyPacket CreateSetMoneyPacket(int money, bool instant)
    {
        return new SetMoneyPacket(money, instant);
    }

    public static ToggleDebuggerPacket CreateToggleDebuggerPacket(bool visible)
    {
        return new ToggleDebuggerPacket(visible);
    }

    public static DebugEchoPacket CreateDebugEchoPacket(string message, DebugLevel level)
    {
        return new DebugEchoPacket(message, (byte)level, Color.White);
    }

    public static DebugEchoPacket CreateDebugEchoPacket(string message, DebugLevel level, Color color)
    {
        return new DebugEchoPacket(message, (byte)level, color);
    }

    public static ForcePlayerMapPacket CreateForcePlayerMapPacket(bool visible)
    {
        return new ForcePlayerMapPacket(visible);
    }

    public static SetTransferBoxVisiblePacket CreateTransferBoxVisiblePacket(bool visible)
    {
        return new SetTransferBoxVisiblePacket(visible);
    }

    public static ToggleAllControlsPacket CreateToggleAllControlsPacket(bool enabled, bool gtaControls = true, bool mtaControls = true)
    {
        return new ToggleAllControlsPacket(enabled, gtaControls, mtaControls);
    }

    public static PlayerQuitPacket CreateQuitPacket(Player player, QuitReason reason = QuitReason.Quit)
    {
        return new PlayerQuitPacket(player.Id, (byte)reason);
    }

    public static SpawnPlayerPacket CreateSpawnPacket(Player player)
    {
        return new SpawnPlayerPacket(player.Id, 0, player.Position, player.PedRotation, player.Model, ElementId.Zero, player.Interior, player.Dimension, player.GetAndIncrementTimeContext());
    }

    public static PlayerWastedPacket CreateWastedPacket(
        Player player, Element? killer, DamageType weaponType, BodyPart bodyPart, bool isStealth,
        ulong animationGroup, ulong animationId
    )
    {
        return new PlayerWastedPacket(player.Id, killer?.Id ?? ElementId.Zero, (byte)weaponType, (byte)bodyPart, isStealth,
            player.GetAndIncrementTimeContext(), animationGroup, animationId);
    }

    public static ChangeNicknamePacket CreateNicknameChangePacket(Player player)
    {
        return new ChangeNicknamePacket(player.Id, player.Name);
    }

    public static UpdateInfoPacket CreateUpdateInfoPacket(string version, bool mandatory = true)
    {
        if (mandatory)
            return new UpdateInfoPacket("Mandatory", version);
        return new UpdateInfoPacket("Optional", version);
    }
}
