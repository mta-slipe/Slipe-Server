using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class PedEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPedDamage_FiresWhenPedTriggerDamaged(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut) as Ped;

        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            addEventHandler("onPedDamage", testPed, function(loss)
                assertPrint("damage:" .. tostring(loss))
            end)
            """);

        ped!.TriggerDamaged(25.0f);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("damage:25");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPedWasted_FiresWhenPedKilled(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut) as Ped;

        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            addEventHandler("onPedWasted", testPed, function(totalAmmo, killer, killerWeapon, bodypart, stealth, animGroup, animID)
                assertPrint("wasted:" .. tostring(killerWeapon))
            end)
            """);

        ped!.Kill(null, DamageType.WEAPONTYPE_PISTOL, BodyPart.Head);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be($"wasted:{(int)DamageType.WEAPONTYPE_PISTOL}");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPedVehicleEnter_FiresWhenPedWarpsIntoVehicle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut) as Ped;
        var vehicle = new Vehicle(411, Vector3.Zero);

        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            addEventHandler("onPedVehicleEnter", testPed, function(vehicle, seat, jacked)
                assertPrint("enter:" .. tostring(seat))
            end)
            """);

        ped!.WarpIntoVehicle(vehicle);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("enter:0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPedVehicleExit_FiresWhenPedExitsVehicle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut) as Ped;
        var vehicle = new Vehicle(411, Vector3.Zero);
        ped!.WarpIntoVehicle(vehicle);

        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            addEventHandler("onPedVehicleExit", testPed, function(vehicle, seat, jacker, forcedByScript)
                assertPrint("exit:" .. tostring(seat))
            end)
            """);

        ped.RemoveFromVehicle();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("exit:0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPedWeaponSwitch_FiresWhenWeaponSlotChanged(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut) as Ped;
        ped!.AddWeapon(WeaponId.Colt, 30);

        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            addEventHandler("onPedWeaponSwitch", testPed, function(previousWeapon, currentWeapon)
                assertPrint(tostring(previousWeapon) .. ":" .. tostring(currentWeapon))
            end)
            """);

        ped.CurrentWeaponSlot = WeaponSlot.Handguns;

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be($"0:{(int)WeaponId.Colt}");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPedWeaponReload_FiresWhenWeaponReloaded(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut) as Ped;
        ped!.CurrentWeapon = new Weapon(WeaponId.Colt, 30);

        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            addEventHandler("onPedWeaponReload", testPed, function(weaponId, ammoInClip, ammo)
                assertPrint(tostring(weaponId))
            end)
            """);

        ped.ReloadWeapon();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be($"{(int)WeaponId.Colt}");
    }
}
