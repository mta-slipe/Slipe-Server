using Microsoft.Extensions.Logging;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;
using System.Collections;
using System.Reflection;

namespace SlipeServer.LuaControllers.Commands;

public class CommandArgumentList(IEnumerable<string> arguments) : IEnumerable<string>
{
    public IEnumerator<string> GetEnumerator() => arguments.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => arguments.GetEnumerator();
}

public sealed class CommandControllerLogic
{
    private readonly MtaServer server;
    private readonly CommandService commandService;
    private readonly ILogger logger;
    private readonly LuaControllerArgumentsMapper argumentsMapper;
    private readonly Dictionary<string, List<BoundCommand>> syncHandlers = [];
    private readonly Dictionary<string, List<AsyncBoundCommand>> asyncHandlers = [];

    public CommandControllerLogic(
        MtaServer server,
        CommandService commandService,
        ILogger logger,
        LuaControllerArgumentsMapper argumentsMapper)
    {
        this.server = server;
        this.commandService = commandService;
        this.logger = logger;
        this.argumentsMapper = argumentsMapper;
        IndexControllers();
    }

    private void IndexControllers()
    {
        var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetExportedTypes())
            .Where(x => x.IsAssignableTo(typeof(BaseCommandController)))
            .Where(x => !x.IsAbstract);

        foreach (var controllerType in controllerTypes ?? [])
        {
            var controllerAttribute = controllerType
                .GetCustomAttributes<CommandControllerAttribute>()
                .SingleOrDefault();

            var methods = controllerType.GetMethods();

            var controller = controllerAttribute?.UsesScopedCommands == true ? null : (BaseCommandController)this.server.Instantiate(controllerType);

            foreach (var method in methods)
            {
                if (method.GetCustomAttributes<NoCommandAttribute>().Any())
                    continue;

                if (method.ReturnType == typeof(void))
                {
                    var commandAttributes = method.GetCustomAttributes<CommandAttribute>();

                    foreach (var attribute in commandAttributes)
                        AddHandler(attribute.Command, controllerType, method, controller, attribute.IsCaseSensitive);

                    if (!commandAttributes.Any())
                        AddHandler(method.Name, controllerType, method, controller);
                }
                else if(method.ReturnType == typeof(Task))
                {
                    var commandAttributes = method.GetCustomAttributes<CommandAttribute>();

                    foreach (var attribute in commandAttributes)
                        AddAsyncHandler(attribute.Command, controllerType, method, controller, attribute.IsCaseSensitive);

                    if (!commandAttributes.Any())
                        AddAsyncHandler(method.Name, controllerType, method, controller);
                }
            }
        }
    }

    private void AddHandler(string command, Type type, MethodInfo method, BaseCommandController? controller, bool isCaseSensitive = false)
    {
        if (!this.syncHandlers.ContainsKey(command))
        {
            this.syncHandlers[command] = [];
            this.commandService.AddCommand(command, isCaseSensitive).Triggered += (_, args) => HandleCommand(command, args);
        }

        this.syncHandlers[command].Add(new BoundCommand(this.server.Services, command, type, method, controller));
    }
    
    private void AddAsyncHandler(string command, Type type, MethodInfo method, BaseCommandController? controller, bool isCaseSensitive = false)
    {
        if (!this.asyncHandlers.ContainsKey(command))
        {
            this.asyncHandlers[command] = [];
            this.commandService.AddCommand(command, isCaseSensitive).Triggered += (_, args) => HandleAsyncCommand(command, args);
        }

        this.asyncHandlers[command].Add(new AsyncBoundCommand(this.server.Services, command, type, method, controller));
    }

    private void HandleCommand(string command, CommandTriggeredEventArgs e)
    {
        if (!this.syncHandlers.TryGetValue(command, out var handlers))
            return;

        foreach (var handler in handlers)
        {
            try
            {
                var parameters = this.argumentsMapper.MapParameters(e.Player, e.Arguments, handler.Method);
                if(parameters != null)
                    handler.HandleCommand(e.Player, command, parameters);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occured while handling the command {event}:\n{message}", command, exception.Message);
            }
        }
    }

    private async Task HandleAsyncCommand(string command, CommandTriggeredEventArgs e)
    {
        try
        {
            if (!this.asyncHandlers.TryGetValue(command, out var handlers))
                return;

            foreach (var handler in handlers)
            {
                try
                {
                    var parameters = this.argumentsMapper.MapParameters(e.Player, e.Arguments, handler.Method);
                    if (parameters != null)
                        await handler.HandleCommand(e.Player, command, parameters);
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception, "An error occured while handling the command {event}:\n{message}", command, exception.Message);
                }
            }

        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception thrown while executing command {commandName}", command);
        }
    }
}
