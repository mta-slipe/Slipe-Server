using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structures;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Sync
{
    public class CameraSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_CAMERA_SYNC;
        public override PacketReliability Reliability => PacketReliability.UnreliableSequenced;
        public override PacketPriority Priority => PacketPriority.Medium;

        public byte TimeContext { get; private set; } 
        public bool IsFixed { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 LookAt { get; private set; }
        public uint TargetId { get; private set; }

        public CameraSyncPacket()
        {

        }

        public CameraSyncPacket(byte timeContext, bool isFixed, Vector3 position, Vector3 lookAt, uint targetId)
        {
            this.TimeContext = timeContext;
            this.IsFixed = isFixed;
            this.Position = position;
            this.LookAt = lookAt;
            this.TargetId = targetId;
        }

        public CameraSyncPacket(byte timeContext, Vector3 position, Vector3 lookAt)
            : this(timeContext, true, position, lookAt, 0) { }

        public CameraSyncPacket(byte timeContext, uint targetId)
            : this(timeContext, false, Vector3.Zero, Vector3.Zero, targetId) { }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.TimeContext = reader.GetByte();
            this.IsFixed = reader.GetBit();

            if (this.IsFixed)
            {
                if (reader.Size - reader.Counter > 3 * 24)
                    this.Position = reader.GetVector3WithZAsFloat(14, 10);
                if (reader.Size - reader.Counter > 3 * 24)
                    this.LookAt = reader.GetVector3WithZAsFloat(14, 10);
                this.TargetId = 0;
            } else
            {
                this.TargetId = reader.GetElementId();
            }
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write(this.TimeContext);
            builder.Write(this.IsFixed);
            
            if (this.IsFixed)
            {
                builder.WriteVector3WithZAsFloat(this.Position, 14, 10);
                builder.WriteVector3WithZAsFloat(this.LookAt, 14, 10);
            } else
            {
                builder.WriteElementId(this.TargetId);
            }

            return builder.Build();
        }
    }
}
