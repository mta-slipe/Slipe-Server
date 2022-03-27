using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle
{
    public class SetVehicleHeadlightColorRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public Color Color { get; set; }

        public SetVehicleHeadlightColorRpcPacket(uint elementId, Color color)
        {
            this.ElementId = elementId;
            this.Color = color;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_VEHICLE_HEADLIGHT_COLOR);
            builder.WriteElementId(this.ElementId);
            builder.Write(Color);
            return builder.Build();
        }
    }
}
