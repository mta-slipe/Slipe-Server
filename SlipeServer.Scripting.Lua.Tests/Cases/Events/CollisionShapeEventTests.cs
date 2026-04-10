using AutoFixture.Xunit2;
using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class CollisionShapeEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnColShapeHit_FiresWhenElementEntersColShape(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(0, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local colshape = createColSphere(10, 0, 0, 5)
            addEventHandler("onColShapeHit", colshape, function(hitElement, matchingDimension)
                if hitElement == testPlayer then
                    assertPrint("hit:" .. tostring(matchingDimension))
                end
            end)
            """);

        player.Position = new Vector3(7, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hit:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnColShapeLeave_FiresWhenElementLeavesColShape(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(0, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local colshape = createColSphere(10, 0, 0, 5)
            addEventHandler("onColShapeLeave", colshape, function(leftElement, matchingDimension)
                if leftElement == testPlayer then
                    assertPrint("left:" .. tostring(matchingDimension))
                end
            end)
            """);

        player.Position = new Vector3(7, 0, 0);
        player.Position = new Vector3(0, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("left:true");
    }
}
