using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.Camera
{
    public class SetCameraInteriorPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public byte Interior { get; set; }

        public SetCameraInteriorPacket(byte interior)
        {
            Interior = interior;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_CAMERA_INTERIOR);
            builder.Write(this.Interior);

            return builder.Build();
        }
    }
}
