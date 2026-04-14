using FluentAssertions;
using SlipeServer.Packets.Enums;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Tests.Tools;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class PlayerEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerVehicleEnter_FiresWhenPlayerWarpsIntoVehicle(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerVehicleEnter", testPlayer, function(vehicle, seat, jacked)
                assertPrint("enter:" .. tostring(seat))
            end)
            """);

        player.WarpIntoVehicle(vehicle);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("enter:0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerVehicleExit_FiresWhenPlayerExitsVehicle(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero);
        player.WarpIntoVehicle(vehicle);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerVehicleExit", testPlayer, function(vehicle, seat, jacker, forcedByScript)
                assertPrint("exit:" .. tostring(seat))
            end)
            """);

        player.RemoveFromVehicle();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("exit:0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerVoiceStart_FiresWhenVoiceDataReceived(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerVoiceStart", testPlayer, function()
                assertPrint("voicestart")
            end)
            """);

        player.VoiceDataStart([]);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("voicestart");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerVoiceStop_FiresWhenVoiceDataEnded(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerVoiceStop", testPlayer, function()
                assertPrint("voicestop")
            end)
            """);

        player.VoiceDataEnd();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("voicestop");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerACInfo_FiresWhenACInfoReceived(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerACInfo", testPlayer, function(acList, d3d9Size, d3d9MD5, d3d9SHA256)
                assertPrint(d3d9MD5)
            end)
            """);

        player.TriggerPlayerACInfo([], 0, "testmd5hash", "testsha256hash");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("testmd5hash");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerModInfo_FiresWhenModInfoReceived(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerModInfo", testPlayer, function(infoType, modInfoItems)
                assertPrint(infoType)
            end)
            """);

        player.TriggerPlayerModInfo("vehiclemod", []);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("vehiclemod");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerNetworkStatus_FiresWhenNetworkStatusReceived(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerNetworkStatus", testPlayer, function(status, ticks)
                assertPrint(tostring(status))
            end)
            """);

        player.TriggerNetworkStatus(PlayerNetworkStatusType.InterruptionBegan, 100);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerWeaponSwitch_FiresWhenWeaponSlotChanged(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.AddWeapon(WeaponId.Colt, 30);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerWeaponSwitch", testPlayer, function(previousWeapon, currentWeapon)
                assertPrint(tostring(previousWeapon) .. ":" .. tostring(currentWeapon))
            end)
            """);

        player.CurrentWeaponSlot = WeaponSlot.Handguns;

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be($"0:{(int)WeaponId.Colt}");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerWeaponReload_FiresWhenWeaponReloaded(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.CurrentWeapon = new Weapon(WeaponId.Colt, 30);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerWeaponReload", testPlayer, function(weaponId, ammoInClip, ammo)
                assertPrint(tostring(weaponId))
            end)
            """);

        player.ReloadWeapon();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be($"{(int)WeaponId.Colt}");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerWeaponFire_FiresWhenWeaponFired(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerWeaponFire", testPlayer, function(weapon, endX, endY, endZ, hitElement, startX, startY, startZ)
                assertPrint(tostring(weapon))
            end)
            """);

        player.TriggerWeaponFired(WeaponId.Colt, Vector3.Zero, new Vector3(10, 0, 0), null);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be($"{(int)WeaponId.Colt}");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerDetonateSatchels_FiresWhenSatchelsDetonated(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerDetonateSatchels", testPlayer, function()
                assertPrint("detonated")
            end)
            """);

        player.TriggerSatchelsDetonated();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("detonated");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerChat_FiresWhenSayCommandEntered(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerChat", testPlayer, function(message, messageType)
                assertPrint(message)
            end)
            """);

        player.TriggerCommand("say", ["hello", "world"]);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello world");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerMute_FiresWhenPlayerIsMuted(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerMute", testPlayer, function()
                assertPrint("muted")
            end)
            """);

        player.IsChatMuted = true;

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("muted");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerUnmute_FiresWhenPlayerIsUnmuted(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.IsChatMuted = true;

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerUnmute", testPlayer, function()
                assertPrint("unmuted")
            end)
            """);

        player.IsChatMuted = false;

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("unmuted");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerTeamChange_FiresWhenTeamIsChanged(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var team = new Team("TestTeam", Color.Red);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerTeamChange", testPlayer, function(previousTeam, newTeam)
                if previousTeam == nil and newTeam ~= nil then
                    assertPrint("teamchanged")
                end
            end)
            """);

        player.Team = team;

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("teamchanged");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerStealthKill_FiresWhenPlayerTriggersStealthKill(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var target = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerStealthKill", testPlayer, function(targetPlayer)
                assertPrint("stealthkill")
            end)
            """);

        player.TriggerStealthKill(target);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("stealthkill");
    }
}
