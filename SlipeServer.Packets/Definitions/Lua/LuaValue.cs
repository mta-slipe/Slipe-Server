using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua;

[DebuggerDisplay("LuaValue")]
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
    public static LuaValue Nil { get; } = new LuaValue();

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
        this.LuaType = LuaType.Userdata;
        this.ElementId = value;
        this.IsNil = value == null;
    }

    public LuaValue(Dictionary<LuaValue, LuaValue>? value)
    {
        this.LuaType = LuaType.Table;
        this.TableValue = value;
        this.IsNil = value == null;
    }

    public LuaValue(IEnumerable<LuaValue>? value)
    {
        this.LuaType = LuaType.Table;

        if (value != null)
        {
            this.TableValue = new Dictionary<LuaValue, LuaValue>();
            int i = 1;
            foreach (var arrayValue in value)
                this.TableValue[i++] = arrayValue;
        }

        this.IsNil = value == null;
    }

    public override bool Equals(object? obj)
    {
        if (obj is LuaValue luaValue)
            return 
                (luaValue.IntegerValue != null && luaValue.IntegerValue == this.IntegerValue) ||
                (luaValue.DoubleValue != null && luaValue.DoubleValue == this.DoubleValue) ||
                (luaValue.FloatValue != null && luaValue.FloatValue == this.FloatValue) ||
                (luaValue.BoolValue != null && luaValue.BoolValue == this.BoolValue) ||
                (luaValue.ElementId != null && luaValue.ElementId == this.ElementId) ||
                (luaValue.StringValue != null && luaValue.StringValue == this.StringValue) ||
                (luaValue.IsNil && this.IsNil) ||
                base.Equals(obj);

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return
            this.IntegerValue?.GetHashCode() ??
            this.DoubleValue?.GetHashCode() ??
            this.FloatValue?.GetHashCode() ??
            this.BoolValue?.GetHashCode() ??
            this.ElementId?.GetHashCode() ??
            this.StringValue?.GetHashCode() ??
            base.GetHashCode();
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

    public static implicit operator LuaValue(uint value) => new(value);
    public static implicit operator LuaValue(string value) => new(value);
    public static implicit operator LuaValue(bool value) => new(value);
    public static implicit operator LuaValue(int value) => new(value);
    public static implicit operator LuaValue(float value) => new(value);
    public static implicit operator LuaValue(double value) => new(value);
    public static implicit operator LuaValue(Dictionary<LuaValue, LuaValue> value) => new(value);
    public static implicit operator LuaValue(LuaValue[] value) => new(value);

    public static bool operator == (LuaValue left, LuaValue right) => left.Equals(right);
    public static bool operator != (LuaValue left, LuaValue right) => !left.Equals(right);

    public static implicit operator LuaValue(Vector2 vector) => new(new Dictionary<LuaValue, LuaValue>()
    {
        ["X"] = vector.X,
        ["Y"] = vector.Y,
    });
    public static implicit operator LuaValue(Vector3 vector) => new(new Dictionary<LuaValue, LuaValue>()
    {
        ["X"] = vector.X,
        ["Y"] = vector.Y,
        ["Z"] = vector.Z,
    });

    public static explicit operator uint(LuaValue value) => value.ElementId ?? 0;
    public static explicit operator string(LuaValue value) => value.StringValue ?? "";
    public static explicit operator bool(LuaValue value) => value.BoolValue ?? false;

    public static explicit operator int(LuaValue value) => value.IntegerValue ?? (int?)value.FloatValue ?? (int?)value.DoubleValue ?? 0;
    public static explicit operator float(LuaValue value) => value.FloatValue ?? (float?)value.DoubleValue ?? (float?)value.IntegerValue ?? 0;
    public static explicit operator double(LuaValue value) => value.DoubleValue ?? (double?)value.FloatValue ?? (double?)value.IntegerValue ?? 0;

    public static explicit operator Vector2(LuaValue value)
    {
        if (value.TableValue == null)
            return Vector2.Zero;

        var stringKeyedDictionary = value.TableValue.ToDictionary(x => x.Key.StringValue!, x => x.Value);
        return new((float)stringKeyedDictionary["X"], (float)stringKeyedDictionary["Y"]);
    }

    public static explicit operator Vector3(LuaValue value)
    {
        if (value.TableValue == null)
            return Vector3.Zero;

        var stringKeyedDictionary = value.TableValue.ToDictionary(x => x.Key.StringValue!, x => x.Value);
        return new ((float)stringKeyedDictionary["X"], (float)stringKeyedDictionary["Y"], (float)stringKeyedDictionary["Z"]);
    }
}
