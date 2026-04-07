using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using SlipeServer.Server.Structs;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class WaterTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreateWater_Triangle_CreatesWater(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Water> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local w = createWater(0, 0, 0, 100, 0, 0, 50, 100, 0)
            assert(w ~= nil, "createWater should return a water element")
            assert(isElement(w), "createWater result should be an element")
            """);

        captures.Should().ContainSingle();
        captures[0].Vertices.Should().HaveCount(3);
        captures[0].Vertices.Should().Contain(new Vector3(0, 0, 0));
        captures[0].Vertices.Should().Contain(new Vector3(100, 0, 0));
        captures[0].Vertices.Should().Contain(new Vector3(50, 100, 0));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateWater_Quad_CreatesWater(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<Water> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local w = createWater(0, 0, 0, 100, 0, 0, 0, 100, 0, 100, 100, 0)
            assert(w ~= nil, "createWater should return a water element")
            assert(isElement(w), "createWater result should be an element")
            """);

        captures.Should().ContainSingle();
        captures[0].Vertices.Should().HaveCount(4);
        captures[0].Vertices.Should().Contain(new Vector3(0, 0, 0));
        captures[0].Vertices.Should().Contain(new Vector3(100, 0, 0));
        captures[0].Vertices.Should().Contain(new Vector3(0, 100, 0));
        captures[0].Vertices.Should().Contain(new Vector3(100, 100, 0));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWaterVertexPosition_ReturnsCorrectPosition(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<Water> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local w = createWater(10, 20, 5, 110, 20, 5, 60, 120, 5)
            local x, y, z = getWaterVertexPosition(w, 1)
            assertPrint(x .. "," .. y .. "," .. z)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("10,20,5");
        captures.Should().ContainSingle();
        captures[0].Vertices.First().Should().Be(new Vector3(10, 20, 5));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWaterVertexPosition_UpdatesVertex(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<Water> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local w = createWater(0, 0, 0, 100, 0, 0, 50, 100, 0)
            setWaterVertexPosition(w, 1, 99, 88, 7)
            local x, y, z = getWaterVertexPosition(w, 1)
            assertPrint(x .. "," .. y .. "," .. z)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("99,88,7");
        captures.Should().ContainSingle();
        captures[0].Vertices.First().Should().Be(new Vector3(99, 88, 7));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWaveHeight_ReturnsDefaultZero(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("""
            assertPrint(tostring(getWaveHeight()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("0");
        gameWorld.WaveHeight.Should().Be(0f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWaveHeight_ChangesWaveHeight(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setWaveHeight(2.5)");

        gameWorld.WaveHeight.Should().BeApproximately(2.5f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWaterColor_ChangesWaterColor(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setWaterColor(100, 150, 200, 180)");

        gameWorld.WaterColor.Should().NotBeNull();
        gameWorld.WaterColor!.Value.R.Should().Be(100);
        gameWorld.WaterColor!.Value.G.Should().Be(150);
        gameWorld.WaterColor!.Value.B.Should().Be(200);
        gameWorld.WaterColor!.Value.A.Should().Be(180);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetWaterColor_ReturnsSetColor(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("""
            setWaterColor(10, 20, 30, 40)
            local r, g, b, a = getWaterColor()
            assertPrint(r .. "," .. g .. "," .. b .. "," .. a)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("10,20,30,40");
        gameWorld.WaterColor.Should().NotBeNull();
        gameWorld.WaterColor!.Value.R.Should().Be(10);
        gameWorld.WaterColor!.Value.G.Should().Be(20);
        gameWorld.WaterColor!.Value.B.Should().Be(30);
        gameWorld.WaterColor!.Value.A.Should().Be(40);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetWaterColor_ClearsWaterColor(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.WaterColor = Color.Red;

        sut.RunLuaScript("resetWaterColor()");

        gameWorld.WaterColor.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetWaterLevel_ChangesSeaLevel(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();

        sut.RunLuaScript("setWaterLevel(10)");

        gameWorld.WaterLevels.SeaLevel.Should().BeApproximately(10f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetWaterLevel_ResetsLevels(IMtaServer sut)
    {
        var gameWorld = sut.GetRequiredService<IGameWorld>();
        gameWorld.WaterLevels = new WaterLevels { SeaLevel = 50 };

        sut.RunLuaScript("resetWaterLevel()");

        gameWorld.WaterLevels.SeaLevel.Should().BeApproximately(0f, 0.001f);
    }
}
