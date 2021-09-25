using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetWaterLevelPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public float Level { get; set; }
        public bool IncludeWaterFeatures { get; set; }
        public bool IncludeWorldSea { get; set; }
        public bool IncludeOutsideWorldSea { get; set; }

        public SetWaterLevelPacket(float level, bool includeWaterFeatures = true, bool includeWorldSea = true, bool includeOutsideWorldSea = false)
        {
            this.Level = level;
            this.IncludeWaterFeatures = includeWaterFeatures;
            this.IncludeWorldSea = includeWorldSea;
            this.IncludeOutsideWorldSea = includeOutsideWorldSea;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_WORLD_WATER_LEVEL);
            builder.Write(this.Level);
            builder.Write(this.IncludeWaterFeatures);
            builder.Write(this.IncludeWorldSea);
            builder.Write(this.IncludeOutsideWorldSea);

            return builder.Build();
        }
    }
}
