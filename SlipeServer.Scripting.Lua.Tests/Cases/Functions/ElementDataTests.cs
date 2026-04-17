using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class ElementDataTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementData_StoresValue(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementData(object, "myKey", 42)
            """);

        var worldObject = captures.Single();
        var value = worldObject.GetData("myKey");
        value.Should().NotBeNull();
        value!.DoubleValue.Should().Be(42);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementData_ReturnsStoredValue(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementData(object, "score", 100)
            local value = getElementData(object, "score")
            assertPrint(tostring(value))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("100");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void GetElementData_ReturnsStoredValueForElements(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            local testMarker = createMarker(1, 2, 3, "checkpoint")
            setElementData(object, "marker", testMarker)

            local value = getElementData(object, "marker")
            assertPrint(tostring(isElement(value)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementData_ReturnsNilForMissingKey(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            local value = getElementData(object, "nonexistent")
            assertPrint(tostring(value))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementData_StringValue_StoresValue(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementData(object, "tag", "hello")
            local value = getElementData(object, "tag")
            assertPrint(value)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemoveElementData_RemovesStoredValue(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementData(object, "myKey", 42)
            removeElementData(object, "myKey")
            local value = getElementData(object, "myKey")
            assertPrint(tostring(value))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void HasElementData_ReturnsTrueForExistingKey(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementData(object, "myKey", 42)
            assertPrint(tostring(hasElementData(object, "myKey")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void HasElementData_ReturnsFalseForMissingKey(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(hasElementData(object, "nonexistent")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetAllElementData_ReturnsAllStoredData(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementData(object, "a", 1)
            setElementData(object, "b", 2)
            local data = getAllElementData(object)
            assertPrint(tostring(data["a"]))
            assertPrint(tostring(data["b"]))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("1");
        assertDataProvider.AssertPrints[1].Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AddElementDataSubscriber_SubscribesPlayer(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        LightTestPlayer player,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.AddGlobal("testPlayer", player);
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addElementDataSubscriber(object, "myKey", testPlayer)
            """);

        var worldObject = captures.Single();
        worldObject.IsPlayerSubscribedToData(player, "myKey").Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemoveElementDataSubscriber_UnsubscribesPlayer(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        LightTestPlayer player,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.AddGlobal("testPlayer", player);
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addElementDataSubscriber(object, "myKey", testPlayer)
            removeElementDataSubscriber(object, "myKey", testPlayer)
            """);

        var worldObject = captures.Single();
        worldObject.IsPlayerSubscribedToData(player, "myKey").Should().BeFalse();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void HasElementDataSubscriber_ReturnsTrueAfterSubscribe(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.AddGlobal("testPlayer", player);
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addElementDataSubscriber(object, "myKey", testPlayer)
            assertPrint(tostring(hasElementDataSubscriber(object, "myKey", testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }
}
