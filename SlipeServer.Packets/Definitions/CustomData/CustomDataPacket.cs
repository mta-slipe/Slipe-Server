using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Packets.Definitions.CustomData
{
    public class CustomDataPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_CUSTOM_DATA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;
        public uint ElementId { get; set; }
        public LuaValue Value { get; set; }
        public string Name { get; set; }
        public ushort MaxDataNameLength { get; set; } = 128;

        public CustomDataPacket()
        {

        }

        public CustomDataPacket(uint elementId, string name, LuaValue value)
        {
            this.ElementId = elementId;
            this.Name = name;
            this.Value = value;
        }

        public override byte[] Write() => throw new NotImplementedException();

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.ElementId = reader.GetElementId();

            ushort nameLength = reader.GetCompressedUint16();

            if (nameLength > 0 && nameLength <= MaxDataNameLength)
            {
                this.Name = reader.GetStringCharacters(nameLength);

                this.Value = reader.GetLuaValue();
            }
        }
    }
}
