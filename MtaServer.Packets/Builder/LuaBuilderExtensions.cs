using MtaServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Builder
{
    public static class LuaBuilderExtensions
    {
        public static void Write(this PacketBuilder builder, LuaValue luaValue, Dictionary<LuaValue, ulong>? knownTables = null)
        {
            knownTables ??= new Dictionary<LuaValue, ulong>();

            switch (luaValue.LuaType)
            {
                case LuaType.Nil:
                    WriteLuaNil(builder);
                    break;
                case LuaType.Boolean:
                    WriteLuaBool(builder, luaValue);
                    break;
                case LuaType.Table:
                    WriteLuaTable(builder, luaValue, knownTables);
                    break;
                case LuaType.Number:
                    WriteLuaNumber(builder, luaValue);
                    break;
                case LuaType.String:
                    WriteLuaString(builder, luaValue);
                    break;
                case LuaType.Userdata:
                    WriteLuaUserdata(builder, luaValue);
                    break;
                default:
                    WriteLuaNil(builder);
                    break;
            }
        }

        public static void Write(this PacketBuilder builder, IEnumerable<LuaValue> luaValues, Dictionary<LuaValue, ulong>? knownTables = null)
        {
            knownTables ??= new Dictionary<LuaValue, ulong>();
            knownTables[new LuaValue()] = 0;

            builder.WriteCompressed((uint)luaValues.Count());
            foreach (var value in luaValues)
            {
                builder.Write(value, knownTables);
            }
        }

        private static void WriteLuaNil(PacketBuilder builder)
        {
            builder.WriteCapped((byte)LuaType.Nil, 4);
        }

        private static void WriteLuaBool(PacketBuilder builder, LuaValue value)
        {
            if (value.BoolValue.HasValue)
            {
                builder.WriteCapped((byte)value.LuaType, 4);
                builder.Write(value.BoolValue.Value);
            }
        }

        private static void WriteLuaTable(PacketBuilder builder, LuaValue value, Dictionary<LuaValue, ulong> knownTables)
        {
            if (value.TableValue != null)
            {
                if (knownTables.ContainsKey(value))
                {
                    builder.WriteCapped((byte)LuaType.TableRef, 4);
                    builder.WriteCompressed(knownTables[value]);
                }
                else
                {
                    knownTables.Add(value, (ulong)knownTables.Count());

                    builder.WriteCapped((byte)LuaType.Table, 4);

                    builder.WriteCompressed((uint)value.TableValue.Count() * 2);
                    foreach (var kvPair in value.TableValue)
                    {
                        Write(builder, kvPair.Key, knownTables);
                        Write(builder, kvPair.Value, knownTables);
                    }
                }
            }
        }

        private static void WriteLuaNumber(PacketBuilder builder, LuaValue value)
        {
            builder.WriteCapped((byte)value.LuaType, 4);
            if (value.IntegerValue.HasValue)
            {
                builder.Write(false);
                builder.WriteCompressed(value.IntegerValue.Value);
            }
            else if (value.FloatValue.HasValue)
            {
                builder.Write(true);
                builder.Write(false);
                builder.Write(value.FloatValue.Value);
            }
            else if (value.DoubleValue.HasValue)
            {
                builder.Write(true);
                builder.Write(true);
                builder.Write(value.DoubleValue.Value);
            }
        }

        private static void WriteLuaString(PacketBuilder builder, LuaValue value)
        {
            if (value.StringValue != null)
            {
                if (value.StringValue.Length < ushort.MaxValue)
                {
                    builder.WriteCapped((byte)LuaType.String, 4);
                    builder.WriteCompressed((ushort)value.StringValue.Length);
                    builder.WriteStringWithoutLength(value.StringValue);
                } else
                {
                    builder.WriteCapped((byte)LuaType.LongString, 4);
                    builder.WriteCompressed((uint)value.StringValue.Length);
                    builder.AlignToByteBoundary();
                    builder.WriteStringWithoutLength(value.StringValue);
                }
            }
        }

        private static void WriteLuaUserdata(PacketBuilder builder, LuaValue value)
        {
            if (value.ElementId != null)
            {
                if (value.IsNil)
                {
                    WriteLuaNil(builder);
                } else
                {
                    builder.WriteCapped((byte)value.LuaType, 4);
                    builder.WriteElementId(value.ElementId.Value);
                }
            }
        }
    }
}
