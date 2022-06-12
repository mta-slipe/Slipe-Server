using SlipeServer.LuaControllers.Attributes;
using SlipeServer.LuaControllers.Contexts;
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
    private readonly Dictionary<string, List<Func<LuaEventContext, LuaValue[], LuaResult?>>> handlers;
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
            var methods = controllerType.GetMethods();
            var controller = (BaseLuaController)this.server.Instantiate(controllerType);
            var prefix = controllerType.GetCustomAttributes<LuaControllerAttribute>().Select(x => x.EventPrefix).FirstOrDefault() ?? "";

            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes<LuaEventAttribute>();
                foreach (var attribute in attributes)
                    AddHandler(prefix + attribute.EventName, controller, method);

                if (!attributes.Any())
                    AddHandler(prefix + method.Name, controller, method);
            }
        }

    }

    private void AddHandler(string name, BaseLuaController controller, MethodInfo method)
    {
        if (!this.handlers.ContainsKey(name))
            this.handlers[name] = new();

        this.handlers[name].Add((context, values) =>
        {
            controller.SetContext(context);

            var parameters = MapParameters(values, method);

            var result = method.Invoke(controller, parameters);
            controller.SetContext(null);

            if (method.ReturnType == typeof(void))
                return null;

            if (result is LuaResult luaResult)
                return luaResult;

            return LuaResult<object?>.Success(result);
        });

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

        var context = new LuaEventContext(luaEvent.Player, luaEvent.Source, luaEvent.Name);
        foreach (var handler in handlers)
        {
            try
            {
                var result = handler.Invoke(context, luaEvent.Parameters);
                if (result != null)
                    if (result is LuaResult<object> objectResult)
                        this.luaEventService.TriggerEventFor(
                            luaEvent.Player,
                            luaEvent.Name + result.EventSuffix,
                            luaEvent.Player,
                            this.luaValueMapper.Map(objectResult.Data));
                    else
                        this.luaEventService.TriggerEventFor(luaEvent.Player, luaEvent.Name + result.EventSuffix, luaEvent.Player);
            } catch (Exception _)
            {
                this.luaEventService.TriggerEventFor(luaEvent.Player, luaEvent.Name + ".Error", luaEvent.Player);
            }
        }
    }
}
