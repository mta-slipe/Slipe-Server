using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class BlipTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void GetBlipSize_ReturnsCorrectSize(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Blip>()));

        var blip = new Blip(new Vector3(1, 2, 3), BlipIcon.Marker) { Size = 5 }.AssociateWith(sut);
        sut.AddGlobal("testBlip", blip);

        sut.RunLuaScript("""
            assertPrint(tostring(getBlipSize(testBlip)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetBlipSize_ChangesSize(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Blip>()));

        var blip = new Blip(new Vector3(1, 2, 3), BlipIcon.Marker) { Size = 2 }.AssociateWith(sut);
        sut.AddGlobal("testBlip", blip);

        sut.RunLuaScript("""
            setBlipSize(testBlip, 10)
            """);

        blip.Size.Should().Be(10);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetBlipColor_ReturnsCorrectColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Blip>()));

        var blip = new Blip(new Vector3(1, 2, 3), BlipIcon.Marker) { Color = Color.FromArgb(255, 100, 150, 200) }.AssociateWith(sut);
        sut.AddGlobal("testBlip", blip);

        sut.RunLuaScript("""
            local r, g, b, a = getBlipColor(testBlip)
            assertPrint(r .. "," .. g .. "," .. b .. "," .. a)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("100,150,200,255");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetBlipColor_ChangesColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Blip>()));

        var blip = new Blip(new Vector3(1, 2, 3), BlipIcon.Marker).AssociateWith(sut);
        sut.AddGlobal("testBlip", blip);

        sut.RunLuaScript("""
            setBlipColor(testBlip, 50, 100, 150, 200)
            """);

        blip.Color.R.Should().Be(50);
        blip.Color.G.Should().Be(100);
        blip.Color.B.Should().Be(150);
        blip.Color.A.Should().Be(200);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetBlipOrdering_ReturnsCorrectOrdering(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Blip>()));

        var blip = new Blip(new Vector3(1, 2, 3), BlipIcon.Marker) { Ordering = 3 }.AssociateWith(sut);
        sut.AddGlobal("testBlip", blip);

        sut.RunLuaScript("""
            assertPrint(tostring(getBlipOrdering(testBlip)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("3");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetBlipOrdering_ChangesOrdering(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Blip>()));

        var blip = new Blip(new Vector3(1, 2, 3), BlipIcon.Marker).AssociateWith(sut);
        sut.AddGlobal("testBlip", blip);

        sut.RunLuaScript("""
            setBlipOrdering(testBlip, 7)
            """);

        blip.Ordering.Should().Be(7);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetBlipVisibleDistance_ReturnsCorrectDistance(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Blip>()));

        var blip = new Blip(new Vector3(1, 2, 3), BlipIcon.Marker, 500).AssociateWith(sut);
        sut.AddGlobal("testBlip", blip);

        sut.RunLuaScript("""
            assertPrint(tostring(getBlipVisibleDistance(testBlip)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("500");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetBlipVisibleDistance_ChangesVisibleDistance(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Blip>()));

        var blip = new Blip(new Vector3(1, 2, 3), BlipIcon.Marker).AssociateWith(sut);
        sut.AddGlobal("testBlip", blip);

        sut.RunLuaScript("""
            setBlipVisibleDistance(testBlip, 1000)
            """);

        blip.VisibleDistance.Should().Be(1000);
    }
}
