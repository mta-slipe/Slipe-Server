using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Player
{
    public class TakePlayerScreenshotPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public byte TimeContext { get; set; }
        public ushort SizeX { get; set; }
        public ushort SizeY { get; set; }
        public string Tag { get; set; }
        public byte Quality { get; set; }
        public uint MaxBandwith { get; set; }
        public ushort MaxPacketSize { get; set; }
        public ushort ResourceId { get; set; }
        public string ResourceName { get; set; } = "unknown";

        public TakePlayerScreenshotPacket(byte timeContext, ushort sizeX, ushort sizeY, string tag, byte quality, uint maxBandwith, ushort maxPacketSize, ushort resourceId)
        {
            this.TimeContext = timeContext;
            this.SizeX = sizeX;
            this.SizeY = sizeY;
            this.Tag = tag;
            this.Quality = quality;
            this.MaxBandwith = maxBandwith;
            this.MaxPacketSize = maxPacketSize;
            this.ResourceId = resourceId;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write((byte)ElementRpcFunction.TAKE_PLAYER_SCREEN_SHOT);

            builder.Write(this.SizeX);
            builder.Write(this.SizeY);
            builder.Write(this.Tag);
            builder.Write(this.Quality);
            builder.Write(this.MaxBandwith);
            builder.Write(this.MaxPacketSize);
            builder.Write(this.ResourceId);
            builder.Write(this.ResourceName);
            builder.Write((uint)0);

            builder.Write(this.TimeContext);

            return builder.Build();
        }
    }
}
