using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class RadarAreaTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreateRadarArea_CreatesArea(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<RadarArea> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            createRadarArea(10, 20, 100, 200)
            """);

        var area = captures.Single();
        area.Position.X.Should().Be(10);
        area.Position.Y.Should().Be(20);
        area.Size.Should().Be(new Vector2(100, 200));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetRadarAreaSize_ReturnsCorrectSize(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<RadarArea>()));

        sut.RunLuaScript("""
            local area = createRadarArea(0, 0, 50, 75)
            local w, h = getRadarAreaSize(area)
            assertPrint(w .. "," .. h)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("50,75");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetRadarAreaSize_ChangesSize(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<RadarArea> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local area = createRadarArea(0, 0, 10, 10)
            setRadarAreaSize(area, 200, 300)
            """);

        captures.Single().Size.Should().Be(new Vector2(200, 300));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetRadarAreaColor_ReturnsCorrectColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<RadarArea>()));

        sut.RunLuaScript("""
            local area = createRadarArea(0, 0, 100, 100)
            local r, g, b, a = getRadarAreaColor(area)
            assertPrint(r .. "," .. g .. "," .. b .. "," .. a)
            """);

        // Default color is Color.FromArgb(255, 0, 0, 255) → R=0,G=0,B=255,A=255
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0,0,255,255");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetRadarAreaColor_ChangesColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<RadarArea> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local area = createRadarArea(0, 0, 100, 100)
            setRadarAreaColor(area, 255, 128, 0, 200)
            """);

        var color = captures.Single().Color;
        color.R.Should().Be(255);
        color.G.Should().Be(128);
        color.B.Should().Be(0);
        color.A.Should().Be(200);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsRadarAreaFlashing_ReturnsFalseByDefault(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<RadarArea>()));

        sut.RunLuaScript("""
            local area = createRadarArea(0, 0, 100, 100)
            assertPrint(tostring(isRadarAreaFlashing(area)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetRadarAreaFlashing_ChangesFlashingState(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<RadarArea> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local area = createRadarArea(0, 0, 100, 100)
            setRadarAreaFlashing(area, true)
            """);

        captures.Single().IsFlashing.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsInsideRadarArea_ReturnsTrueForPositionInside(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<RadarArea>()));

        sut.RunLuaScript("""
            local area = createRadarArea(0, 0, 100, 100)
            assertPrint(tostring(isInsideRadarArea(area, 50, 50)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsInsideRadarArea_ReturnsFalseForPositionOutside(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<RadarArea>()));

        sut.RunLuaScript("""
            local area = createRadarArea(0, 0, 100, 100)
            assertPrint(tostring(isInsideRadarArea(area, 200, 200)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }
}
