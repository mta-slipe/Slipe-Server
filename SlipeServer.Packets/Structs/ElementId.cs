using System;

namespace SlipeServer.Packets.Structs;

/// <summary>
/// Represents an element ID
/// </summary>
public readonly struct ElementId
{
    public uint Value { get; init; }

    public override bool Equals(object? obj) => obj is ElementId id && this.Value == id.Value;
    public override int GetHashCode() => HashCode.Combine(this.Value);

    public static bool operator ==(ElementId left, ElementId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ElementId left, ElementId right)
    {
        return !(left == right);
    }

    public static explicit operator ElementId(uint value) => new() { Value = value };
    public static explicit operator uint(ElementId id) => id.Value;

    public static ElementId Zero { get; } = ElementId.Zero;

    public override string ToString() => this.Value.ToString();
}
