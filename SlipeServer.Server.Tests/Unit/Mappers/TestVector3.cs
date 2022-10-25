namespace SlipeServer.Server.Tests.Unit.Mappers;

internal struct TestVector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public TestVector3(float x, float y, float z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
}
