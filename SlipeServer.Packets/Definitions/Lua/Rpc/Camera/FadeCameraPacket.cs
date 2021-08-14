using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Packets.Lua.Camera
{
    public enum CameraFade
    {
        In,
        Out
    }

    public class FadeCameraPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public CameraFade CameraFade { get; set; }
        public float FadeTime { get; set; }
        public Color Color { get; set; }

        public FadeCameraPacket()
        {

        }

        public FadeCameraPacket(CameraFade cameraFade, float fadeTime = 1, Color? color = null)
        {
            this.CameraFade = cameraFade;
            this.FadeTime = fadeTime;
            this.Color = color ?? Color.Black;
        }

        public override void Read(byte[] bytes)
        {
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.FADE_CAMERA);

            builder.Write((byte)(this.CameraFade == CameraFade.In ? 1 : 0));

            builder.Write(this.FadeTime);
            if (this.CameraFade == CameraFade.Out)
            {
                builder.Write(this.Color);
            }
            return builder.Build();
        }
    }
}
