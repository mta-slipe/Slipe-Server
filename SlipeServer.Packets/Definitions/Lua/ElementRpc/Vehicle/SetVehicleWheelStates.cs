using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle
{
    public class SetVehicleWheelStates : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public byte[] States { get; set; }

        public SetVehicleWheelStates(uint elementId, byte[] states)
        {
            this.ElementId = elementId;
            this.States = states;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_VEHICLE_WHEEL_STATES);
            builder.WriteElementId(this.ElementId);
            builder.Write(this.States);
            return builder.Build();
        }
    }
}
