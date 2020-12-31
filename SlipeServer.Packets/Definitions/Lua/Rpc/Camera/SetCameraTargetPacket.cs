using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Lua.Camera
{
    public class SetCameraTargetPacket: Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }

        public SetCameraTargetPacket(uint elementId)
        {
            this.ElementId = elementId;
        }

        public override void Read(byte[] bytes)
        {
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_CAMERA_TARGET);
            builder.Write((byte)1);
            builder.WriteElementId(this.ElementId);

            return builder.Build();
        }
    }
}
