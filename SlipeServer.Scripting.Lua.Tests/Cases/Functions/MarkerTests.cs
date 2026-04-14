using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class MarkerTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreateMarker_CreatesMarkerWithDefaults(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        Marker? capturedMarker = null;
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()))
            .Callback<Element>(e => capturedMarker = (Marker)e);

        sut.RunLuaScript("createMarker(1, 2, 3)");

        capturedMarker.Should().NotBeNull();
        capturedMarker!.Position.Should().Be(new Vector3(1, 2, 3));
        capturedMarker.MarkerType.Should().Be(MarkerType.Checkpoint);
        capturedMarker.Size.Should().Be(4.0f);
        capturedMarker.Color.Should().Be(Color.FromArgb(255, 0, 0, 255));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateMarker_WithType_CreatesCorrectType(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        Marker? capturedMarker = null;
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()))
            .Callback<Element>(e => capturedMarker = (Marker)e);

        sut.RunLuaScript("""createMarker(0, 0, 0, "ring", 2, 255, 0, 0, 200)""");

        capturedMarker.Should().NotBeNull();
        capturedMarker!.MarkerType.Should().Be(MarkerType.Ring);
        capturedMarker.Size.Should().Be(2.0f);
        capturedMarker.Color.Should().Be(Color.FromArgb(200, 255, 0, 0));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMarkerCount_ReturnsCorrectCount(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));
        elementCollectionMock.Setup(x => x.GetByType<Marker>())
            .Returns([new Marker(Vector3.Zero, MarkerType.Checkpoint), new Marker(Vector3.Zero, MarkerType.Ring)]);

        sut.RunLuaScript("assertPrint(tostring(getMarkerCount()))");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMarkerType_ReturnsTypeString(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        sut.RunLuaScript("""
            local marker = createMarker(0, 0, 0, "cylinder")
            assertPrint(getMarkerType(marker))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("cylinder");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMarkerType_ChangesType(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""setMarkerType(testMarker, "corona")""");

        marker.MarkerType.Should().Be(MarkerType.Corona);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMarkerColor_ReturnsCorrectColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        sut.RunLuaScript("""
            local marker = createMarker(0, 0, 0, "checkpoint", 4, 100, 150, 200, 128)
            local r, g, b, a = getMarkerColor(marker)
            assertPrint(r .. "," .. g .. "," .. b .. "," .. a)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("100,150,200,128");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMarkerColor_ChangesColor(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""setMarkerColor(testMarker, 255, 128, 0, 200)""");

        marker.Color.Should().Be(Color.FromArgb(200, 255, 128, 0));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMarkerSize_ReturnsCorrectSize(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        sut.RunLuaScript("""
            local marker = createMarker(0, 0, 0, "checkpoint", 6)
            assertPrint(tostring(getMarkerSize(marker)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("6");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMarkerSize_ChangesSize(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""setMarkerSize(testMarker, 8)""");

        marker.Size.Should().Be(8.0f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMarkerIcon_ReturnsIconString(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        sut.RunLuaScript("""
            local marker = createMarker(0, 0, 0)
            assertPrint(getMarkerIcon(marker))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("none");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMarkerIcon_ChangesIcon(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""setMarkerIcon(testMarker, "finish")""");

        marker.MarkerIcon.Should().Be(MarkerIcon.Finish);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMarkerTarget_ReturnsTarget(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        sut.RunLuaScript("""
            local marker = createMarker(0, 0, 0, "checkpoint")
            setMarkerTarget(marker, 10, 20, 30)
            local x, y, z = getMarkerTarget(marker)
            assertPrint(x .. "," .. y .. "," .. z)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("10,20,30");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMarkerTarget_SetsTargetPosition(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""setMarkerTarget(testMarker, 5, 10, 15)""");

        marker.TargetPosition.Should().Be(new Vector3(5, 10, 15));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMarkerTargetArrowProperties_ReturnsDefaultProperties(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        sut.RunLuaScript("""
            local marker = createMarker(0, 0, 0, "checkpoint", 4)
            local r, g, b, a, size = getMarkerTargetArrowProperties(marker)
            assertPrint(r .. "," .. g .. "," .. b .. "," .. a .. "," .. size)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("255,64,64,255,2.5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMarkerTargetArrowProperties_ChangesProperties(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint) { Size = 4 }.AssociateWith(sut);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""setMarkerTargetArrowProperties(testMarker, 0, 255, 0, 200, 3)""");

        marker.TargetArrowColor.Should().Be(Color.FromArgb(200, 0, 255, 0));
        marker.TargetArrowSize.Should().Be(3.0f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementColShape_ReturnsColShapeForMarker(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<Marker>()));

        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""
            local colshape = getElementColShape(testMarker)
            assert(colshape ~= nil, "colshape should not be nil")
            assert(isElement(colshape), "colshape should be an element")
            assertPrint("pass")
            """);

        marker.ColShape.Should().NotBeNull();
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("pass");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementColShape_ReturnsNilForNonMarkerElement(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);
        sut.AddGlobal("testPed", ped);

        sut.RunLuaScript("""
            local colshape = getElementColShape(testPed)
            assert(colshape == nil, "colshape should be nil for non-marker element")
            assertPrint("pass")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("pass");
    }
}

