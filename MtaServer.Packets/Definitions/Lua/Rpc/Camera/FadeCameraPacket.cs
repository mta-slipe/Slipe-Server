using MtaServer.Packets.Enums;
using System;
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
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public FadeCameraPacket()
        {

        }

        public FadeCameraPacket(CameraFade cameraFade, float fadeTime = 1, byte red = 0, byte green = 0, byte blue = 0)
        {
            CameraFade = cameraFade;
            FadeTime = fadeTime;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.CameraFade = (CameraFade)reader.GetByte();
            if (CameraFade == CameraFade.Out)
            {
                this.Red = reader.GetByte();
                this.Green = reader.GetByte();
                this.Blue = reader.GetByte();
            }
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.FADE_CAMERA);

            builder.Write((byte)(this.CameraFade == CameraFade.In ? 1 : 0));

            builder.Write(this.FadeTime);
            if (CameraFade == CameraFade.Out)
            {
                builder.Write(this.Red);
                builder.Write(this.Green);
                builder.Write(this.Blue);
            }
            return builder.Build();
        }
    }
}
