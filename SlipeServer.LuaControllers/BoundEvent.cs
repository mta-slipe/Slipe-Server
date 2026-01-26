using Microsoft.Extensions.DependencyInjection;
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

    public LuaResult? HandleEvent(LuaEvent luaEvent, object?[] parameters)
    {
        var controller = this.ControllerInstance;
        if (controller == null)
        {
            var scope = this.ServiceProvider.CreateScope();
            controller = (BaseLuaController)ActivatorUtilities.CreateInstance(scope.ServiceProvider, this.ControllerType); 
        }

        var result = controller.HandleEvent(luaEvent, (values) => this.Method.Invoke(controller, parameters));

        if (this.Method.ReturnType == typeof(void) || this.Method.ReturnType == typeof(Task))
            return null;

        if (result is LuaResult luaResult)
            return luaResult;

        return LuaResult<object?>.Success(result);
    }
}
