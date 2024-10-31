using Microsoft.Extensions.Logging;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;
using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace SlipeServer.LuaControllers.Commands;

public class CommandArgumentList(IEnumerable<string> arguments) : IEnumerable<string>
{
    public IEnumerator<string> GetEnumerator() => arguments.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => arguments.GetEnumerator();
}

public class ControllerArgumentException : Exception
{
    public int Index { get; }

    public ControllerArgumentException(int index, Exception innerException) : base(null, innerException)
    {
        this.Index = index;
    }
}

public sealed class LuaControllerArgumentsMapper
{
    private readonly Dictionary<Type, Func<string, object?>> mappings = []; 

    public LuaControllerArgumentsMapper() { }

    public void DefineMap<T>(Func<string, object?> map)
    {
        if (!this.mappings.ContainsKey(typeof(T)))
        {
            this.mappings[typeof(T)] = map;
        }
    }

    internal object? MapParameter(Type targetType, string value)
    {
        if (this.mappings.TryGetValue(targetType, out var mapFunction))
        {
            return mapFunction(value);
        }

        return null;
    }
}

public sealed class CommandControllerLogic
{
    private readonly MtaServer server;
    private readonly CommandService commandService;
    private readonly ILogger logger;
    private readonly LuaControllerArgumentsMapper argumentsMapper;
    private readonly Dictionary<string, List<BoundCommand>> handlers = [];

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
                if (!method.GetCustomAttributes<NoCommandAttribute>().Any())
                {
                    var commandAttributes = method.GetCustomAttributes<CommandAttribute>();

                    foreach (var attribute in commandAttributes)
                        AddHandler(attribute.Command, controllerType, method, controller, attribute.IsCaseSensitive);

                    if (!commandAttributes.Any())
                        AddHandler(method.Name, controllerType, method, controller);
                }
            }
        }
    }

    private void AddHandler(string command, Type type, MethodInfo method, BaseCommandController? controller, bool isCaseSensitive = false)
    {
        if (!this.handlers.ContainsKey(command))
        {
            this.handlers[command] = [];
            this.commandService.AddCommand(command, isCaseSensitive).Triggered += (_, args) => HandleCommand(command, args);
        }

        this.handlers[command].Add(new BoundCommand(this.server.Services, command, type, method, controller));
    }

    private object? MapParameter(Type targetType, string value)
    {
        if (targetType == typeof(string))
            return value;

        if (targetType.IsAssignableFrom(typeof(string)))
            return targetType;

        if (targetType == typeof(byte))
            return byte.Parse(value);

        if (targetType == typeof(ushort))
            return ushort.Parse(value);
        if (targetType == typeof(short))
            return short.Parse(value);

        if (targetType == typeof(uint))
            return uint.Parse(value);
        if (targetType == typeof(int))
            return int.Parse(value);

        if (targetType == typeof(ulong))
            return ulong.Parse(value);
        if (targetType == typeof(long))
            return long.Parse(value);

        if (targetType == typeof(float))
            return float.Parse(value);
        if (targetType == typeof(double))
            return double.Parse(value);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value, true);

        return argumentsMapper.MapParameter(targetType, value);
    }

    private object?[] MapParameters(string[] values, MethodInfo method)
    {
        var parameters = method.GetParameters();

        if (parameters.Length == 0)
            return [];

        if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableTo(typeof(IEnumerable<string>)))
            return [ new CommandArgumentList(values) ];

        var objects = new List<object?>();
        for (var i = 0; i < parameters.Length; i++)
            if (!parameters[i].IsOptional || values.Length > i)
                objects.Add(MapParameter(parameters[i].ParameterType, values[i]));
            else if (values.Length <= i)
                objects.Add(parameters[i].DefaultValue);

        return [.. objects];
    }

    private void HandleCommand(string command, CommandTriggeredEventArgs e)
    {
        if (!this.handlers.TryGetValue(command, out var handlers))
            return;

        foreach (var handler in handlers)
        {
            try
            {
                var parameters = MapParameters(e.Arguments, handler.Method);
                handler.HandleCommand(e.Player, command, parameters);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occured while handling the command {event}:\n{message}", command, exception.Message);
            }
        }
    }
}
