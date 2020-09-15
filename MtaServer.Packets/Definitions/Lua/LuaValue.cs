using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MtaServer.Packets.Definitions.Lua
{
    public class LuaValue
    {
        public LuaType LuaType { get; set; }

        public bool? BoolValue { get; }
        public string? StringValue { get; }
        public float? FloatValue { get; }
        public double? DoubleValue { get; }
        public int? IntegerValue { get; }
        public uint? ElementId { get; }
        public Dictionary<LuaValue, LuaValue>? TableValue { get; }
        public bool IsNil { get; }


        public LuaValue()
        {
            this.IsNil = true;
        }

        public LuaValue(bool? value)
        {
            this.LuaType = LuaType.Boolean;
            this.BoolValue = value;
            this.IsNil = value == null;
        }

        public LuaValue(string? value)
        {
            this.LuaType = LuaType.String;
            this.StringValue = value;
            this.IsNil = value == null;
        }

        public LuaValue(float? value)
        {
            this.LuaType = LuaType.Number;
            this.FloatValue = value;
            this.IsNil = value == null;
        }

        public LuaValue(double? value)
        {
            this.LuaType = LuaType.Number;
            this.DoubleValue = value;
            this.IsNil = value == null;
        }

        public LuaValue(int? value)
        {
            this.LuaType = LuaType.Number;
            this.IntegerValue = value;
            this.IsNil = value == null;
        }

        public LuaValue(uint? value)
        {
            this.LuaType = LuaType.LightUserdata;
            this.ElementId = value;
            this.IsNil = value == null;
        }

        public LuaValue(Dictionary<LuaValue, LuaValue>? value)
        {
            this.LuaType = LuaType.Table;
            this.TableValue = value;
            this.IsNil = value == null;
        }

        public override string ToString()
        {
            if (this.TableValue != null)
                return $"{{{string.Join(", ", this.TableValue.Select(kvPair => $"{kvPair.Key}: {kvPair.Value}"))}}}";

            return
                this.IntegerValue?.ToString() ??
                this.DoubleValue?.ToString() ??
                this.FloatValue?.ToString() ??
                this.BoolValue?.ToString() ??
                this.ElementId?.ToString() ??
                this.StringValue?.ToString() ??
                "nil";
        }

        public static implicit operator LuaValue(string value) => new LuaValue(value);
        public static implicit operator LuaValue(bool value) => new LuaValue(value);
        public static implicit operator LuaValue(int value) => new LuaValue(value);
        public static implicit operator LuaValue(float value) => new LuaValue(value);
        public static implicit operator LuaValue(double value) => new LuaValue(value);
    }
}
