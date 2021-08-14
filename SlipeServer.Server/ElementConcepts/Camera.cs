using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SlipeServer.Server.ElementConcepts
{
    public class Camera
    {
        private readonly Player player;

        private Element? target;
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

        public Vector3? Position { get; set; }
        public Vector3? LookAt { get; set; }

        public Camera(Player player)
        {
            this.player = player;
        }

        public void Fade(CameraFade fade, float fadeTime = 1, Color? color = null)
        {
            this.player.Client.SendPacket(new FadeCameraPacket(fade, fadeTime, color));
        }

        public void SetMatrix(Vector3 position, Vector3 lookAt, float roll = 0, float fov = 0)
        {
            this.target = null;
            this.Position = position;
            this.LookAt = lookAt;
            this.player.Client.SendPacket(new SetCameraMatrixPacket(position, lookAt, roll, fov, this.player.GetAndIncrementTimeContext()));
        }
    }
}
