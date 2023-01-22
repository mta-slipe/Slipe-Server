namespace SlipeServer.Server.ElementConcepts;

/// <summary>
/// Represent a vehicle variant, this consists of two bytes, combining separate features.
/// Vehicle variants are actual changes within the 3d model itself.
/// </summary>
public readonly struct VehicleVariants
{
    public byte Variant1 { get; init; }
    public byte Variant2 { get; init; }
}
