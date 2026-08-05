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
    public bool WithLogScope { get; set; } = method.GetCustomAttribute<WithLogScopeAttribute>() != null;
    public ILogger? Logger { get; set; }

    public LuaResult? HandleEvent(LuaEvent luaEvent, object?[] parameters)
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

            var result = controller.HandleEvent(luaEvent, (values) => this.Method.Invoke(controller, parameters));

            if (this.Method.ReturnType == typeof(void) || this.Method.ReturnType == typeof(Task))
                return null;

            if (result is LuaResult luaResult)
                return luaResult;

            return LuaResult<object?>.Success(result);
        } finally
        {
            logScope?.Dispose();
        }
    }
}
