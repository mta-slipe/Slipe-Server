using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Packets.Lua.Camera
{
    public class SetCameraMatrixPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public Vector3 Position { get; set; }
        public Vector3 LookAt { get; set; }
        public float Roll { get; set; }
        public float Fov { get; set; }
        public byte TimeContext { get; set; }
        public SetCameraMatrixPacket(Vector3 position, Vector3 lookAt, float roll, float fov, byte timeContext)
        {
            this.Position = position;
            this.LookAt = lookAt;
            this.Roll = roll;
            this.Fov = fov;
            this.TimeContext = timeContext;
        }

        public override void Read(byte[] bytes)
        {
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_CAMERA_MATRIX);
            builder.Write(this.TimeContext);
            builder.Write(this.Position);
            builder.Write(this.LookAt);
            builder.Write(this.Roll);
            builder.Write(this.Fov);
            return builder.Build();
        }
    }
}
