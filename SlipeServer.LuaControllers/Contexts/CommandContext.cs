using SlipeServer.Server.Elements;

namespace SlipeServer.LuaControllers.Contexts;


public class CommandContext
{
    public Player Player { get; }
    public string Command { get; }

    public CommandContext(Player player, string command)
    {
        this.Player = player;
        this.Command = command;
    }
}


public class CommandContext<TPlayer> : CommandContext where TPlayer : Player
{
    public new TPlayer Player => (base.Player as TPlayer)!;

    public CommandContext(TPlayer player, string command) : base(player, command)
    {
    }
}
