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
}

