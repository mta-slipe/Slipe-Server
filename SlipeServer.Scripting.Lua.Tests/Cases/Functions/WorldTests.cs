using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using SlipeServer.Server.Structs;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class WorldTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void GetGameSpeed_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getGameSpeed()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGameSpeed_UpdatesGameSpeed(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setGameSpeed(2.0)");

        gameWorld.GameSpeed.Should().BeApproximately(2.0f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetGravity_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("""
            local g = getGravity()
            assert(g > 0, "gravity should be greater than 0")
            assertPrint(tostring(g > 0))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        gameWorld.Gravity.Should().BeApproximately(0.008f, 0.0001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGravity_UpdatesGravity(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setGravity(0.016)");

        gameWorld.Gravity.Should().BeApproximately(0.016f, 0.0001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAircraftMaxHeight_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getAircraftMaxHeight()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("800");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAircraftMaxHeight_UpdatesHeight(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setAircraftMaxHeight(1200)");

        gameWorld.AircraftMaxHeight.Should().BeApproximately(1200f, 0.1f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAircraftMaxVelocity_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getAircraftMaxVelocity()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("1.5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAircraftMaxVelocity_UpdatesVelocity(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setAircraftMaxVelocity(3.0)");

        gameWorld.AircraftMaxVelocity.Should().BeApproximately(3.0f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetCloudsEnabled_ReturnsTrueByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getCloudsEnabled()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetCloudsEnabled_UpdatesClouds(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setCloudsEnabled(false)");

        gameWorld.CloudsEnabled.Should().BeFalse();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetFarClipDistance_UpdatesDistance(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setFarClipDistance(500)");

        gameWorld.FarClipDistance.Should().BeApproximately(500f, 0.1f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetFarClipDistance_ClearsDistance(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.FarClipDistance = 500;

        sut.RunLuaScript("resetFarClipDistance()");

        gameWorld.FarClipDistance.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetFogDistance_UpdatesDistance(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setFogDistance(100)");

        gameWorld.FogDistance.Should().BeApproximately(100f, 0.1f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetFogDistance_ClearsDistance(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.FogDistance = 100;

        sut.RunLuaScript("resetFogDistance()");

        gameWorld.FogDistance.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetHeatHaze_ReturnsDefaults(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local intensity, shift, minSpeed, maxSpeed, scanX, scanY, renderX, renderY, inside = getHeatHaze()
            assertPrint(tostring(intensity) .. "," .. tostring(shift) .. "," .. tostring(minSpeed))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("0,0,12");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetHeatHaze_UpdatesHeatHaze(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setHeatHaze(50, 10, 5, 30)");

        gameWorld.HeatHaze.Should().NotBeNull();
        gameWorld.HeatHaze!.Value.Intensity.Should().Be(50);
        gameWorld.HeatHaze!.Value.RandomShift.Should().Be(10);
        gameWorld.HeatHaze!.Value.MinSpeed.Should().Be(5);
        gameWorld.HeatHaze!.Value.MaxSpeed.Should().Be(30);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetHeatHaze_ClearsHeatHaze(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.HeatHaze = new HeatHaze { Intensity = 50 };

        sut.RunLuaScript("resetHeatHaze()");

        gameWorld.HeatHaze.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetInteriorSoundsEnabled_ReturnsTrueByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getInteriorSoundsEnabled()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetInteriorSoundsEnabled_UpdatesSounds(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setInteriorSoundsEnabled(false)");

        gameWorld.AreInteriorSoundsEnabled.Should().BeFalse();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetJetpackMaxHeight_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getJetpackMaxHeight()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("100");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetJetpackMaxHeight_UpdatesHeight(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setJetpackMaxHeight(200)");

        gameWorld.MaxJetpackHeight.Should().BeApproximately(200f, 0.1f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMinuteDuration_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getMinuteDuration()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1000");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMinuteDuration_UpdatesDuration(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setMinuteDuration(2000)");

        gameWorld.MinuteDuration.Should().Be(2000u);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMoonSize_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getMoonSize()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("3");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMoonSize_UpdatesMoonSize(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setMoonSize(10)");

        gameWorld.MoonSize.Should().Be(10);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetMoonSize_ResetsToDefault(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.MoonSize = 10;

        sut.RunLuaScript("resetMoonSize()");

        gameWorld.MoonSize.Should().Be(3);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetOcclusionsEnabled_ReturnsTrueByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getOcclusionsEnabled()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetOcclusionsEnabled_UpdatesOcclusions(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setOcclusionsEnabled(false)");

        gameWorld.OcclusionsEnabled.Should().BeFalse();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetRainLevel_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getRainLevel()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetRainLevel_UpdatesRainLevel(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setRainLevel(5)");

        gameWorld.RainLevel.Should().BeApproximately(5f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetRainLevel_ResetsToZero(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.RainLevel = 5;

        sut.RunLuaScript("resetRainLevel()");

        gameWorld.RainLevel.Should().Be(0f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetSunColor_UpdatesSunColor(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setSunColor(255, 0, 0, 0, 255, 0)");

        var colors = gameWorld.GetSunColor();
        colors.Should().NotBeNull();
        colors!.Value.Item1.R.Should().Be(255);
        colors!.Value.Item1.G.Should().Be(0);
        colors!.Value.Item2.G.Should().Be(255);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSunColor_AfterSet_ReturnsCorrectValues(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            setSunColor(100, 150, 200, 50, 75, 100)
            local r1, g1, b1, r2, g2, b2 = getSunColor()
            assertPrint(r1 .. "," .. g1 .. "," .. b1 .. "," .. r2 .. "," .. g2 .. "," .. b2)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("100,150,200,50,75,100");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSunSize_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getSunSize()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetSunSize_UpdatesSunSize(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setSunSize(5)");

        gameWorld.SunSize.Should().Be(5);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetSunSize_ResetsToDefault(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.SunSize = 5;

        sut.RunLuaScript("resetSunSize()");

        gameWorld.SunSize.Should().Be(1);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTime_ReturnsDefaultTime(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local h, m = getTime()
            assertPrint(tostring(h) .. "," .. tostring(m))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("0,0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTime_UpdatesTime(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setTime(14, 30)");

        var time = gameWorld.GetTime();
        time.Item1.Should().Be(14);
        time.Item2.Should().Be(30);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTrafficLightState_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getTrafficLightState()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTrafficLightState_UpdatesState(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setTrafficLightState(5)");

        gameWorld.TrafficLightState.Should().Be(TrafficLightState.AllGreen);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTrafficLightState_IntOverload_PreservesLockedState(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.SetTrafficLightState(TrafficLightState.GreenRed, true);

        sut.RunLuaScript("setTrafficLightState(2)");

        gameWorld.TrafficLightState.Should().Be(TrafficLightState.AllRed);
        gameWorld.AreTrafficLightsForced.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTrafficLightState_AutoString_UnlocksAndSetsStateZero(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.SetTrafficLightState(TrafficLightState.AllOff, true);

        sut.RunLuaScript("setTrafficLightState('auto')");

        gameWorld.AreTrafficLightsForced.Should().BeFalse();
        gameWorld.TrafficLightState.Should().Be(TrafficLightState.GreenRed);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTrafficLightState_DisabledString_LocksAndSetsAllOff(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setTrafficLightState('disabled')");

        gameWorld.AreTrafficLightsForced.Should().BeTrue();
        gameWorld.TrafficLightState.Should().Be(TrafficLightState.AllOff);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTrafficLightState_TwoColorStrings_LocksAndComputesState(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setTrafficLightState('red', 'green')");

        gameWorld.AreTrafficLightsForced.Should().BeTrue();
        gameWorld.TrafficLightState.Should().Be(TrafficLightState.RedGreen);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTrafficLightState_TwoColorStrings_AllCombinations(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("""
            assert(setTrafficLightState('green',  'red')    == true)
            assert(setTrafficLightState('yellow', 'red')    == true)
            assert(setTrafficLightState('red',    'red')    == true)
            assert(setTrafficLightState('red',    'green')  == true)
            assert(setTrafficLightState('red',    'yellow') == true)
            assert(setTrafficLightState('green',  'green')  == true)
            assert(setTrafficLightState('yellow', 'yellow') == true)
            assert(setTrafficLightState('yellow', 'green')  == true)
            assert(setTrafficLightState('green',  'yellow') == true)
            """);

        gameWorld.TrafficLightState.Should().Be(TrafficLightState.GreenYellow);
        gameWorld.AreTrafficLightsForced.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AreTrafficLightsLocked_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(areTrafficLightsLocked()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTrafficLightsLocked_UpdatesLockState(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setTrafficLightsLocked(true)");

        gameWorld.AreTrafficLightsForced.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWeather_ReturnsDefaultWeather(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local w, prev = getWeather()
            assertPrint(tostring(w) .. "," .. tostring(prev))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("0,0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWeather_UpdatesWeather(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setWeather(5)");

        gameWorld.Weather.Should().Be(5);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWindVelocity_ReturnsDefaultZero(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local x, y, z = getWindVelocity()
            assertPrint(x .. "," .. y .. "," .. z)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("0,0,0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWindVelocity_UpdatesVelocity(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setWindVelocity(1, 2, 3)");

        gameWorld.WindVelocity.Should().Be(new Vector3(1, 2, 3));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetWindVelocity_ResetsToZero(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.WindVelocity = new Vector3(1, 2, 3);

        sut.RunLuaScript("resetWindVelocity()");

        gameWorld.WindVelocity.Should().Be(Vector3.Zero);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetSkyGradient_UpdatesGradient(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setSkyGradient(10, 20, 30, 40, 50, 60)");

        var gradient = gameWorld.GetSkyGradient();
        gradient.Should().NotBeNull();
        gradient!.Value.Item1.R.Should().Be(10);
        gradient!.Value.Item2.B.Should().Be(60);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetSkyGradient_AfterSet_ReturnsCorrectValues(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            setSkyGradient(10, 20, 30, 40, 50, 60)
            local r1, g1, b1, r2, g2, b2 = getSkyGradient()
            assertPrint(r1 .. "," .. g1 .. "," .. b1 .. "," .. r2 .. "," .. g2 .. "," .. b2)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("10,20,30,40,50,60");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetSkyGradient_ResetsGradient(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.SetSkyGradient(Color.Red, Color.Blue);

        sut.RunLuaScript("resetSkyGradient()");

        var gradient = gameWorld.GetSkyGradient();
        gradient.Should().NotBeNull();
        gradient!.Value.Item1.R.Should().Be(0);
        gradient!.Value.Item2.B.Should().Be(255);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsWorldSpecialPropertyEnabled_HovercarsDisabledByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(isWorldSpecialPropertyEnabled("hovercars")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWorldSpecialPropertyEnabled_EnablesProperty(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("""
            setWorldSpecialPropertyEnabled("hovercars", true)
            assertPrint(tostring(isWorldSpecialPropertyEnabled("hovercars")))
            """);

        gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.Hovercars].Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetZoneName_LosAngeles_ReturnsLosAngelesCity(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getZoneName(2500, -1700, 15, true))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("Los Santos");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetZoneName_OutsideAllZones_ReturnsSanAndreas(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getZoneName(0, 0, 2000, true))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("Countryside");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsGarageOpen_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(isGarageOpen(0)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGarageOpen_OpensGarage(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setGarageOpen(0, true)");

        gameWorld.IsGarageOpen(GarageLocation.Commerce).Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemoveWorldModel_AddsModelRemoval(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("removeWorldModel(1234, 100, 0, 0, 0)");

        gameWorld.WorldObjectRemovals.Should().ContainSingle(r => r.Model == 1234);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RestoreAllWorldModels_ClearsAllRemovals(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.RemoveWorldModel(1234, Vector3.Zero, 100);
        gameWorld.RemoveWorldModel(5678, Vector3.Zero, 100);

        sut.RunLuaScript("restoreAllWorldModels()");

        gameWorld.WorldObjectRemovals.Should().BeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetJetpackWeaponEnabled_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getJetpackWeaponEnabled(31)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetJetpackWeaponEnabled_EnablesWeapon(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setJetpackWeaponEnabled(31, true)");

        gameWorld.IsJetpackWeaponEnabled(WeaponId.M4).Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetWorldProperties_ResetsGameSpeed(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.GameSpeed = 2.0f;

        sut.RunLuaScript("resetWorldProperties()");

        gameWorld.GameSpeed.Should().BeApproximately(1.0f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetWorldProperties_ResetsGravity(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.Gravity = 0.05f;

        sut.RunLuaScript("resetWorldProperties()");

        gameWorld.Gravity.Should().BeApproximately(0.008f, 0.0001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetMapInfo_ResetsGravityAndGameSpeed(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.Gravity = 0.05f;
        gameWorld.GameSpeed = 3.0f;

        sut.RunLuaScript("""
            local result = resetMapInfo()
            assert(result == true, "resetMapInfo should return true")
            """);

        gameWorld.Gravity.Should().BeApproximately(0.008f, 0.0001f);
        gameWorld.GameSpeed.Should().BeApproximately(1.0f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetMapInfo_ResetsJetpackAndAircraftLimits(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.MaxJetpackHeight = 500.0f;
        gameWorld.AircraftMaxHeight = 2000.0f;
        gameWorld.AircraftMaxVelocity = 5.0f;

        sut.RunLuaScript("resetMapInfo()");

        gameWorld.MaxJetpackHeight.Should().BeApproximately(100.0f, 0.1f);
        gameWorld.AircraftMaxHeight.Should().BeApproximately(800.0f, 0.1f);
        gameWorld.AircraftMaxVelocity.Should().BeApproximately(1.5f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetMapInfo_ResetsWeatherEffects(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.RainLevel = 5.0f;
        gameWorld.SunSize = 10;
        gameWorld.WindVelocity = new Vector3(5, 5, 5);
        gameWorld.MoonSize = 10;
        gameWorld.CloudsEnabled = false;

        sut.RunLuaScript("resetMapInfo()");

        gameWorld.RainLevel.Should().BeApproximately(0f, 0.001f);
        gameWorld.SunSize.Should().Be(1);
        gameWorld.WindVelocity.Should().Be(Vector3.Zero);
        gameWorld.MoonSize.Should().Be(3);
        gameWorld.CloudsEnabled.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetMapInfo_ClearsNullableWorldProperties(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.WaterColor = Color.FromArgb(255, 0, 128, 255);
        gameWorld.FarClipDistance = 1000.0f;
        gameWorld.FogDistance = 200.0f;

        sut.RunLuaScript("resetMapInfo()");

        gameWorld.WaterColor.Should().BeNull();
        gameWorld.FarClipDistance.Should().BeNull();
        gameWorld.FogDistance.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetMapInfo_ResetsTrafficLightsAndInteriorSounds(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.SetTrafficLightState(TrafficLightState.AllRed, true);
        gameWorld.AreInteriorSoundsEnabled = false;

        sut.RunLuaScript("resetMapInfo()");

        gameWorld.AreTrafficLightsForced.Should().BeFalse();
        gameWorld.TrafficLightState.Should().Be(TrafficLightState.GreenRed);
        gameWorld.AreInteriorSoundsEnabled.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetMapInfo_ResetsHeatHazeAndWaterLevel(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.HeatHaze = new HeatHaze { Intensity = 100 };
        gameWorld.WaveHeight = 3.0f;

        sut.RunLuaScript("resetMapInfo()");

        gameWorld.HeatHaze.Should().BeNull();
        gameWorld.WaveHeight.Should().BeApproximately(0.0f, 0.001f);
        gameWorld.WaterLevels.SeaLevel.Should().BeApproximately(0.0f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetMapInfo_WithPlayerArg_StillResetsGlobalState(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.Gravity = 0.05f;

        sut.RunLuaScript("""
            local result = resetMapInfo(root)
            assert(result == true, "resetMapInfo(root) should return true")
            """);

        gameWorld.Gravity.Should().BeApproximately(0.008f, 0.0001f);
    }
}
