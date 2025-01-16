using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetHeatHazePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte Intensity { get; set; }
    public byte RandomShift { get; set; }
    public ushort SpeedMin { get; set; }
    public ushort SpeedMax { get; set; }
    public short ScanSizeX { get; set; }
    public short ScanSizeY { get; set; }
    public short RenderSizeX { get; set; }
    public short RenderSizeY { get; set; }
    public bool InsideBuilding { get; set; }

    public SetHeatHazePacket(byte intensity, byte randomShift = 0, ushort seedMin = 12, ushort speedMax = 18, short scanSizeX = 75, short scanSizeY = 80, short renderSizeX = 80, short renderSizeY = 85, bool insideBuilding = false)
    {
        this.Intensity = intensity;
        this.RandomShift = randomShift;
        this.SpeedMin = seedMin;
        this.SpeedMax = speedMax;
        this.ScanSizeX = scanSizeX;
        this.ScanSizeY = scanSizeY;
        this.RenderSizeX = renderSizeX;
        this.RenderSizeY = renderSizeY;
        this.InsideBuilding = insideBuilding;

    }
    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_HEAT_HAZE);
        builder.Write(this.Intensity);
        builder.Write(this.RandomShift);
        builder.WriteRange(this.SpeedMin, 10, 0, 1000);
        builder.WriteRange(this.SpeedMax, 10, 0, 1000);
        builder.WriteRange(this.ScanSizeX, 11, -1000, 1000);
        builder.WriteRange(this.ScanSizeY, 11, -1000, 1000);
        builder.WriteRange(this.RenderSizeX, 10, 0, 1000);
        builder.WriteRange(this.RenderSizeY, 10, 0, 1000);
        builder.Write(this.InsideBuilding);

        return builder.Build();
    }
}
