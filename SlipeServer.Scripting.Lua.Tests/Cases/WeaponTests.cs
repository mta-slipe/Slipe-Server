using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class WeaponTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void GiveWeapon_GivesWeaponToPed(IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("giveWeapon(testPed, 31, 200)");

        ped.Weapons.Should().Contain(w => w.Type == WeaponId.M4 && w.Ammo == 200);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GiveWeapon_SetAsCurrent_UpdatesCurrentSlot(IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("giveWeapon(testPed, 31, 100, true)");

        ped.CurrentWeaponSlot.Should().Be(WeaponSlot.AssaultRifles);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TakeAllWeapons_RemovesAllWeapons(IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        ped.AddWeapon(WeaponId.M4, 200);
        ped.AddWeapon(WeaponId.Shotgun, 10);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("takeAllWeapons(testPed)");

        ped.Weapons.Should().BeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TakeWeapon_RemovesSpecificWeapon(IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        ped.AddWeapon(WeaponId.M4, 200);
        ped.AddWeapon(WeaponId.Shotgun, 10);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("takeWeapon(testPed, 31)");

        ped.Weapons.Should().NotContain(w => w.Type == WeaponId.M4);
        ped.Weapons.Should().Contain(w => w.Type == WeaponId.Shotgun);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWeaponAmmo_UpdatesAmmo(IMtaServer sut)
    {
        var ped = new Ped(PedModel.Male01, Vector3.Zero).AssociateWith(sut);
        ped.AddWeapon(WeaponId.M4, 200);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("setWeaponAmmo(testPed, 31, 50)");

        var weapon = ped.Weapons.FirstOrDefault(w => w.Type == WeaponId.M4);
        weapon.Should().NotBeNull();
        weapon!.Ammo.Should().Be(50);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWeaponNameFromID_ReturnsCorrectName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getWeaponNameFromID(31))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("m4");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWeaponIDFromName_ReturnsCorrectId(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getWeaponIDFromName("m4")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("31");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSlotFromWeapon_ReturnsCorrectSlot(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getSlotFromWeapon(31)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be(((int)WeaponSlot.AssaultRifles).ToString());
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWeaponProperty_ReturnsRange(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local range = getWeaponProperty(31, "poor", "weapon_range")
            assert(range ~= nil, "weapon_range should not be nil")
            assertPrint(tostring(range > 0))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetOriginalWeaponProperty_ReturnsOriginalValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local original = getOriginalWeaponProperty(31, "poor", "weapon_range")
            setWeaponProperty(31, "poor", "weapon_range", 999)
            local modified = getWeaponProperty(31, "poor", "weapon_range")
            local orig2 = getOriginalWeaponProperty(31, "poor", "weapon_range")
            assertPrint(tostring(original == orig2))
            assertPrint(tostring(modified > 900))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWeaponProperty_UpdatesProperty(IMtaServer sut)
    {
        sut.RunLuaScript("""
            setWeaponProperty(31, "poor", "damage", 100)
            local dmg = getWeaponProperty(31, "poor", "damage")
            assert(dmg == 100, "damage should be 100 after set")
            """);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWeaponNameFromID_UnknownId_ReturnsUnknown(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getWeaponNameFromID(999))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("unknown");
    }
}
