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

    [Theory]
    [ScriptingAutoDomainData]
    public void DestroyElement_DestroysElement(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            destroyElement(object)
            """);

        var worldObject = captures.Single();
        worldObject.IsDestroyed.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementRotation_RotatesElement(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementRotation(object, 10, 20, 30)
            """);

        var worldObject = captures.Single();
        worldObject.Rotation.Should().Be(new Vector3(10, 20, 30));
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementRotation_ReturnsCurrentRotation(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementRotation(object, 10, 20, 30)
            local rx, ry, rz = getElementRotation(object)
            assertPrint(rx .. ", " .. ry .. ", " .. rz)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("10, 20, 30");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementType_ReturnsCorrectType(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(getElementType(object))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("object");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementAlpha_ReturnsDefaultAlpha(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(getElementAlpha(object)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("255");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementAlpha_ChangesAlpha(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementAlpha(object, 128)
            """);

        captures.Single().Alpha.Should().Be(128);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementDimension_ReturnsDefaultDimension(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(getElementDimension(object)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementDimension_ChangesDimension(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementDimension(object, 3)
            """);

        captures.Single().Dimension.Should().Be(3);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementInterior_ReturnsDefaultInterior(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(getElementInterior(object)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementInterior_ChangesInterior(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementInterior(object, 2)
            """);

        captures.Single().Interior.Should().Be(2);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementID_ChangesName(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementID(object, "myObject")
            """);

        captures.Single().Name.Should().Be("myObject");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementID_ReturnsName(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementID(object, "myObject")
            assertPrint(getElementID(object))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("myObject");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementVelocity_ReturnsVelocity(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementVelocity(object, 1, 2, 3)
            local vx, vy, vz = getElementVelocity(object)
            assertPrint(vx .. ", " .. vy .. ", " .. vz)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1, 2, 3");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementFrozen_FreezesElement(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementFrozen(object, true)
            """);

        captures.Single().IsFrozen.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsElementFrozen_ReturnsFrozenState(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(isElementFrozen(object)))
            setElementFrozen(object, true)
            assertPrint(tostring(isElementFrozen(object)))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("false");
        assertDataProvider.AssertPrints[1].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetElementCollisionsEnabled_DisablesCollisions(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementCollisionsEnabled(object, false)
            """);

        captures.Single().AreCollisionsEnabled.Should().BeFalse();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementCollisionsEnabled_ReturnsCollisionState(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(getElementCollisionsEnabled(object)))
            setElementCollisionsEnabled(object, false)
            assertPrint(tostring(getElementCollisionsEnabled(object)))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementChildren_ReturnsChildren(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local parent = createObject(321, 0, 0, 0)
            local child = createObject(321, 1, 0, 0)
            setElementParent(child, parent)
            assertPrint(tostring(getElementChildrenCount(parent)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementParent_ReturnsParent(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local parent = createObject(321, 0, 0, 0)
            local child = createObject(321, 1, 0, 0)
            setElementParent(child, parent)
            local retrieved = getElementParent(child)
            assertPrint(tostring(retrieved == parent))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AttachElements_AttachesElements(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local target = createObject(321, 5, 0, 0)
            local source = createObject(321, 0, 0, 0)
            attachElements(source, target, 1, 0, 0)
            assertPrint(tostring(isElementAttached(source)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DetachElements_DetachesElements(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local target = createObject(321, 5, 0, 0)
            local source = createObject(321, 0, 0, 0)
            attachElements(source, target)
            detachElements(source)
            assertPrint(tostring(isElementAttached(source)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementAttachedTo_ReturnsTarget(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local target = createObject(321, 5, 0, 0)
            local source = createObject(321, 0, 0, 0)
            attachElements(source, target)
            assertPrint(tostring(getElementAttachedTo(source) == target))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void GetElementByID_FindsElementByName(
        [Frozen] IElementCollection elementCollection,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementID(object, "myObj")
            local found = getElementByID("myObj")
            assertPrint(tostring(found == object))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void GetElementsByType_ReturnsMatchingElements(
        [Frozen] IElementCollection elementCollection,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            createObject(321, 0, 0, 0)
            createObject(321, 1, 0, 0)
            local objects = getElementsByType("object")
            assertPrint(tostring(#objects))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void IsElement_ReturnsTrueForElement(
        [Frozen] IElementCollection elementCollection,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(isElement(object)))
            assertPrint(tostring(isElement(nil)))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void GetRootElement_ReturnsRoot(
        [Frozen] IElementCollection elementCollection,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local root = getRootElement()
            assertPrint(tostring(isElement(root)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGetElementCallPropagation_Works(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(isElementCallPropagationEnabled(object)))
            setElementCallPropagationEnabled(object, true)
            assertPrint(tostring(isElementCallPropagationEnabled(object)))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("false");
        assertDataProvider.AssertPrints[1].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGetElementDoubleSided_Works(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            assertPrint(tostring(isElementDoubleSided(object)))
            setElementDoubleSided(object, true)
            assertPrint(tostring(isElementDoubleSided(object)))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("false");
        assertDataProvider.AssertPrints[1].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void GetElementsWithinRange_ReturnsElementsInRange(
        [Frozen] IElementCollection elementCollection,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            createObject(321, 0, 0, 0)
            createObject(321, 1, 0, 0)
            createObject(321, 100, 0, 0)
            local nearby = getElementsWithinRange(0, 0, 0, 10, "object")
            assertPrint(tostring(#nearby))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetElementMatrix_ReturnsMatrix(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            local m = getElementMatrix(object)
            assertPrint(tostring(m[4][1]) .. ", " .. tostring(m[4][2]) .. ", " .. tostring(m[4][3]))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("3, 4, 5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGetElementAngularVelocity_Works(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<WorldObject> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementAngularVelocity(object, 1, 2, 3)
            local rx, ry, rz = getElementAngularVelocity(object)
            assertPrint(rx .. ", " .. ry .. ", " .. rz)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1, 2, 3");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateElement_CreatesCustomElement(
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        List<DummyElement> captures = [];
        elementCollectionMock.Setup(x => x.Add(Capture.In(captures)));

        sut.RunLuaScript("""
            local elem = createElement("flag", "myFlag")
            assertPrint(tostring(isElement(elem)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        captures.Should().ContainSingle().Which.ElementTypeName.Should().Be("flag");
    }
}
