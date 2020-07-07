using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetHeatHazePacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public byte Intensity { get; set; }
        public byte RandomShift { get; set; }
        public ushort SpeedMin { get; set; }
        public ushort SpeedMax { get; set; }
        public short ScanSizeX { get; set; }
        public short ScanSizeY { get; set; }
        public byte RenderSizeX { get; set; }
        public byte RenderSizeY { get; set; }
        public bool InsideBuilding { get; set; }

        public SetHeatHazePacket(byte intensity,byte randomShift = 0,ushort seedMin = 12,ushort speedMax = 18, short scanSizeX = 75,short scanSizeY = 80,byte renderSizeX = 80,byte renderSizeY = 85,bool insideBuilding = false)
        {
            Intensity = intensity;
            RandomShift = randomShift;
            SpeedMin = seedMin;
            SpeedMax = speedMax;
            ScanSizeX = scanSizeX;
            ScanSizeY = scanSizeY;
            RenderSizeX = renderSizeX;
            RenderSizeY = renderSizeY;
            InsideBuilding = insideBuilding;

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
            builder.Write(this.SpeedMin);
            builder.Write(this.SpeedMax);
            builder.Write(this.ScanSizeX);
            builder.Write(this.ScanSizeY);
            builder.Write(this.RenderSizeX);
            builder.Write(this.RenderSizeY);
            builder.Write(this.InsideBuilding);

            return builder.Build();
        }
    }
}
