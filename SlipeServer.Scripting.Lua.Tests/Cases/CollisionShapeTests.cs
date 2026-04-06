using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements.ColShapes;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class CollisionShapeTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreateColSphere_CreatesCollisionSphere(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionSphere> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            createColSphere(1, 2, 3, 10)
            """);

        var sphere = captures.Single();
        sphere.Position.Should().Be(new Vector3(1, 2, 3));
        sphere.Radius.Should().Be(10);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateColCircle_CreatesCollisionCircle(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionCircle> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            createColCircle(5, 6, 15)
            """);

        var circle = captures.Single();
        circle.Position.X.Should().Be(5);
        circle.Position.Y.Should().Be(6);
        circle.Radius.Should().Be(15);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateColCuboid_CreatesCollisionCuboid(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionCuboid> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            createColCuboid(1, 2, 3, 4, 5, 6)
            """);

        var cuboid = captures.Single();
        cuboid.Position.Should().Be(new Vector3(1, 2, 3));
        cuboid.Dimensions.Should().Be(new Vector3(4, 5, 6));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateColRectangle_CreatesCollisionRectangle(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionRectangle> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            createColRectangle(1, 2, 10, 20)
            """);

        var rectangle = captures.Single();
        rectangle.Position.X.Should().Be(1);
        rectangle.Position.Y.Should().Be(2);
        rectangle.Dimensions.X.Should().Be(10);
        rectangle.Dimensions.Y.Should().Be(20);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateColTube_CreatesCollisionTube(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionTube> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            createColTube(1, 2, 3, 5, 10)
            """);

        var tube = captures.Single();
        tube.Position.Should().Be(new Vector3(1, 2, 3));
        tube.Radius.Should().Be(5);
        tube.Height.Should().Be(10);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateColPolygon_CreatesCollisionPolygon(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionPolygon> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            createColPolygon(0, 0, 1, 0, 0, 1, -1, 0)
            """);

        var polygon = captures.Single();
        // The position Vector2 is included as the first vertex due to params-array parsing
        polygon.GetPointsCount().Should().Be(4);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetColShapeType_ReturnsSphereTypeValue(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<CollisionSphere>()));

        sut.RunLuaScript("""
            local sphere = createColSphere(0, 0, 0, 10)
            assertPrint(tostring(getColShapeType(sphere)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetColShapeType_ReturnsCuboidTypeValue(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<CollisionCuboid>()));

        sut.RunLuaScript("""
            local cuboid = createColCuboid(0, 0, 0, 5, 5, 5)
            assertPrint(tostring(getColShapeType(cuboid)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetColShapeRadius_ReturnsSphereRadius(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<CollisionSphere>()));

        sut.RunLuaScript("""
            local sphere = createColSphere(0, 0, 0, 10)
            assertPrint(tostring(getColShapeRadius(sphere)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("10");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetColShapeRadius_ChangesSphereRadius(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionSphere> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local sphere = createColSphere(0, 0, 0, 10)
            setColShapeRadius(sphere, 25)
            """);

        captures.Single().Radius.Should().Be(25);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetColShapeSize_ReturnsCuboidDimensions(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<CollisionCuboid>()));

        sut.RunLuaScript("""
            local cuboid = createColCuboid(0, 0, 0, 4, 5, 6)
            local x, y, z = getColShapeSize(cuboid)
            assertPrint(x .. "," .. y .. "," .. z)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("4,5,6");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetColShapeSize_ChangesCuboidDimensions(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionCuboid> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local cuboid = createColCuboid(0, 0, 0, 1, 1, 1)
            setColShapeSize(cuboid, 10, 20)
            """);

        captures.Single().Dimensions.Should().Be(new Vector3(10, 20, 0));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsInsideColShape_ReturnsTrueForPositionInside(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<CollisionSphere>()));

        sut.RunLuaScript("""
            local sphere = createColSphere(0, 0, 0, 10)
            assertPrint(tostring(isInsideColShape(sphere, 5, 0, 0)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsInsideColShape_ReturnsFalseForPositionOutside(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<CollisionSphere>()));

        sut.RunLuaScript("""
            local sphere = createColSphere(0, 0, 0, 10)
            assertPrint(tostring(isInsideColShape(sphere, 15, 0, 0)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetColPolygonHeight_ReturnsDefaultHeight(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<CollisionPolygon>()));

        sut.RunLuaScript("""
            local poly = createColPolygon(0, 0, 1, 0, 0, 1, -1, 0)
            local minZ, maxZ = getColPolygonHeight(poly)
            assertPrint(tostring(minZ < maxZ))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetColPolygonHeight_ChangesHeight(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionPolygon> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local poly = createColPolygon(0, 0, 1, 0, 0, 1, -1, 0)
            setColPolygonHeight(poly, 5, 20)
            """);

        captures.Single().Height.Should().Be(new Vector2(5, 20));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetColPolygonPointPosition_ReturnsVertexPosition(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        elementCollectionMock.Setup(x => x.Add(It.IsAny<CollisionPolygon>()));

        sut.RunLuaScript("""
            local poly = createColPolygon(0, 0, 1, 0, 0, 1, -1, 0)
            local x, y = getColPolygonPointPosition(poly, 1)
            assertPrint(x .. "," .. y)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1,0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetColPolygonPointPosition_ChangesVertexPosition(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionPolygon> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local poly = createColPolygon(0, 0, 1, 0, 0, 1, -1, 0)
            setColPolygonPointPosition(poly, 1, 5, 6)
            """);

        captures.Single().GetPointPosition(0).Should().Be(new Vector2(5, 6));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AddColPolygonPoint_IncreasesPointCount(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionPolygon> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local poly = createColPolygon(0, 0, 1, 0, 0, 1, -1, 0)
            addColPolygonPoint(poly, 2, 2)
            """);

        // Starts at 4 vertices (position included), adding 1 gives 5
        captures.Single().GetPointsCount().Should().Be(5);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemoveColPolygonPoint_DecreasesPointCount(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<CollisionPolygon> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local poly = createColPolygon(0, 0, 1, 0, 0, 1, -1, 0, 2, 2)
            removeColPolygonPoint(poly, 1)
            """);

        // Starts at 5 vertices (position + 4 args), removing 1 gives 4
        captures.Single().GetPointsCount().Should().Be(4);
    }
}
