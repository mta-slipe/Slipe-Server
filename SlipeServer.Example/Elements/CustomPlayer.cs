using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Numerics;

namespace SlipeServer.Example.Elements;
public class CustomPlayer : Player
{
    private readonly ExplosionService explosionService;

    public bool IsClickingVehicle { get; set; }

    public CustomPlayer(ExplosionService explosionService) : base()
    {
        this.explosionService = explosionService;

        this.Wasted += HandleWasted;
    }

    private async void HandleWasted(Ped sender, Server.Elements.Events.PedWastedEventArgs e)
    {
        this.explosionService.CreateExplosion(this.position, Server.Enums.ExplosionType.Tiny);
        await Task.Delay(500);
        this.Camera.Fade(CameraFade.Out, 1.75f);
        await Task.Delay(2000);
        this.Camera.Fade(CameraFade.In, 0);
        this.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
    }

    public void SetIsCursorShowing(bool isShowing)
    {
        this.TriggerLuaEvent("Slipe.Test.RequestCursor", parameters: isShowing);
    }
}
