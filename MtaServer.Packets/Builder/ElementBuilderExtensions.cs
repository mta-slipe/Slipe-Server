using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Builder
{
    public static class ElementBuilderExtensions
    {
        public static void WritePlayerHealth(this PacketBuilder builder, float health)
            => builder.WriteFloatFromBits(health, 8, 0, 255, true, false);

        public static void WriteVehicleHealth(this PacketBuilder builder, float health)
            => builder.WriteFloatFromBits(health, 12, 0, 2047.5f, true, false);

        public static void WritePlayerArmor(this PacketBuilder builder, float armor)
            => builder.WriteFloatFromBits(armor, 8, 0, 127.5f, true, false);
    }
}
