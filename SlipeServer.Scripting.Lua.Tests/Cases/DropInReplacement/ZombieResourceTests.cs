using FluentAssertions;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.DropInReplacement;

public class ZombieResourceTests
{
    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void StartZombiesResource_DoesNotThrow(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        var exception = Record.Exception(() => service.StartResource("zombies"));

        if (exception != null)
            throw new Exception(
                $"Starting 'zombies' resource failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void StartZombiesResource_HasNoScriptErrors(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("zombies");

        server.ScriptErrors.Should().BeEmpty();
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void StartZombiesResource_LoadsMaxZombiesSettingFromMeta(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        ISettingsRegistry registry)
    {
        service.StartResource("zombies");

        var value = registry.Get("zombies.MaxZombies");

        value.Should().NotBeNull();
        value!.IsNil.Should().BeFalse();
        value.DoubleValue.Should().Be(100);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void StartZombiesResource_LoadsStreamMethodSettingFromMeta(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        ISettingsRegistry registry)
    {
        service.StartResource("zombies");

        var value = registry.Get("zombies.StreamMethod");

        value.Should().NotBeNull();
        value!.IsNil.Should().BeFalse();
        value.DoubleValue.Should().Be(1);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void StartZombiesResource_LoadsSpeedSettingFromMeta(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        ISettingsRegistry registry)
    {
        service.StartResource("zombies");

        var value = registry.Get("zombies.Speed");

        value.Should().NotBeNull();
        value!.IsNil.Should().BeFalse();
        value.DoubleValue.Should().Be(1);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void StartZombiesResource_WithPlayerJoining_DoesNotThrow(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("zombies");

        var exception = Record.Exception(() => server.JoinFakePlayer());

        if (exception != null)
            throw new Exception(
                $"Player joining after 'zombies' started failed with {exception.GetType().Name}: {exception.Message}",
                exception);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void StartZombiesResource_WithPlayerJoining_HasNoScriptErrors(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("zombies");

        server.JoinFakePlayer();

        server.ScriptErrors.Should().BeEmpty();
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void StartZombiesResource_WhenPlayerSpawns_HasNoScriptErrors(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("zombies");

        var player = server.JoinFakePlayer();
        player.Spawn(Vector3.Zero, 0, 0, 0, 0);

        server.ScriptErrors.Should().BeEmpty();
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_WhenZombieStatusChangesToChasing_SetsZombieSyncerToTarget(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("zombies");

        var player = server.JoinFakePlayer();
        var zombie = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(server) as Ped;
        zombie!.SetData("zombie", true);
        zombie.SetData("target", player);
        zombie.SetData("status", "chasing");

        server.ScriptErrors.Should().BeEmpty();
        zombie.Syncer.Should().Be(player);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_WhenHeadboomEventTriggered_KillsZombieAndMakesItHeadless(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime)
    {
        service.StartResource("zombies");

        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(server) as Ped;
        ped!.SetData("zombie", true);
        var player = server.JoinFakePlayer();

        eventRuntime.TriggerCustomEvent("headboom", server.RootElement, ped, player, 22, 9);

        ped.IsHeadless.Should().BeTrue();
        ped.IsAlive.Should().BeFalse();
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_WhenPlayereatenEventTriggered_KillsPlayer(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime)
    {
        service.StartResource("zombies");

        var player = server.JoinFakePlayer();
        var zombie = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(server) as Ped;
        zombie!.SetData("zombie", true);

        eventRuntime.TriggerCustomEvent("playereaten", server.RootElement, player, zombie, 22, 9);

        player.IsAlive.Should().BeFalse();
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_WhenOnZombieSpawnEventTriggered_CreatesZombiePedInWorld(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        IElementCollection elements)
    {
        service.StartResource("zombies");

        var pedsBefore = elements.GetByType<Ped>().Count();

        eventRuntime.TriggerCustomEvent("onZombieSpawn", server.RootElement, 0.0f, 0.0f, 0.0f, 90.0f);

        server.ScriptErrors.Should().BeEmpty();
        var pedsAfter = elements.GetByType<Ped>().Count();
        pedsAfter.Should().BeGreaterThan(pedsBefore);
        elements.GetByType<Ped>().Should().Contain(p => p.GetData("zombie") == true);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_WhenOnZombieLostPlayerEventTriggered_StoresLastKnownTargetCoordsOnZombie(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime)
    {
        service.StartResource("zombies");

        var zombie = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(server) as Ped;
        zombie!.SetData("zombie", true);

        eventRuntime.TriggerCustomEvent("onZombieLostPlayer", zombie, 10.0f, 20.0f, 5.0f);

        zombie.GetData("Tx")?.DoubleValue.Should().Be(10);
        zombie.GetData("Ty")?.DoubleValue.Should().Be(20);
        zombie.GetData("Tz")?.DoubleValue.Should().Be(5);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_WhenZombiePedWastedByPlayer_IncrementsPlayerZombieKillCount(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service)
    {
        service.StartResource("zombies");

        var player = server.JoinFakePlayer();
        var zombie = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(server) as Ped;
        zombie!.SetData("zombie", true);

        zombie.Kill(player, DamageType.WEAPONTYPE_PISTOL, BodyPart.Head);

        player.GetData("Zombie kills")?.DoubleValue.Should().Be(1);
    }

    // --- Timer-based background behaviour tests ---

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_SetangleTimer_UpdatesZombieRotationToFaceTarget(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        IElementCollection elements,
        ManualScriptTimerService timerService)
    {
        service.StartResource("zombies");

        // Spawn zombie via event so it is added to Lua's everyZombie table
        eventRuntime.TriggerCustomEvent("onZombieSpawn", server.RootElement, 0.0f, 0.0f, 0.0f, 0.0f);
        var zombie = elements.GetByType<Ped>().FirstOrDefault(p => p.GetData("zombie") == true);
        zombie.Should().NotBeNull();

        // Place player off to the side so rotation must change from 0
        var player = server.JoinFakePlayer();
        player.Position = new Vector3(100, 0, 0);

        zombie!.SetData("target", player);
        zombie.SetData("status", "chasing");

        // Fire the setangle timer (400ms interval)
        timerService.FireTimersWithMaxInterval(400);

        server.ScriptErrors.Should().BeEmpty();
        // Zombie at (0,0,0) pointing to player at (100,0,0) => ~270 degrees
        zombie.PedRotation.Should().BeApproximately(270f, 1f);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_ClearFarZombiesTimer_RemovesZombiesFarFromAllPlayers(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        IElementCollection elements,
        ManualScriptTimerService timerService)
    {
        service.StartResource("zombies");

        // Spawn 2 zombies near origin — clearFarZombies only acts when > 1 far zombie exists
        eventRuntime.TriggerCustomEvent("onZombieSpawn", server.RootElement, 0.0f, 0.0f, 0.0f, 0.0f);
        eventRuntime.TriggerCustomEvent("onZombieSpawn", server.RootElement, 1.0f, 0.0f, 0.0f, 0.0f);
        var pedsBefore = elements.GetByType<Ped>().Count(p => p.GetData("zombie") == true && !p.IsDestroyed);
        pedsBefore.Should().BeGreaterThanOrEqualTo(2);

        // Place a player very far away (> 75 units)
        var player = server.JoinFakePlayer();
        player.Position = new Vector3(1000, 1000, 0);

        // Fire the clearFarZombies timer (3000ms interval)
        timerService.FireTimersWithMaxInterval(3000);

        server.ScriptErrors.Should().BeEmpty();
        var pedsAfter = elements.GetByType<Ped>().Count(p => p.GetData("zombie") == true && !p.IsDestroyed);
        pedsAfter.Should().BeLessThan(pedsBefore);
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_SpawnZombieTimer_SpawnsNewZombiesNearPlayer(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        ManualScriptTimerService timerService,
        TestPacketContext packetContext)
    {
        service.StartResource("zombies");

        var player = server.JoinFakePlayer();
        // Simulate the state set by the resource's outbreak/onPlayerSpawn handlers:
        // dangercount is set on connect, alreadyspawned is set on first spawn
        player.SetData("dangercount", 0);
        player.SetData("alreadyspawned", true);
        player.Spawn(Vector3.Zero, 0, 0, 0, 0);

        timerService.FireTimersWithMaxInterval(2500);

        server.ScriptErrors.Should().BeEmpty();
        packetContext.GetSentLuaEvents()
            .Should().Contain(e => e.Name == "Spawn_Placement" && e.Address == player.GetAddress());
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_ZombIdleTimer_TransitionsZombieToIdleStatus(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        IElementCollection elements,
        ManualScriptTimerService timerService)
    {
        service.StartResource("zombies");

        // Spawn a zombie so it goes through initialization timers
        eventRuntime.TriggerCustomEvent("onZombieSpawn", server.RootElement, 0.0f, 0.0f, 0.0f, 0.0f);

        var zombie = elements.GetByType<Ped>().FirstOrDefault(p => p.GetData("zombie") == true);
        zombie.Should().NotBeNull();

        // Fire all pending timers to advance past the spawn initialization timers (500ms, 1000ms, 2000ms)
        timerService.FireAllTimers();

        server.ScriptErrors.Should().BeEmpty();
        zombie!.GetData("status")?.StringValue.Should().Be("idle");
    }

    [Theory(Skip = "Resource not provided")]
    [DropInReplacementAutoDomainData]
    public void ZombiesResource_ZombieDeleteTimer_RemovesDeadZombieAfterDelay(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        IElementCollection elements,
        ManualScriptTimerService timerService)
    {
        service.StartResource("zombies");

        // Spawn a zombie via event so it is tracked in Lua's everyZombie table
        eventRuntime.TriggerCustomEvent("onZombieSpawn", server.RootElement, 0.0f, 0.0f, 0.0f, 0.0f);
        var zombie = elements.GetByType<Ped>().FirstOrDefault(p => p.GetData("zombie") == true);
        zombie.Should().NotBeNull();

        // Fire init timers so the zombie is fully registered in everyZombie
        timerService.FireAllTimers();

        var pedsAfterSpawn = elements.GetByType<Ped>().Count(p => p.GetData("zombie") == true && !p.IsDestroyed);

        // Kill the zombie first (isPedDead must be true), then set status to "dead"
        // so the onElementDataChange handler schedules Zomb_delete
        var player = server.JoinFakePlayer();
        zombie!.Kill(player, DamageType.WEAPONTYPE_PISTOL, BodyPart.Head);
        zombie.SetData("status", "dead");

        // Fire the Zomb_delete timer (10000ms interval)
        timerService.FireAllTimers();

        server.ScriptErrors.Should().BeEmpty();
        var pedsAfterDelete = elements.GetByType<Ped>().Count(p => p.GetData("zombie") == true && !p.IsDestroyed);
        pedsAfterDelete.Should().BeLessThan(pedsAfterSpawn);
    }
}
