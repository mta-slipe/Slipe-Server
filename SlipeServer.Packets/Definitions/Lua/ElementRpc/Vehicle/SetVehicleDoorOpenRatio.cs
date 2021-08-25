using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle
{
    public class SetVehicleDoorOpenRatio : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public byte Door { get; set; }
        public float Ratio { get; set; }
        public uint Time { get; set; }

        public SetVehicleDoorOpenRatio(uint elementId, byte door, float ratio, uint time)
        {
            this.ElementId = elementId;
            this.Door = door;
            this.Ratio = Math.Clamp(ratio, 0.0f, 1.0f);
            this.Time = time;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_VEHICLE_DOOR_OPEN_RATIO);
            builder.WriteElementId(this.ElementId);
            builder.WriteCapped(this.Door, 3);
            if(this.Ratio == 0 || this.Ratio == 1)
            {
                builder.Write(false);
                builder.Write(this.Ratio == 1.0f);
            }
            else
            {
                builder.Write(true);
                builder.WriteFloatFromBits(this.Ratio, 10, 0, 1, true);
            }
            builder.WriteCompressed(this.Time);
            return builder.Build();
        }
    }
}
