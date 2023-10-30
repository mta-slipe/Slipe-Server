using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace SlipeServer.Console.Elements;
public class CustomPlayer : Player
{
    private readonly ExplosionService explosionService;

    public bool IsClickingVehicle { get; set; }
    public Blip Blip { get; }

    public CustomPlayer(ExplosionService explosionService, MtaServer server) : base()
    {
        this.explosionService = explosionService;

        this.Wasted += HandleWasted;

        var color = this.GetColor();
        this.Blip = this.CreateBlipFor(BlipIcon.Marker, color: color).AssociateWith(server);
        this.NametagColor = color;

        this.Disconnected += HandleDisconnect;
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

    private void HandleDisconnect(Player sender, Server.Elements.Events.PlayerQuitEventArgs e)
    {
        this.Blip.Destroy();
    }

    public void SetIsCursorShowing(bool isShowing)
    {
        this.TriggerLuaEvent("Slipe.Test.RequestCursor", parameters: isShowing);
    }

    private Color GetColor()
    {
        var random = new Random();
        return Color.FromArgb(255, random.Next(1, 255), random.Next(1, 255), random.Next(1, 255));
    }

    public override bool Destroy()
    {
        return base.Destroy();
    }
}
