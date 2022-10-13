using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;

namespace SlipeServer.Server.Behaviour;

public class PlayerBehaviour
{
    public PlayerBehaviour(MtaServer server)
    {
        server.PlayerJoined += OnPlayerJoin;
    }

    private void OnPlayerJoin(Player player)
    {
        player.KeyBound += RelayKeyBound;
        player.KeyUnbound += RelayKeyUnbound;
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
}
