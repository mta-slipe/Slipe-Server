using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace MtaServer.Packets.Lua.Camera
{
    public enum CameraFade
    {
        In,
        Out
    }

    public class FadeCameraPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public CameraFade CameraFade { get; set; }
        public float FadeTime { get; set; }
        public Color Color { get; set; }

        public FadeCameraPacket()
        {

        }

        public FadeCameraPacket(CameraFade cameraFade, float fadeTime = 1, Color? color = null)
        {
            CameraFade = cameraFade;
            FadeTime = fadeTime;
            Color = color ?? Color.Black;
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
            if (CameraFade == CameraFade.Out)
            {
                builder.Write(this.Color);
            }
            return builder.Build();
        }
    }
}
