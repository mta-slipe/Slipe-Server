using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;

namespace SlipeServer.Server.Extensions.Relaying;

public static class PlayerPropertyRelayingExtensions
{
    public static void AddPlayerRelayers(this Player player)
    {
        player.KeyBound += RelayKeyBound;
        player.KeyUnbound += RelayKeyUnbound;
        player.NametagTextChanged += RelayNametagChange;
        player.NametagColorChanged += RelayNametagColorChange;
        player.IsNametagShowingChanged += RelayNametagShowingChange;
        player.TeamChanged += RelayTeamChange;
        player.Spawned += RelayPlayerSpawn;
        player.WantedLevelChanged += WantedLevelChanged;
        player.MoneyChanged += RelayMoneyChanged;
        player.Wasted += RelayPlayerWasted;
        player.NameChanged += RelayedNameChanged;
    }


    private static void RelayKeyBound(Player player, PlayerBindKeyArgs e)
    {
        if (e.KeyState == KeyState.Down || e.KeyState == KeyState.Both)
        {
            new BindKeyPacket(e.Key, true).SendTo(player);
        }
        if (e.KeyState == KeyState.Up || e.KeyState == KeyState.Both)
        {
            new BindKeyPacket(e.Key, false).SendTo(player);
        }
    }

    private static void RelayKeyUnbound(Player player, PlayerBindKeyArgs e)
    {
        if (e.KeyState == KeyState.Down || e.KeyState == KeyState.Both)
        {
            new UnbindKeyPacket(e.Key, true).SendTo(player);
        }
        if (e.KeyState == KeyState.Up || e.KeyState == KeyState.Both)
        {
            new UnbindKeyPacket(e.Key, false).SendTo(player);
        }
    }

    private static void RelayNametagChange(Player sender, ElementChangedEventArgs<Player, string> args)
    {
        sender.RelayChange(new SetPlayerNametagTextPacket(sender.Id, args.NewValue));
    }

    private static void RelayNametagColorChange(Player sender, ElementChangedEventArgs<Player, System.Drawing.Color?> args)
    {
        if (args.NewValue.HasValue)
            sender.RelayChange(new SetPlayerNametagColorPacket(sender.Id, args.NewValue.Value));
        else
            sender.RelayChange(new RemovePlayerNametagColorPacket(sender.Id));
    }

    private static void RelayNametagShowingChange(Player sender, ElementChangedEventArgs<Player, bool> args)
    {
        sender.RelayChange(new SetPlayerNametagShowingPacket(sender.Id, args.NewValue));
    }

    private static void RelayTeamChange(Player sender, PlayerTeamChangedArgs e)
    {
        sender.RelayChange(new SetPlayerTeamRpcPacket()
        {
            SourceElementId = sender.Id,
            TeamId = e.NewTeam?.Id ?? ElementId.Zero
        });
    }


    private static void RelayPlayerSpawn(Player sender, PlayerSpawnedEventArgs args)
    {
        var packet = PlayerPacketFactory.CreateSpawnPacket(args.Source);
        sender.RelayChange(packet);
    }

    private static void WantedLevelChanged(Player sender, ElementChangedEventArgs<Player, byte> args)
    {
        var packet = PlayerPacketFactory.CreateSetWantedLevelPacket(args.NewValue);
        args.Source.Client.SendPacket(packet);
    }

    private static void RelayMoneyChanged(Player sender, PlayerMoneyChangedEventArgs args)
    {
        var packet = PlayerPacketFactory.CreateSetMoneyPacket(args.Money, args.Instant);
        args.Source.Client.SendPacket(packet);
    }

    private static void RelayPlayerWasted(Ped sender, PedWastedEventArgs e)
    {
        var packet = PlayerPacketFactory.CreateWastedPacket(
            (e.Source as Player)!, e.Killer, e.WeaponType, e.BodyPart, false, e.AnimationGroup, e.AnimationId
        );
        sender.RelayChange(packet);
    }

    private static void RelayedNameChanged(Element sender, ElementChangedEventArgs<string> args)
    {
        if (!args.IsSync)
        {
            var packet = PlayerPacketFactory.CreateNicknameChangePacket((Player)args.Source);
            sender.RelayChange(packet);
        }
    }
}
