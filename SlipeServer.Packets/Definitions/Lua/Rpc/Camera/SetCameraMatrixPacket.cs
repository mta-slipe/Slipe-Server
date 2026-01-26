using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System.Numerics;

namespace SlipeServer.Packets.Lua.Camera;

public sealed class SetCameraMatrixPacket(Vector3 position, Vector3 lookAt, float roll, float fov, byte timeContext) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public Vector3 Position { get; set; } = position;
    public Vector3 LookAt { get; set; } = lookAt;
    public float Roll { get; set; } = roll;
    public float Fov { get; set; } = fov;
    public byte TimeContext { get; set; } = timeContext;

    public override void Read(byte[] bytes)
    {
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_CAMERA_MATRIX);
        builder.Write(this.TimeContext);
        builder.Write(this.Position);
        builder.Write(this.LookAt);
        builder.Write(this.Roll);
        builder.Write(this.Fov);
        return builder.Build();
    }
}
