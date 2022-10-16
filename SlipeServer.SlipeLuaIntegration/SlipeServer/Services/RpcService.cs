using SlipeLua.Shared.Rpc;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace SlipeServer.SlipeLuaIntegration.SlipeServer.Services;
public class RpcService
{
    private readonly LuaValueMapper luaValueMapper;
    private readonly LuaEventService luaEventService;

    public RpcService(LuaValueMapper luaValueMapper, LuaEventService luaEventService)
    {
        luaValueMapper.DefineMapper<IntegrationMtaElement>(MapElement);

        this.luaValueMapper = luaValueMapper;
        this.luaEventService = luaEventService;
    }

    public void TriggerRpc(Player player, string key, IRpc rpc)
    {
        this.luaEventService.TriggerEventFor(player, key, player, rpc);
    }

    private LuaValue MapElement(IntegrationMtaElement value)
    {
        return new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            ["MTAElement"] = value.Element == null ? new LuaValue() : (LuaValue)value.Element
        });
    }
}
