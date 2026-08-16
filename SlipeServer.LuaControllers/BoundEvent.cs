using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.LuaControllers.Results;
using SlipeServer.Server.Events;
using System.Reflection;

namespace SlipeServer.LuaControllers;

public class BoundEvent(
    IServiceProvider serviceProvider,
    string eventName,
    Type controllerType,
    MethodInfo method,
    BaseLuaController? controllerInstance)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public string EventName { get; set; } = eventName;
    public Type ControllerType { get; set; } = controllerType;
    public BaseLuaController? ControllerInstance { get; set; } = controllerInstance;
    public MethodInfo Method { get; set; } = method;
    public TimeSpan? RateLimit { get; set; } = method.GetCustomAttribute<RateLimitAttribute>()?.TimeSpan;

    public bool WithLogScope { get; set; } = (
        method.GetCustomAttribute<WithLogScopeAttribute>() ?? 
        method.DeclaringType?.GetCustomAttribute<WithLogScopeAttribute>()) != null;

    public bool SurpressErrorResponse { get; set; } = (
        method.GetCustomAttribute<SurpressErrorResponseAttribute>() ??
        method.DeclaringType?.GetCustomAttribute<SurpressErrorResponseAttribute>()) != null;

    public ILogger? Logger { get; set; }

    public async Task<LuaResult?> HandleEventAsync(LuaEvent luaEvent, object?[] parameters)
    {
        IDisposable? logScope = null;
        try
        {
            var controller = this.ControllerInstance;
            if (controller == null)
            {
                var scope = this.ServiceProvider.CreateScope();
                controller = (BaseLuaController)ActivatorUtilities.CreateInstance(scope.ServiceProvider, this.ControllerType);
            }

            if (this.WithLogScope)
                logScope = this.Logger?.BeginScope(new List<KeyValuePair<string, object?>>()
                {
                    new("LuaController", this.Method.DeclaringType?.Name),
                    new("LuaEventTriggered", luaEvent.Name),
                    new("LuaEventTriggeredByPlayer", luaEvent.Player.Name)
                });

            var result = await controller.HandleEventAsync(luaEvent, () => InvokeMethodAsync(controller, parameters)).ConfigureAwait(false);

            if (result is LuaResult luaResult)
                return luaResult;

            return result != null ? LuaResult<object?>.Success(result) : null;
        }
        finally
        {
            logScope?.Dispose();
        }
    }

    private async Task<object?> InvokeMethodAsync(BaseLuaController controller, object?[] parameters)
    {
        var invokeResult = this.Method.Invoke(controller, parameters);

        if (invokeResult is Task task)
        {
            await task.ConfigureAwait(false);

            if (task.GetType().IsGenericType)
                return task.GetType().GetProperty("Result")?.GetValue(task);

            return null;
        }

        return invokeResult;
    }
}
