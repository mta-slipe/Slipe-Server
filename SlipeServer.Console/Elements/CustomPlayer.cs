using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Elements;
using System.Numerics;
using System.Threading.Tasks;

namespace SlipeServer.Console.Elements;
public class CustomPlayer : Player
{
    public bool IsClickingVehicle { get; set; }

    public CustomPlayer() : base()
    {
        this.Wasted += async (o, args) =>
        {
            await Task.Delay(500);
            this.Camera.Fade(CameraFade.Out, 1.75f);
            await Task.Delay(2000);
            this.Camera.Fade(CameraFade.In, 0);
            this.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
        };
    }

    public void SetIsCursorShowing(bool isShowing)
    {
        this.TriggerLuaEvent("Slipe.Test.RequestCursor", parameters: isShowing);
    }
}
