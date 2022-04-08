using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour;

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
