using MtaServer.Net;
using MtaServer.Packets;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MtaServer.Server
{
    public class Client
    {
        private readonly NetWrapper netWrapper;
        private readonly uint binaryAddress;

        public Player Player { get; }

        public string? Serial { get; private set; }
        public string? Extra { get; private set; }
        public string? Version { get; private set; }
        public IPAddress? IPAddress { get; set; }
        public bool IsConnected { get; internal set; }

        public Client(uint binaryAddress, NetWrapper netWrapper)
        {
            this.binaryAddress = binaryAddress;
            this.netWrapper = netWrapper;
            this.Player = new Player(this);
            this.IsConnected = true;
        }

        public void SendPacket(Packet packet)
        {
            if (this.IsConnected && (ClientPacketScope.Current == null || ClientPacketScope.Current.ContainsClient(this)))
            {
                this.netWrapper.SendPacket(this.binaryAddress, packet);
            }
        }

        public void SendPacket(PacketId packetId, byte[] data)
        {
            if (this.IsConnected && (ClientPacketScope.Current == null || ClientPacketScope.Current.ContainsClient(this)))
            {
                this.netWrapper.SendPacket(this.binaryAddress, packetId, data);
            }
        }

        public void SetVersion(ushort version)
        {
            if(this.IsConnected)
            {
                this.netWrapper.SetVersion(this.binaryAddress, version);
            }
        }

        public void FetchSerial()
        {
            Tuple<string, string, string> serialExtraAndVersion = netWrapper.GetClientSerialExtraAndVersion(binaryAddress);
            this.Serial = serialExtraAndVersion.Item1;
            this.Extra = serialExtraAndVersion.Item2;
            this.Version = serialExtraAndVersion.Item3;
        }
    }
}
