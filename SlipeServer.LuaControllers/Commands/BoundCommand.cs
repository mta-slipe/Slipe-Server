using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.Elements;
using System.Reflection;

namespace SlipeServer.LuaControllers.Commands;

public class BoundCommand
{
    public IServiceProvider ServiceProvider { get; }
    public string Command { get; set; }
    public Type ControllerType { get; set; }
    public BaseCommandController? ControllerInstance { get; set; }
    public MethodInfo Method { get; set; }

    public BoundCommand(
        IServiceProvider serviceProvider,
        string command,
        Type controllerType,
        MethodInfo method,
        BaseCommandController? controllerInstance)
    {
        this.ServiceProvider = serviceProvider;
        this.Command = command;
        this.ControllerType = controllerType;
        this.Method = method;
        this.ControllerInstance = controllerInstance;
    }

    public void HandleCommand(Player player, string command, IEnumerable<object?> args)
    {
        var controller = this.ControllerInstance;
        if (controller == null)
        {
            var scope = this.ServiceProvider.CreateScope();
            controller = (BaseCommandController)ActivatorUtilities.CreateInstance(scope.ServiceProvider, this.ControllerType);
        }

        controller.HandleCommand(player, command, args, this.Method, (values) => this.Method.Invoke(controller, values.ToArray()));
    }
}
