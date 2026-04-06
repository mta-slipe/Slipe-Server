using AutoFixture.Xunit2;
using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Scenarios;

public class GateTests
{

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void GateWithColshapeWorksAsIntended(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        IMtaServer sut)
    {
        player.Position = new Vector3(0, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local object = createObject(321, 10, 0, 0)
            local colshape = createColSphere(10, 0, 0, 5)

            addEventHandler("onColShapeHit", colshape, function(hitElement)
                if hitElement == testPlayer then
                    moveObject(object, 1000, 10, 0, 10)
                end
            end)

            addEventHandler("onColShapeLeave", colshape, function(leftElement)
                if leftElement == testPlayer then
                    moveObject(object, 1000, 10, 0, 0)
                end
            end)
            """);

        var worldObject = elementCollection.GetByType<WorldObject>().Single();
        var colShape = elementCollection.GetByType<CollisionSphere>().Single();
        worldObject.Movement.Should().BeNull();

        player.Position = new Vector3(7, 0, 0);
        worldObject.Movement.Should().NotBeNull();
        worldObject.Movement.TargetPosition.Should().Be(new Vector3(10, 0, 10));

        player.Position = new Vector3(0, 0, 0);
        worldObject.Movement.Should().NotBeNull();
        worldObject.Movement.TargetPosition.Should().Be(new Vector3(10, 0, 0));
    }
}
