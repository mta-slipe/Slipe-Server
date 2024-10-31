using SlipeServer.Server.Elements;
using System.Reflection;

namespace SlipeServer.LuaControllers.Contexts;

public class CommandContext
{
    public Player Player { get; }
    public string Command { get; }
    public IEnumerable<object?> Arguments { get; }
    public MethodInfo MethodInfo { get; }

    public CommandContext(Player player, string command, IEnumerable<object?> arguments, MethodInfo methodInfo)
    {
        this.Player = player;
        this.Command = command;
        this.Arguments = arguments;
        this.MethodInfo = methodInfo;
    }
}

public class CommandContext<TPlayer> : CommandContext where TPlayer : Player
{
    public new TPlayer Player => (base.Player as TPlayer)!;

    public CommandContext(TPlayer player, string command, IEnumerable<object?> arguments, MethodInfo methodInfo) : base(player, command, arguments, methodInfo)
    {
    }
}
