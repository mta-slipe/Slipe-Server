using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Vehicles
{
    public class VehicleInOutPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_INOUT;

        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

        public override PacketPriority Priority => PacketPriority.High;

        public uint PedId { get; set; }
        public uint VehicleId { get; set; }
        public VehicleInOutAction ActionId { get; set; }
        public VehicleInOutActionReturns OutActionId { get; set; }
        public byte Seat { get; set; }
        public bool IsOnWater { get; set; }
        public byte Door { get; set; }
        public float DoorOpenRatio { get; set; }
        public bool StartedJacking { get; set; }
        public VehicleEnterFailReason FailReason { get; set; }
        public Vector3 CorrectPosition { get; set; }
        public uint PlayerInId { get; set; }
        public uint PlayerOutId { get; set; }


        public VehicleInOutPacket()
        {

        }

        //public VehicleInOutPacket()
        //{
        //}

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.PedId = reader.GetElementId();
            this.VehicleId = reader.GetElementId();
            this.ActionId = (VehicleInOutAction)reader.GetByteCapped(4);

            switch (this.ActionId)
            {
                case VehicleInOutAction.RequestIn:
                    this.Seat = reader.GetByteCapped(4);
                    this.IsOnWater = reader.GetBit();
                    this.Door = reader.GetByteCapped(3);
                    break;
                case VehicleInOutAction.NotifyJackAbort:
                    this.Door = reader.GetByteCapped(3);
                    this.ReadDoorOpenRatio(reader);
                    this.StartedJacking = reader.GetBit();
                    break;
                case VehicleInOutAction.NotifyAbortIn:
                    this.Door = reader.GetByteCapped(3);
                    this.ReadDoorOpenRatio(reader);
                    break;
                case VehicleInOutAction.RequestOut:
                    this.Door = reader.GetByteCapped(2);
                    break;
            }
        }

        private void ReadDoorOpenRatio(PacketReader reader)
        {
            var uncompressed = reader.GetBit();
            if (!uncompressed)
            {
                var notZero = reader.GetBit();
                this.DoorOpenRatio = notZero ? 1 : 0;
            } else
            {
                this.DoorOpenRatio = reader.GetFloatFromBits(10, 0.0f, 0.1f);
            }
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.PedId);
            builder.WriteElementId(this.VehicleId);
            builder.WriteCapped(this.Seat, 4);
            builder.WriteCapped((byte)this.OutActionId, 4);

            switch (this.OutActionId)
            {
                case VehicleInOutActionReturns.RequestInConfirmed:
                case VehicleInOutActionReturns.RequestJackConfirmed:
                    builder.WriteCapped(this.Door, 3);
                    break;
                case VehicleInOutActionReturns.NotifyJackReturn:
                    builder.WriteElementId(this.PlayerInId);
                    builder.WriteElementId(this.PlayerOutId);
                    break;
                case VehicleInOutActionReturns.VehicleAttemptFailed:
                    builder.Write((byte)this.FailReason);

                    if (this.FailReason == VehicleEnterFailReason.Distance)
                    {
                        builder.WriteVector3WithZAsFloat(this.CorrectPosition);
                    }
                    break;
                case VehicleInOutActionReturns.NotifyInAbortReturn:
                    builder.WriteCapped(this.Door, 3);
                    WriteDoorOpenRatio(builder);
                    break;
                case VehicleInOutActionReturns.RequestOutConfirmed:
                    builder.WriteCapped(this.Door, 2);
                    break;
            }
            return builder.Build();
        }

        private void WriteDoorOpenRatio(PacketBuilder builder)
        {
            if (this.DoorOpenRatio == 0 || this.DoorOpenRatio == 1)
            {
                builder.Write(false);
                builder.Write(this.DoorOpenRatio == 1);
            } else
            {
                builder.Write(true);
                builder.WriteFloatFromBits(this.DoorOpenRatio, 10, 0.0f, 1.0f, true);
            }
        }

        public override void Reset()
        {
            this.Seat = 0;
            this.IsOnWater = false;
            this.StartedJacking = false;
            this.DoorOpenRatio = 0;
        }
    }
}
