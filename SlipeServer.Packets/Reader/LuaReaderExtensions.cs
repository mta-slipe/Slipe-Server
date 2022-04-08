using SlipeServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Reader;

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

        return type switch
        {
            LuaType.Nil => new LuaValue(),
            LuaType.Boolean => GetLuaBool(reader),
            LuaType.Table => GetLuaTable(reader, knownTables),
            LuaType.TableRef => GetLuaTableByReference(reader, knownTables),
            LuaType.Number => GetLuaNumber(reader),
            LuaType.String => GetLuaString(reader),
            LuaType.LongString => GetLuaLongString(reader),
            LuaType.Userdata => GetLuaUserdata(reader),
            LuaType.LightUserdata => GetLuaUserdata(reader),
            _ => new LuaValue(),
        };
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
            return new LuaValue(reader.GetCompressedInt32());
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
