using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Packets.Lua.Camera
{
    public class SetCameraMatrixPacket : LuaPacket
    {
        public SetCameraMatrixPacket(Vector3 position, Vector3 lookAt, float roll, float fov, byte timeContext) : base(Enums.ElementRPCFunction.SET_CAMERA_MATRIX)
        {
            Data = Data
                .Concat(new byte[] { timeContext })
                .Concat(BitConverter.GetBytes(position.X))
                .Concat(BitConverter.GetBytes(position.Y))
                .Concat(BitConverter.GetBytes(position.Z))
                .Concat(BitConverter.GetBytes(lookAt.X))
                .Concat(BitConverter.GetBytes(lookAt.Y))
                .Concat(BitConverter.GetBytes(lookAt.Z))
                .Concat(BitConverter.GetBytes(roll))
                .Concat(BitConverter.GetBytes(fov))
                .ToArray();
        }
    }
}
