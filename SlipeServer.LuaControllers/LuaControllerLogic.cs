using SlipeServer.LuaControllers.Attributes;
using SlipeServer.LuaControllers.Results;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;
using System.Reflection;

namespace SlipeServer.LuaControllers;

public class LuaControllerLogic
{
    private readonly MtaServer server;
    private readonly LuaEventService luaEventService;
    private readonly IElementCollection elementCollection;
    private readonly LuaValueMapper luaValueMapper;
    private readonly Dictionary<string, List<BoundEvent>> handlers;
    private readonly Dictionary<Type, Func<LuaValue, object>> implicitlyCastableTypes;

    public LuaControllerLogic(
        MtaServer server,
        LuaEventService luaEventService,
        IElementCollection elementCollection,
        LuaValueMapper luaValueMapper)
    {
        this.server = server;
        this.luaEventService = luaEventService;
        this.elementCollection = elementCollection;
        this.luaValueMapper = luaValueMapper;

        this.handlers = new();
        this.implicitlyCastableTypes = new();

        IndexImplicitlyCastableTypes();
        IndexControllers();
    }

    private void IndexImplicitlyCastableTypes()
    {
        foreach (var method in typeof(LuaValue).GetMethods().Where(x => x.Name == "op_Explicit"))
            this.implicitlyCastableTypes[method.ReturnType] = (value) => method.Invoke(null, new object[] { value })!;
    }

    private void IndexControllers()
    {
        var controllerTypes = Assembly
            .GetEntryAssembly()?
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(BaseLuaController)));

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
                var attributes = method.GetCustomAttributes<LuaEventAttribute>();
                foreach (var attribute in attributes)
                    AddHandler(prefix + attribute.EventName, controllerType, method, controller);

                if (!attributes.Any())
                    AddHandler(prefix + method.Name, controllerType, method, controller);
            }
        }
    }

    private void AddHandler(string name, Type type, MethodInfo method, BaseLuaController? controller)
    {
        if (!this.handlers.ContainsKey(name))
            this.handlers[name] = new();

        this.handlers[name].Add(new BoundEvent(this.server.Services, name, type, method, controller));

        this.luaEventService.AddEventHandler(name, HandleLuaEvent);
    }

    private object?[] MapParameters(LuaValue[] values, MethodInfo method)
    {
        List<object?> objects = new();

        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
            objects.Add(ConvertLuaValue(parameters[i].ParameterType, values[i]));

        return objects.ToArray();
    }

    private object? ConvertLuaValue(Type type, LuaValue value)
    {
        if (type.IsAssignableTo(typeof(ILuaValue)))
        {
            var instance = (ILuaValue)Activator.CreateInstance(type)!;
            instance.Parse(value);
            return instance;
        } else if (type.IsAssignableTo(typeof(Element)) && value.ElementId.HasValue)
            return this.elementCollection.Get(value.ElementId!.Value);
        else if (this.implicitlyCastableTypes.ContainsKey(type))
            return this.implicitlyCastableTypes[type](value);
        else if (type.IsAssignableTo(typeof(Dictionary<,>)) && value.TableValue != null)
            return value.TableValue.ToDictionary(
                x => ConvertLuaValue(type.GenericTypeArguments.First(), x.Key) ?? new object(),
                x => ConvertLuaValue(type.GenericTypeArguments.ElementAt(1), x.Value));
        else if (type.IsAssignableTo(typeof(IEnumerable<>)) && value.TableValue != null)
            return value.TableValue.Values.Select(x => ConvertLuaValue(type.GenericTypeArguments.First(), value));
        else
            return null;
    }

    private void HandleLuaEvent(LuaEvent luaEvent)
    {
        if (!this.handlers.TryGetValue(luaEvent.Name, out var handlers))
            return;

        foreach (var handler in handlers)
        {
            try
            {
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
            catch (Exception)
            {
                this.luaEventService.TriggerEventFor(luaEvent.Player, luaEvent.Name + ".Error", luaEvent.Player);
            }
        }
    }
}
