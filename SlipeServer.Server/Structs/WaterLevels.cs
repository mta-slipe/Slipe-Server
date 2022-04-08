namespace SlipeServer.Server.Structs;

public struct WaterLevels
{
    public float SeaLevel { get; init; } = 0;
    public float? OutsideSeaLevel { get; init; } = null;
    public float? NonSeaLevel { get; init; } = null;

    public WaterLevels()
    {

    }
}
