using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Services;
using System.Linq;

namespace SlipeServer.Console.AdditionalResources.Parachute;

public class ParachuteLogic
{
    private readonly MtaServer server;
    private readonly LuaEventService luaEventService;
    private readonly ILogger logger;
    private readonly IElementCollection elementCollection;
    private readonly ParachuteResource resource;

    public ParachuteLogic(MtaServer server,
        LuaEventService luaEventService,
        ILogger logger,
        IElementCollection elementCollection)
    {
        this.server = server;
        this.luaEventService = luaEventService;
        this.logger = logger;
        this.elementCollection = elementCollection;
        server.PlayerJoined += HandlePlayerJoin;

        luaEventService.AddEventHandler("requestAddParachute", HandleRequestAddParachute);
        luaEventService.AddEventHandler("requestRemoveParachute", HandleRequestRemoveParachute);

        this.resource = this.server.GetAdditionalResource<ParachuteResource>();
    }

    private void HandlePlayerJoin(Player player)
    {
        this.resource.StartFor(player);
    }

    public void HandleRequestAddParachute(LuaEvent luaEvent)
    {
        this.logger.LogInformation("{player} started parachuting", luaEvent.Player.Name);

        var otherPlayers = this.elementCollection
            .GetByType<Player>()
            .Except([luaEvent.Player]);
        this.luaEventService.TriggerEventForMany(otherPlayers, "doAddParachuteToPlayer", luaEvent.Player);
    }

    public void HandleRequestRemoveParachute(LuaEvent luaEvent)
    {
        luaEvent.Player.Weapons.Remove(Server.Enums.WeaponId.Parachute);
        this.logger.LogInformation("{player} finished parachuting", luaEvent.Player.Name);

        var otherPlayers = this.elementCollection
            .GetByType<Player>()
            .Except([luaEvent.Player]);
        this.luaEventService.TriggerEventForMany(otherPlayers, "doAddParachuteToPlayer", luaEvent.Player);
    }
}
