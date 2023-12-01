using Microsoft.Extensions.DependencyInjection;
using SlipeServer.LuaControllers.Results;
using SlipeServer.Server.Events;
using System.Reflection;

namespace SlipeServer.LuaControllers;

public class BoundEvent
{
    public IServiceProvider ServiceProvider { get; }
    public string EventName { get; set; }
    public Type ControllerType { get; set; }
    public BaseLuaController? ControllerInstance { get; set; }
    public MethodInfo Method { get; set; }

    public BoundEvent(
        IServiceProvider serviceProvider,
        string eventName,
        Type controllerType,
        MethodInfo method,
        BaseLuaController? controllerInstance)
    {
        this.ServiceProvider = serviceProvider;
        this.EventName = eventName;
        this.ControllerType = controllerType;
        this.Method = method;
        this.ControllerInstance = controllerInstance;
    }

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
