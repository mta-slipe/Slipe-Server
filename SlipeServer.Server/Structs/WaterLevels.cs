namespace SlipeServer.Server.Structs
{
    public struct WaterLevels
    {
        public float SeaLevel { get; init; }
        public float? OutsideSeaLevel { get; init; }
        public float? NonSeaLevel { get; init; }
    }
}
