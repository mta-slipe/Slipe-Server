using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Elements;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public readonly record struct CameraMatrix(Vector3 Position, Vector3 LookAt, float Roll, float Fov);

public class CameraScriptDefinitions
{
    [ScriptFunctionDefinition("fadeCamera")]
    public bool FadeCamera(Player player, bool fadeIn, float timeToFade = 1.0f, int red = 0, int green = 0, int blue = 0)
    {
        var fade = fadeIn ? CameraFade.In : CameraFade.Out;
        player.Camera.Fade(fade, timeToFade, Color.FromArgb(255, red, green, blue));
        return true;
    }

    [ScriptFunctionDefinition("getCameraInterior")]
    public int GetCameraInterior(Player player)
    {
        return player.Camera.Interior;
    }

    [ScriptFunctionDefinition("getCameraMatrix")]
    public CameraMatrix GetCameraMatrix(Player player)
    {
        return new CameraMatrix(
            player.Camera.Position ?? Vector3.Zero,
            player.Camera.LookAt ?? Vector3.Zero,
            player.Camera.Roll,
            player.Camera.Fov);
    }

    [ScriptFunctionDefinition("getCameraTarget")]
    public Element? GetCameraTarget(Player player)
    {
        return player.Camera.Target;
    }

    [ScriptFunctionDefinition("setCameraInterior")]
    public bool SetCameraInterior(Player player, int interior)
    {
        player.Camera.Interior = (byte)interior;
        return true;
    }

    [ScriptFunctionDefinition("setCameraMatrix")]
    public bool SetCameraMatrix(Player player, float posX, float posY, float posZ, float lookAtX = 0, float lookAtY = 0, float lookAtZ = 0, float roll = 0, float fov = 70)
    {
        player.Camera.SetMatrix(new Vector3(posX, posY, posZ), new Vector3(lookAtX, lookAtY, lookAtZ), roll, fov);
        return true;
    }

    [ScriptFunctionDefinition("setCameraTarget")]
    public bool SetCameraTarget(Player player, Element? target = null)
    {
        player.Camera.Target = target;
        return true;
    }
}
