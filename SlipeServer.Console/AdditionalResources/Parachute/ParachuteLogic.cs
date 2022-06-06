using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Repositories;
using SlipeServer.Server.Services;
using System.Linq;

namespace SlipeServer.Console.AdditionalResources.Parachute;

public class ParachuteLogic
{
    private readonly MtaServer server;
    private readonly LuaEventService luaEventService;
    private readonly ILogger logger;
    private readonly IElementRepository elementRepository;
    private readonly ParachuteResource resource;

    public ParachuteLogic(MtaServer server,
        LuaEventService luaEventService,
        ILogger logger,
        IElementRepository elementRepository)
    {
        this.server = server;
        this.luaEventService = luaEventService;
        this.logger = logger;
        this.elementRepository = elementRepository;
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

        var otherPlayers = this.elementRepository
            .GetByType<Player>()
            .Except(new Player[] { luaEvent.Player });
        this.luaEventService.TriggerEventForMany(otherPlayers, "doAddParachuteToPlayer", luaEvent.Player);
    }

    public void HandleRequestRemoveParachute(LuaEvent luaEvent)
    {
        luaEvent.Player.Weapons.Remove(Server.Enums.WeaponId.Parachute);
        this.logger.LogInformation("{player} finished parachuting", luaEvent.Player.Name);

        var otherPlayers = this.elementRepository
            .GetByType<Player>()
            .Except(new Player[] { luaEvent.Player });
        this.luaEventService.TriggerEventForMany(otherPlayers, "doAddParachuteToPlayer", luaEvent.Player);
    }
}
