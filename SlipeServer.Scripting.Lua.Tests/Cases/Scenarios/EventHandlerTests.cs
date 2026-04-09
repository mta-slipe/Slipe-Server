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

public class EventHandlerTests
{

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void EventHandlerRegisteredOnRootBeforeElementCreation_HandlesEvents(
        [Frozen] IElementCollection elementCollection,
        AssertDataProvider assertDataProvider,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        IMtaServer sut)
    {
        player.Position = new Vector3(0, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""           
            
            addEventHandler("onColShapeHit", root, function(hitElement)
                assertPrint("Colshape was hit")
            end)
            
            addEventHandler("onColShapeLeave", root, function(leftElement)
                assertPrint("Colshape was left")
            end)
            
            local colshape = createColSphere(10, 0, 0, 5)
            """);

        var colShape = elementCollection.GetByType<CollisionSphere>().Single();

        player.Position = new Vector3(7, 0, 0);
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Colshape was hit");


        player.Position = new Vector3(0, 0, 0);
        assertDataProvider.AssertPrints.Last().Should().Be("Colshape was left");
    }
}
