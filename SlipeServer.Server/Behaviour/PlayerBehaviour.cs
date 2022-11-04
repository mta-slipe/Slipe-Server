using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;

namespace SlipeServer.Server.Behaviour;

public class PlayerBehaviour
{
    private readonly MtaServer server;

    public PlayerBehaviour(MtaServer server)
    {
        this.server = server;

        server.PlayerJoined += OnPlayerJoin;
    }

    private void OnPlayerJoin(Player player)
    {
        player.KeyBound += RelayKeyBound;
        player.KeyUnbound += RelayKeyUnbound;
        player.NametagTextChanged += RelayNametagChange;
        player.NametagColorChanged += RelayNametagColorChange;
        player.IsNametagShowingChanged += RelayNametagShowingChange;
    }

    private void RelayKeyBound(Player player, PlayerBindKeyArgs e)
    {
        if (e.KeyState == KeyState.Down || e.KeyState == KeyState.Both)
        {
            new BindKeyPacket(e.Player.Id, e.Key, true).SendTo(player);
        }
        if (e.KeyState == KeyState.Up || e.KeyState == KeyState.Both)
        {
            new BindKeyPacket(e.Player.Id, e.Key, false).SendTo(player);
        }
    }

    private void RelayKeyUnbound(Player player, PlayerBindKeyArgs e)
    {
        if (e.KeyState == KeyState.Down || e.KeyState == KeyState.Both)
        {
            new UnbindKeyPacket(e.Player.Id, e.Key, true).SendTo(player);
        }
        if (e.KeyState == KeyState.Up || e.KeyState == KeyState.Both)
        {
            new UnbindKeyPacket(e.Player.Id, e.Key, false).SendTo(player);
        }
    }

    private void RelayNametagChange(Player sender, ElementChangedEventArgs<Player, string> args)
    {
        this.server.BroadcastPacket(new SetPlayerNametagTextPacket(sender.Id, args.NewValue));
    }

    private void RelayNametagColorChange(Player sender, ElementChangedEventArgs<Player, System.Drawing.Color?> args)
    {
        if (args.NewValue.HasValue)
            this.server.BroadcastPacket(new SetPlayerNametagColorPacket(sender.Id, args.NewValue.Value));
        else
            this.server.BroadcastPacket(new RemovePlayerNametagColorPacket(sender.Id));
    }

    private void RelayNametagShowingChange(Player sender, ElementChangedEventArgs<Player, bool> args)
    {
        this.server.BroadcastPacket(new SetPlayerNametagShowingPacket(sender.Id, args.NewValue));
    }
}
