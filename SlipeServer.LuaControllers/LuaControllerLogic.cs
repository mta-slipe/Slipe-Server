using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.LuaControllers.Commands;
using SlipeServer.LuaControllers.Results;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;
using System.Collections.Concurrent;
using System.Reflection;

namespace SlipeServer.LuaControllers;

public class LuaControllerLogic
{
    private readonly IMtaServer server;
    private readonly ILuaEventService luaEventService;
    private readonly ILuaValueMapper luaValueMapper;
    private readonly IFromLuaValueMapper fromLuaValueMapper;
    private readonly ITimerService timerService;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<LuaControllerLogic> logger;
    private readonly Dictionary<string, List<BoundEvent>> handlers = [];
    private readonly ConcurrentDictionary<Player, ConcurrentDictionary<BoundEvent, DateTime>> rateLimitTimes = [];

    public LuaControllerLogic(
        IMtaServer server,
        ILuaEventService luaEventService,
        ILuaValueMapper luaValueMapper,
        IFromLuaValueMapper fromLuaValueMapper,
        ITimerService timerService,
        IServiceProvider serviceProvider,
        ILogger<LuaControllerLogic> logger)
    {
        this.server = server;
        this.luaEventService = luaEventService;
        this.luaValueMapper = luaValueMapper;
        this.fromLuaValueMapper = fromLuaValueMapper;
        this.timerService = timerService;
        this.serviceProvider = serviceProvider;
        this.logger = logger;

        IndexControllers();

        server.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        this.rateLimitTimes[player] = [];

        player.Disconnected += HandlePlayerDisconnect;
    }

    private void HandlePlayerDisconnect(Player player, PlayerQuitEventArgs e)
    {
        this.rateLimitTimes.TryRemove(player, out _);
    }

    private void IndexControllers()
    {
        var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetExportedTypes())
            .Where(x => x.IsAssignableTo(typeof(BaseLuaController)))
            .Where(x => !x.IsAbstract);

        foreach (var controllerType in controllerTypes ?? Array.Empty<Type>())
        {
            var controllerAttribute = controllerType
                .GetCustomAttributes<LuaControllerAttribute>()
                .SingleOrDefault();

            var methods = controllerType.GetMethods();
            var prefix = controllerAttribute?.EventPrefix ?? "";

            var controller = controllerAttribute?.UsesScopedEvents == true ? null : (BaseLuaController)this.server.Instantiate(controllerType);

            foreach (var method in methods)
            {
                if (!method.GetCustomAttributes<NoLuaEventAttribute>().Any())
                {
                    var eventAttributes = method.GetCustomAttributes<LuaEventAttribute>();
                    var timedAttributes = method.GetCustomAttributes<TimedAttribute>();
                    var initAttribute = method.GetCustomAttribute<InitAttribute>();
                    var asyncInitAttribute = method.GetCustomAttribute<AsyncInitAttribute>();

                    foreach (var attribute in eventAttributes)
                        AddHandler(prefix + attribute.EventName, controllerType, method, controller);

                    foreach (var attribute in timedAttributes)
                        AddTimedHandler(attribute.Interval, controllerType, method, controller);

                    if (!eventAttributes.Any() && !timedAttributes.Any())
                        AddHandler(prefix + method.Name, controllerType, method, controller);

                    if (initAttribute != null)
                    {
                        var initController = controller ?? (BaseLuaController)this.server.InstantiateScoped(controllerType);
                        method.Invoke(initController, []);
                    }

                    if (asyncInitAttribute != null)
                    {
                        var initController = controller ?? (BaseLuaController)this.server.InstantiateScoped(controllerType);
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await (Task)method.Invoke(initController, [])!;
                            }
                            catch (Exception e)
                            {
                                this.logger.LogError(e, "An error occured while attempting to handle an async init event {Error}\n{StackTrace}", e.Message, e.StackTrace);
                            }
                        });
                    }
                }
            }
        }
    }

    private void AddHandler(string name, Type type, MethodInfo method, BaseLuaController? controller)
    {
        if (!this.handlers.ContainsKey(name))
        {
            this.handlers[name] = new();
            this.luaEventService.AddEventHandler(name, HandleLuaEvent);
        }

        this.handlers[name].Add(new BoundEvent(this.server.Services, name, type, method, controller)
        {
            Logger = this.logger
        });
    }

    private void AddTimedHandler(TimeSpan interval, Type type, MethodInfo method, BaseLuaController? controller)
    {
        var usesLogScope = method.GetCustomAttribute<WithLogScopeAttribute>() != null;

        this.timerService.CreateTimer(() =>
        {
            IDisposable? logScope = null;

            try
            {
                var instance = controller;
                if (instance == null)
                {
                    var scope = this.serviceProvider.CreateScope();
                    instance = (BaseLuaController)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);
                }

                if (usesLogScope)
                    logScope = this.logger.BeginScope(new Dictionary<string, object>
                    {
                        ["LuaController"] = type.Name,
                        ["ControllerMethod"] = method.Name,
                        ["ControllerInterval"] = interval
                    });

                method.Invoke(instance, Array.Empty<object>());

            } catch (Exception e)
            {
                this.logger.LogError(e, "An error occured while handling a timed event");
            }
            finally
            {
                logScope?.Dispose();
            }
        }, interval);
    }

    private object?[] MapParameters(LuaValue[] values, MethodInfo method)
    {
        List<object?> objects = new();

        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
            objects.Add(this.fromLuaValueMapper.Map(parameters[i].ParameterType, values[i]));

        return objects.ToArray();
    }

    private void HandleLuaEvent(LuaEvent luaEvent)
    {
        if (!this.handlers.TryGetValue(luaEvent.Name, out var handlers))
            return;

        foreach (var handler in handlers)
        {
            try
            {
                var hasRateLimit = handler.RateLimit != null;
                var isRateLimited = hasRateLimit &&
                    this.rateLimitTimes.TryGetValue(luaEvent.Player, out var playerRateLimitTimes) &&
                    playerRateLimitTimes.TryGetValue(handler, out var availableAfter) &&
                    availableAfter > DateTime.UtcNow;
                if (isRateLimited)
                    continue;

                if (hasRateLimit)
                    this.rateLimitTimes[luaEvent.Player][handler] = DateTime.UtcNow.Add(handler.RateLimit!.Value);

                var parameters = MapParameters(luaEvent.Parameters, handler.Method);
                var result = handler.HandleEvent(luaEvent, parameters);
                if (result != null)
                    if (result is LuaResult<object> objectResult)
                        this.luaEventService.TriggerEventFor(
                            luaEvent.Player,
                            luaEvent.Name + result.EventSuffix,
                            luaEvent.Player,
                            this.luaValueMapper.Map(objectResult.Data));
                    else
                        this.luaEventService.TriggerEventFor(luaEvent.Player, luaEvent.Name + result.EventSuffix, luaEvent.Player);
            }
            catch (Exception exception)
            {
                this.luaEventService.TriggerEventFor(luaEvent.Player, luaEvent.Name + ".Error", luaEvent.Player);
                this.logger.LogError(exception, "An error occured while handling the event {event}", luaEvent.Name);
            }
        }
    }
}
