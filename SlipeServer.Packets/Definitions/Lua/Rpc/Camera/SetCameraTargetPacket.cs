using System;
using System.Linq;

namespace SlipeServer.Packets.Lua.Camera
{
    public class SetCameraTargetPacket: LuaPacket
    {
        public SetCameraTargetPacket(uint targetId) : base(Enums.ElementRPCFunction.SET_CAMERA_TARGET)
        {
            Data = Data
                .Concat(new byte[] { 1 })
                .Concat(BitConverter.GetBytes(targetId))
                .ToArray();
        }
    }
}
