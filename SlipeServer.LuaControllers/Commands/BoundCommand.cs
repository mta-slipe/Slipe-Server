using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Elements;
using System.Reflection;

namespace SlipeServer.LuaControllers.Commands;

public class BoundCommand(
    IServiceProvider serviceProvider,
    string command,
    Type controllerType,
    MethodInfo method,
    BaseCommandController? controllerInstance)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public string Command { get; set; } = command;
    public Type ControllerType { get; set; } = controllerType;
    public BaseCommandController? ControllerInstance { get; set; } = controllerInstance;
    public MethodInfo Method { get; set; } = method;
    public TimeSpan? RateLimit { get; set; } = method.GetCustomAttribute<RateLimitAttribute>()?.TimeSpan;

    public bool WithLogScope { get; set; } = (
        method.GetCustomAttribute<WithLogScopeAttribute>() ??
        method.DeclaringType?.GetCustomAttribute<WithLogScopeAttribute>()) != null;

    public ILogger? Logger { get; set; }

    public async Task HandleCommandAsync(Player player, string command, IEnumerable<object?> args)
    {
        IDisposable? logScope = null;
        try
        {
            var controller = this.ControllerInstance;
            if (controller == null)
            {
                var scope = this.ServiceProvider.CreateScope();
                controller = (BaseCommandController)ActivatorUtilities.CreateInstance(scope.ServiceProvider, this.ControllerType);
            }

            if (this.WithLogScope)
                logScope = this.Logger?.BeginScope(new List<KeyValuePair<string, object?>>()
                {
                    new("CommandController", this.Method.DeclaringType?.Name),
                    new("CommandTriggered", command),
                    new("CommandTriggeredBy", player.Name)
                });

            await controller.HandleCommandAsync(player, command, args, () => InvokeMethodAsync(controller, args)).ConfigureAwait(false);
        }
        finally
        {
            logScope?.Dispose();
        }
    }

    private async Task InvokeMethodAsync(BaseCommandController controller, IEnumerable<object?> args)
    {
        var invokeResult = this.Method.Invoke(controller, args.ToArray());

        if (invokeResult is Task task)
            await task.ConfigureAwait(false);
    }
}
