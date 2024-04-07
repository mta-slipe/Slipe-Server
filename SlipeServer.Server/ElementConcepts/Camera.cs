using SlipeServer.Packets.Definitions.Lua.Rpc.Camera;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Elements;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents the player's camera view.
/// </summary>
public class Camera
{
    private readonly Player player;

    private Element? target;
    /// <summary>
    /// The camera target, when set the camera follows this element.
    /// This only supports vehicles and players
    /// </summary>
    public Element? Target
    {
        get => this.target;
        set
        {
            if (!this.player.IsSync)
                this.player.Client.SendPacket(new SetCameraTargetPacket(value?.Id ?? this.player.Id));

            this.target = value;
            this.Position = null;
            this.LookAt = null;
        }
    }

    /// <summary>
    /// The camera's position, use SetMatrix if you want to modify this for the client
    /// </summary>
    public Vector3? Position { get; set; }

    /// <summary>
    /// The position the camera is looking at, use SetMatrix if you want to modify this for the client
    /// </summary>
    public Vector3? LookAt { get; set; }

    private byte interior;
    /// <summary>
    /// The interior the camera is rendering content of
    /// </summary>
    public byte Interior
    {
        get => this.interior;
        set
        {
            if (!this.player.IsSync)
                this.player.Client.SendPacket(new SetCameraInteriorPacket(value));

            this.interior = value;
        }
    }


    public Camera(Player player)
    {
        this.player = player;
    }

    /// <summary>
    /// Fades the camera in or out
    /// </summary>
    /// <param name="fade">Determines whether to fade in or out</param>
    /// <param name="fadeTime">Time the fade should take in seconds</param>
    /// <param name="color">color to fade (out) to</param>
    public void Fade(CameraFade fade, float fadeTime = 1, Color? color = null)
    {
        this.player.Client.SendPacket(new FadeCameraPacket(fade, fadeTime, color));
    }

    /// <summary>
    /// Sets the camera's position, direction, roll and field of view
    /// </summary>
    /// <param name="position">Position of the camera</param>
    /// <param name="lookAt">Position the camera looks at</param>
    /// <param name="roll">Camera roll, in degrees</param>
    /// <param name="fov">Camera field of view, in degrees</param>
    public void SetMatrix(Vector3 position, Vector3 lookAt, float roll = 0, float fov = 70)
    {
        this.target = null;
        this.Position = position;
        this.LookAt = lookAt;
        this.player.Client.SendPacket(new SetCameraMatrixPacket(position, lookAt, roll, fov, this.player.GetAndIncrementTimeContext()));
    }
}
