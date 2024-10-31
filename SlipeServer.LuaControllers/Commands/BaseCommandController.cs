using SlipeServer.LuaControllers.Contexts;
using SlipeServer.Server.Elements;
using System.Reflection;

namespace SlipeServer.LuaControllers;

public abstract class BaseCommandController
{
    private readonly AsyncLocal<CommandContext?> context = new();

    public CommandContext Context
    {
        get
        {
            var value = this.context.Value;
            if (value == null)
                throw new Exception("Can not access BaseCommandController.Context outside of command handling methods.");

            return value;
        }
    }

    internal void SetContext(CommandContext? context)
    {
        this.context.Value = context;
    }

    protected virtual void Invoke(Action next)
    {
        next.Invoke();
    }
    
    protected virtual async Task InvokeAsync(Func<Task> next)
    {
        await next();
    }

    internal virtual void HandleCommand(Player player, string command, IEnumerable<object?> args, MethodInfo methodInfo, Func<IEnumerable<object?>, object?> handler)
    {
        var cts = new CancellationTokenSource();
        player.Disconnected += (sender, e) =>
        {
            cts.Cancel();
        };
        if (player.IsDestroyed)
            cts.Cancel();

        SetContext(new CommandContext(player, command, args, methodInfo, cts.Token));
        try
        {
            Invoke(() => handler.Invoke(args));
        }
        finally
        {
            SetContext(null);
        }
    }

    internal virtual async Task HandleCommandAsync(Player player, string command, IEnumerable<object?> args, MethodInfo methodInfo, Func<IEnumerable<object?>, Task> handler)
    {
        var cts = new CancellationTokenSource();
        player.Disconnected += (sender, e) =>
        {
            cts.Cancel();
        };
        if (player.IsDestroyed)
            cts.Cancel();

        SetContext(new CommandContext(player, command, args, methodInfo, cts.Token));
        try
        {
            await InvokeAsync(async () => await handler(args));
        }
        finally
        {
            SetContext(null);
        }
    }
}


public abstract class BaseCommandController<TPlayer> : BaseCommandController where TPlayer : Player
{
    public new CommandContext<TPlayer> Context => (base.Context as CommandContext<TPlayer>)!;

    internal override void HandleCommand(Player player, string command, IEnumerable<object?> args, MethodInfo methodInfo, Func<IEnumerable<object?>, object?> handler)
    {
        if (player is not TPlayer tPlayer)
            return;

        var cts = new CancellationTokenSource();
        tPlayer.Disconnected += (sender, e) =>
        {
            cts.Cancel();
        };
        if(tPlayer.IsDestroyed)
            cts.Cancel();

        SetContext(new CommandContext<TPlayer>(tPlayer, command, args, methodInfo, cts.Token));
        try
        {
            Invoke(() => handler.Invoke(args));
        }
        finally
        {
            SetContext(null);
        }
    }

    internal override async Task HandleCommandAsync(Player player, string command, IEnumerable<object?> args, MethodInfo methodInfo, Func<IEnumerable<object?>, Task> handler)
    {
        if (player is not TPlayer tPlayer)
            return;

        var cts = new CancellationTokenSource();
        tPlayer.Disconnected += (sender, e) =>
        {
            cts.Cancel();
        };
        if (tPlayer.IsDestroyed)
            cts.Cancel();

        SetContext(new CommandContext<TPlayer>(tPlayer, command, args, methodInfo, cts.Token));
        try
        {
            await InvokeAsync(async () => await handler.Invoke(args));
        }
        finally
        {
            SetContext(null);
        }
    }
}
