using MtaServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Reader
{
    public static class LuaReaderExtensions
    {
        public static IEnumerable<LuaValue> GetLuaValues(this PacketReader reader, List<LuaValue>? knownTables = null)
        {
            knownTables ??= new List<LuaValue>();

            uint count = reader.GetCompressedUInt32();
            LuaValue[] luaValues = new LuaValue[count];
            for (int i = 0; i < count; i++)
            {
                luaValues[i] = GetLuaValue(reader, knownTables);
            }

            return luaValues;
        }

        public static LuaValue GetLuaValue(this PacketReader reader, List<LuaValue>? knownTables = null)
        {
            knownTables ??= new List<LuaValue>();

            LuaType type = (LuaType)reader.GetByteCapped(4);

            switch (type)
            {
                case LuaType.Nil:
                    return new LuaValue();
                case LuaType.Boolean:
                    return GetLuaBool(reader);
                case LuaType.Table:
                    return GetLuaTable(reader, knownTables);
                case LuaType.TableRef:
                    return GetLuaTableByReference(reader, knownTables);
                case LuaType.Number:
                    return GetLuaNumber(reader);
                case LuaType.String:
                    return GetLuaString(reader);
                case LuaType.LongString:
                    return GetLuaLongString(reader);
                case LuaType.Userdata:
                    return GetLuaUserdata(reader);
                default:
                    return new LuaValue();
            }
        }

        public static LuaValue GetLuaBool(this PacketReader reader)
        {
            return new LuaValue(reader.GetBit());
        }

        public static LuaValue GetLuaTable(this PacketReader reader, List<LuaValue> knownTables)
        {
            var count = reader.GetCompressedUInt32();

            Dictionary<LuaValue, LuaValue> tableValues = new Dictionary<LuaValue, LuaValue>();
            for (int i = 0; i < count / 2; i++)
            {
                tableValues.Add(
                    GetLuaValue(reader, knownTables),
                    GetLuaValue(reader, knownTables)
                );
            }

            var table = new LuaValue(tableValues);
            knownTables.Add(table);

            return table;
        }

        public static LuaValue GetLuaTableByReference(this PacketReader reader, List<LuaValue> knownTables)
        {
            var tablePointer = reader.GetCompressedUInt64();
            return knownTables[(int)tablePointer];
        }

        public static LuaValue GetLuaNumber(this PacketReader reader)
        {
            if (reader.GetBit())
            {
                if (reader.GetBit())
                {
                    return reader.GetDouble();
                } else
                {
                    return reader.GetFloat();
                }
            } else
            {
                return new LuaValue(reader.GetCompressedUInt32());
            }
        }

        public static LuaValue GetLuaString(this PacketReader reader)
        {
            ushort length = reader.GetCompressedUint16();
            return new LuaValue(reader.GetStringCharacters(length));
        }

        public static LuaValue GetLuaLongString(this PacketReader reader)
        {
            uint length = reader.GetCompressedUInt32();
            reader.AlignToByteBoundary();

            return new LuaValue(reader.GetStringCharacters(length));
        }

        public static LuaValue GetLuaUserdata(this PacketReader reader)
        {
            return new LuaValue(reader.GetElementId());
        }
    }
}
