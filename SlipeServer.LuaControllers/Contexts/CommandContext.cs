using SlipeServer.Server.Elements;
using System.Reflection;

namespace SlipeServer.LuaControllers.Contexts;

public class CommandContext
{
    public Player Player { get; }
    public string Command { get; }
    public IEnumerable<object?> Arguments { get; }
    public MethodInfo MethodInfo { get; }
    public CancellationToken CancellationToken { get; }

    public CommandContext(Player player, string command, IEnumerable<object?> arguments, MethodInfo methodInfo, CancellationToken cancellationToken)
    {
        this.Player = player;
        this.Command = command;
        this.Arguments = arguments;
        this.MethodInfo = methodInfo;
        this.CancellationToken = cancellationToken;
    }
}

public class CommandContext<TPlayer> : CommandContext where TPlayer : Player
{
    public new TPlayer Player => (base.Player as TPlayer)!;

    public CommandContext(TPlayer player, string command, IEnumerable<object?> arguments, MethodInfo methodInfo, CancellationToken cancellationToken) : base(player, command, arguments, methodInfo, cancellationToken)
    {
    }
}
