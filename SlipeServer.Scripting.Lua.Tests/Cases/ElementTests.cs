using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class ElementTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementPosition_RepositionsElement(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""            
            local object = createObject(321, 3, 4, 5)
            setElementPosition(object, 6, 7, 8)            
            """);            

        elementCollectionMock.Verify(x => x.Add(It.IsAny<WorldObject>()), Times.Once);

        var worldObject = captures.Single();
        worldObject.Model.Should().Be(321);
        worldObject.Position.Should().Be(new Vector3(6, 7, 8));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementPosition_ReturnsAppropriatePosition(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""            
            local object = createObject(321, 3, 4, 5)
            local x, y, z = getElementPosition(object)

            assertPrint(x .. ", " .. y .. ", " .. z)
            """);

        elementCollectionMock.Verify(x => x.Add(It.IsAny<WorldObject>()), Times.Once);
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("3, 4, 5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementModel_ReturnsAppropriateModel(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            local model = getElementModel(object)

            assertPrint(tostring(model))
            """);

        elementCollectionMock.Verify(x => x.Add(It.IsAny<WorldObject>()), Times.Once);
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("321");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementModel_ChangesModel(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementModel(object, 400)
            """);

        elementCollectionMock.Verify(x => x.Add(It.IsAny<WorldObject>()), Times.Once);

        var worldObject = captures.Single();
        worldObject.Model.Should().Be(400);
    }
}
