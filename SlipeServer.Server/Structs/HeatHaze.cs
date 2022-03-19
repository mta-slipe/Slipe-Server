using System.Numerics;

namespace SlipeServer.Server.Structs
{
    public struct HeatHaze
    {
        public byte Intensity { get; init; }
        public byte RandomShift { get; init; } = 0;
        public ushort MinSpeed { get; init; } = 12;
        public ushort MaxSpeed { get; init; } = 18;
        public Vector2 ScanSize { get; init; } = new Vector2(75, 80);
        public Vector2 RenderSize { get; init; } = new Vector2(80, 85);
        public bool IsEnabledInsideBuildings { get; init; } = false;
    }
}
