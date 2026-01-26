using SlipeServer.Server.Elements;

namespace SlipeServer.LuaControllers.Contexts;


public class CommandContext(Player player, string command)
{
    public Player Player { get; } = player;
    public string Command { get; } = command;
}


public class CommandContext<TPlayer>(TPlayer player, string command) : CommandContext(player, command) where TPlayer : Player
{
    public new TPlayer Player => (base.Player as TPlayer)!;
}
