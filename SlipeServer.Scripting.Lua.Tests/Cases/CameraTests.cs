using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class CameraTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void SetCameraMatrix_UpdatesCameraPosition(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setCameraMatrix(testPlayer, 10, 20, 30, 1, 2, 3)
            """);

        player.Camera.Position.Should().Be(new Vector3(10, 20, 30));
        player.Camera.LookAt.Should().Be(new Vector3(1, 2, 3));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetCameraMatrix_WithRollAndFov_StoresValues(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setCameraMatrix(testPlayer, 5, 10, 15, 0, 0, 0, 45, 90)
            """);

        player.Camera.Roll.Should().Be(45);
        player.Camera.Fov.Should().Be(90);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetCameraMatrix_ReturnsCorrectValues(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setCameraMatrix(testPlayer, 1, 2, 3, 4, 5, 6, 10, 80)
            local px, py, pz, lx, ly, lz, roll, fov = getCameraMatrix(testPlayer)
            assertPrint(px .. "," .. py .. "," .. pz .. "," .. lx .. "," .. ly .. "," .. lz .. "," .. roll .. "," .. fov)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1,2,3,4,5,6,10,80");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetCameraMatrix_DefaultFovIsSeventyAndRollIsZero(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setCameraMatrix(testPlayer, 0, 0, 0)
            local _, _, _, _, _, _, roll, fov = getCameraMatrix(testPlayer)
            assertPrint(tostring(roll) .. "," .. tostring(fov))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0,70");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetCameraInterior_UpdatesInterior(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setCameraInterior(testPlayer, 3)
            """);

        player.Camera.Interior.Should().Be(3);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetCameraInterior_ReturnsCurrentInterior(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setCameraInterior(testPlayer, 5)
            assertPrint(tostring(getCameraInterior(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetCameraTarget_SetsTargetElement(
        LightTestPlayer player,
        LightTestPlayer targetPlayer,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);
        sut.AddGlobal("targetPlayer", targetPlayer);

        sut.RunLuaScript("""
            setCameraTarget(testPlayer, targetPlayer)
            """);

        player.Camera.Target.Should().Be(targetPlayer);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetCameraTarget_ReturnsTargetElement(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        LightTestPlayer targetPlayer,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);
        sut.AddGlobal("targetPlayer", targetPlayer);

        sut.RunLuaScript("""
            setCameraTarget(testPlayer, targetPlayer)
            local target = getCameraTarget(testPlayer)
            assertPrint(tostring(target == targetPlayer))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetCameraTarget_NilClearsTarget(
        LightTestPlayer player,
        LightTestPlayer targetPlayer,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);
        sut.AddGlobal("targetPlayer", targetPlayer);

        sut.RunLuaScript("""
            setCameraTarget(testPlayer, targetPlayer)
            setCameraTarget(testPlayer)
            """);

        player.Camera.Target.Should().BeNull();
    }
}
