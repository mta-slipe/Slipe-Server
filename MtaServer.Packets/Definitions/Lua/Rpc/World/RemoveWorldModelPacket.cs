using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class RemoveWorldModelPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;
        public ushort ModelID { get; set; }
        public float Radius { get; set; }
        public Vector3 Position { get; set; }
        public byte Interior { get; set; }

        public RemoveWorldModelPacket(ushort model, float radius, Vector3 position, byte interior)
        {
            ModelID = model;
            Radius = radius;
            Position = position;
            Interior = interior;
        }
        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.REMOVE_WORLD_MODEL);
            builder.Write(this.ModelID);
            builder.Write(this.Radius);
            builder.Write(this.Position);
            builder.Write(this.Interior);

            return builder.Build();
        }
    }
}
