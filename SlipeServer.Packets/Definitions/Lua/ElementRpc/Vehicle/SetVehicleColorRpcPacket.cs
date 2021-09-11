using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle
{
    public class SetVehicleColorRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public Color[] Colors { get; set; }

        public SetVehicleColorRpcPacket(uint elementId, Color[] colors)
        {
            this.ElementId = elementId;
            this.Colors = colors;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_VEHICLE_COLOR);
            builder.WriteElementId(this.ElementId);
            builder.WriteCapped((byte)this.Colors.Length, 2);
            foreach (var color in this.Colors)
                builder.Write(color);

            return builder.Build();
        }
    }
}
