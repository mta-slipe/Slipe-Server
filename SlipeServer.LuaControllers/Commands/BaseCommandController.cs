using SlipeServer.LuaControllers.Contexts;
using SlipeServer.Server.Elements;

namespace SlipeServer.LuaControllers;

public abstract class BaseCommandController
{
    private readonly AsyncLocal<CommandContext?> context = new();

    public CommandContext Context
    {
        get
        {
            if (this.context.Value == null)
                throw new Exception("Can not access BaseCommandController.Context outside of command handling methods.");

            return this.context.Value;
        }
    }

    internal void SetContext(CommandContext? context)
    {
        this.context.Value = context;
    }

    internal virtual void HandleCommand(Player player, string command, IEnumerable<object?> args, Func<IEnumerable<object?>, object?> handler)
    {
        this.SetContext(new CommandContext(player, command));
        handler.Invoke(args);
        this.SetContext(null);
    }
}


public abstract class BaseCommandController<TPlayer> : BaseCommandController where TPlayer : Player
{
    public new CommandContext<TPlayer> Context => (base.Context as CommandContext<TPlayer>)!;

    internal override void HandleCommand(Player player, string command, IEnumerable<object?> args, Func<IEnumerable<object?>, object?> handler)
    {
        if (player is not TPlayer tPlayer)
            return;

        this.SetContext(new CommandContext<TPlayer>(tPlayer, command));
        handler.Invoke(args);
        this.SetContext(null);
    }
}
