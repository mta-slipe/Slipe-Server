using Microsoft.Extensions.DependencyInjection;
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

    public void HandleCommand(Player player, string command, IEnumerable<object?> args)
    {
        var controller = this.ControllerInstance;
        if (controller == null)
        {
            var scope = this.ServiceProvider.CreateScope();
            controller = (BaseCommandController)ActivatorUtilities.CreateInstance(scope.ServiceProvider, this.ControllerType);
        }

        controller.HandleCommand(player, command, args, (values) => this.Method.Invoke(controller, values.ToArray()));
    }
}
