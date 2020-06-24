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

    public class FadeCameraPacket : LuaPacket
    {
        public FadeCameraPacket(CameraFade cameraFade, float fadeTime = 1, byte red = 0, byte green = 0, byte blue = 0) : base(ElementRPCFunction.FADE_CAMERA)
        {
            Data = Data
                .Concat(new byte[] { (byte)(cameraFade == CameraFade.In ? 1 : 0) })
                .Concat(BitConverter.GetBytes(fadeTime))
                .ToArray();

            if (cameraFade == CameraFade.Out)
            {
                Data = Data
                    .Concat(new byte[] { red, green, blue })
                    .ToArray();
            }
        }


    }
}
