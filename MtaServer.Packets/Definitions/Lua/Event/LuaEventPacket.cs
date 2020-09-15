using MtaServer.Packets;
using MtaServer.Packets.Builder;
using MtaServer.Packets.Definitions.Lua;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MtaServer.Packets.Lua.Event
{
    public class LuaEventPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_EVENT;
        public override PacketFlags Flags => throw new NotImplementedException();

        public string Name { get; set; }
        public uint ElementId { get; set; }
        public IEnumerable<LuaValue> LuaValues { get; set; }

        public LuaEventPacket()
        {
        }

        public LuaEventPacket(string name, uint elementId, IEnumerable<LuaValue> luaValues)
        {
            Name = name;
            ElementId = elementId;
            LuaValues = luaValues;
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();

            builder.WriteCompressed((ushort)this.Name.Length);
            builder.WriteStringWithoutLength(this.Name);
            builder.WriteElementId(this.ElementId);

            builder.Write(this.LuaValues);

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);

            ushort length = reader.GetCompressedUint16();
            this.Name = reader.GetStringCharacters(length);
            this.ElementId = reader.GetElementId();

            this.LuaValues = reader.GetLuaValues();
        }
    }
}
