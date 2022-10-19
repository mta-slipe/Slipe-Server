using SlipeLua.Shared.Rpc;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace SlipeServer.SlipeLuaIntegration.SlipeServer.Services;
public class RpcService
{
    private readonly LuaValueMapper luaValueMapper;
    private readonly LuaEventService luaEventService;
    private readonly IElementCollection elementCollection;

    public RpcService(
        LuaValueMapper luaValueMapper,
        FromLuaValueMapper fromLuaValueMapper,
        LuaEventService luaEventService,
        IElementCollection elementCollection)
    {
        luaValueMapper.DefineMapper<IntegrationElement>(MapElement);
        luaValueMapper.DefineMapper(typeof(IntegrationElement<,>), MapToElement);

        fromLuaValueMapper.DefineMapper<IntegrationElement>(MapToElement);
        fromLuaValueMapper.DefineMapper(typeof(IntegrationElement<,>), MapToElement);

        this.luaValueMapper = luaValueMapper;
        this.luaEventService = luaEventService;
        this.elementCollection = elementCollection;
    }

    public void TriggerRpc(Player player, string key, IRpc rpc)
    {
        this.luaEventService.TriggerEventFor(player, key, player, rpc);
    }

    private LuaValue MapElement(IntegrationElement value)
    {
        return new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            ["MTAElement"] = value.Element == null ? new LuaValue() : (LuaValue)value.Element
        });
    }

    private LuaValue MapToElement(object obj)
    {
        if (obj is not IntegrationElement value)
            throw new Exception("Attempt to map non-element to element");

        return new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            ["MTAElement"] = value.Element == null ? new LuaValue() : (LuaValue)value.Element
        });
    }

    private IntegrationElement MapToElement(LuaValue luaValue)
    {
        if (luaValue.TableValue == null)
            throw new Exception("Attempt to map non-table lua value to element.");

        if (!luaValue.TableValue.TryGetValue("MTAElement", out var luaElement))
            throw new Exception("Attempt to map lua value without MTAElement field to element");

        if (luaElement.ElementId == null)
            throw new Exception("Attempt to map lua value with non-element MTAElement field to element");

        var element = this.elementCollection.Get(luaElement.ElementId.Value);
        return (IntegrationElement)element;
    }
}
