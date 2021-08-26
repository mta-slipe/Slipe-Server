﻿using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Vehicle
{
    public class SetVehicleTurretDirectionRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public Vector2 Direction { get; set; }

        public SetVehicleTurretDirectionRpcPacket(uint elementId, Vector2 direction)
        {
            this.ElementId = elementId;
            this.Direction = direction;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_VEHICLE_TURRET_POSITION);
            builder.WriteElementId(this.ElementId);
            builder.Write(this.Direction);
            return builder.Build();
        }
    }
}