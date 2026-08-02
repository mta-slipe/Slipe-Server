using SlipeServer.Server;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Logic;

public class ColCuboidTestLogic
{
    private readonly ChatBox chatBox;

    public ColCuboidTestLogic(
        IMtaServer server,
        ChatBox chatBox
    )
    {
        var cuboid = new CollisionCuboid(new(0, 0, 3), new(5, 10, 3)).AssociateWith(server);

        cuboid.ElementEntered += HandleColCuboidEnter;
        this.chatBox = chatBox;
    }

    private void HandleColCuboidEnter(CollisionShape sender, CollisionShapeHitEventArgs e)
    {
        this.chatBox.Output($"An {e.Element} has entered the cuboid.");
    }

}
