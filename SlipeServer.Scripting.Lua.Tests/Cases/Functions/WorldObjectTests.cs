using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class WorldObjectTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreateObject_CreatesWorldObject(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("createObject(321, 3, 4, 5)");

        elementCollectionMock.Verify(x => x.Add(It.IsAny<WorldObject>()), Times.Once);

        var worldObject = captures.Single();
        worldObject.Model.Should().Be(321);
        worldObject.Position.Should().Be(new Vector3(3, 4, 5));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void MoveObject_StartsMovement(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            moveObject(object, 5000, 6, 7, 8)
            """);

        var worldObject = captures.Single();
        worldObject.Movement.Should().NotBeNull();
        worldObject.Movement!.TargetPosition.Should().Be(new Vector3(6, 7, 8));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsObjectMoving_ReturnsFalseWhenNotMoving(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(isObjectMoving(object)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsObjectMoving_ReturnsTrueWhileMoving(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            moveObject(object, 5000, 6, 7, 8)
            assertPrint(tostring(isObjectMoving(object)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void StopObject_CancelsMovement(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            moveObject(object, 5000, 6, 7, 8)
            stopObject(object)
            """);

        var worldObject = captures.Single();
        worldObject.Movement.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetObjectScale_ReturnsDefaultScale(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            local x, y, z = getObjectScale(object)
            assertPrint(tostring(x) .. " " .. tostring(y) .. " " .. tostring(z))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1 1 1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetObjectScale_UniformScale_SetsAllAxes(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setObjectScale(object, 2)
            """);

        var worldObject = captures.Single();
        worldObject.Scale.X.Should().Be(2f);
        worldObject.Scale.Y.Should().Be(2f);
        worldObject.Scale.Z.Should().Be(2f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetObjectScale_NonUniformScale_SetsIndividualAxes(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setObjectScale(object, 1, 2, 3)
            """);

        var worldObject = captures.Single();
        worldObject.Scale.X.Should().Be(1f);
        worldObject.Scale.Y.Should().Be(2f);
        worldObject.Scale.Z.Should().Be(3f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsObjectBreakable_ReturnsFalseByDefault(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(isObjectBreakable(object)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetObjectBreakable_SetsBreakable(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setObjectBreakable(object, true)
            """);

        var worldObject = captures.Single();
        worldObject.IsBreakable.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BreakObject_SetsBroken(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            breakObject(object)
            """);

        var worldObject = captures.Single();
        worldObject.IsBroken.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RespawnObject_ClearsBrokenState(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            breakObject(object)
            respawnObject(object)
            """);

        var worldObject = captures.Single();
        worldObject.IsBroken.Should().BeFalse();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ToggleObjectRespawn_EnablesRespawn(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            toggleObjectRespawn(object, true)
            """);

        var worldObject = captures.Single();
        worldObject.IsRespawnable.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsObjectRespawnable_ReturnsFalseByDefault(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(isObjectRespawnable(object)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }
}

