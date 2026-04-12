using FluentAssertions;
using SlipeServer.DropInReplacement.PacketHandlers;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Scenarios;

/// <summary>
/// Verifies that calling cancelEvent() inside an onPickupUse handler prevents the pickup's
/// effects (health/armor/weapon) from being applied to the player.
/// ScriptingPickupBehaviour must be constructed BEFORE AssociateWith(sut) so that its
/// BeforeUsed reset subscription is registered first, ensuring cancelEvent() state is
/// cleared before each Lua callback chain runs.
/// </summary>
public class PickupUseCancellationTests
{
    private static ScriptingPickupBehaviour CreateBehaviour(IMtaServer sut) =>
        new(
            sut,
            sut.GetRequiredService<IElementCollection>(),
            sut.GetRequiredService<IScriptEventRuntime>()
        );

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnPickupUse_OnPickup_PreventsEffect(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var behaviour = CreateBehaviour(sut);
        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 50).AssociateWith(sut);
        player.Health = 50;

        sut.AddGlobal("testPickup", pickup);
        sut.RunLuaScript("""
            addEventHandler("onPickupUse", testPickup, function(p)
                assertPrint("event fired")
                cancelEvent()
            end)
            """);

        pickup.Use(player);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        player.Health.Should().Be(50);
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnPickupUse_OnRoot_PreventsEffect(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var behaviour = CreateBehaviour(sut);
        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 50).AssociateWith(sut);
        player.Health = 50;

        sut.RunLuaScript("""
            addEventHandler("onPickupUse", root, function(p)
                assertPrint("event fired")
                cancelEvent()
            end)
            """);

        pickup.Use(player);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        player.Health.Should().Be(50);
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void NoCancelEvent_InOnPickupUse_AllowsEffect(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var behaviour = CreateBehaviour(sut);
        var pickup = new Pickup(Vector3.Zero, PickupType.Health, 50).AssociateWith(sut);
        player.Health = 50;

        sut.AddGlobal("testPickup", pickup);
        sut.RunLuaScript("""
            addEventHandler("onPickupUse", testPickup, function(p)
                assertPrint("event fired")
            end)
            """);

        pickup.Use(player);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        player.Health.Should().Be(100);
    }
}
