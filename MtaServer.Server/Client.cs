using MtaServer.Packets;
using MtaServer.Server.Elements;
using MTAServerWrapper.Server;
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

        private byte[] TimeContext { get; set; } = new byte[] {
            0, // setCameraMatrix, index = 0
        };

        public Client(uint binaryAddress, NetWrapper netWrapper)
        {
            this.binaryAddress = binaryAddress;
            this.netWrapper = netWrapper;
            this.Player = new Player(this);
        }

        public void SendPacket(Packet packet) => this.netWrapper.SendPacket(this.binaryAddress, packet);
        public void SetVersion(ushort version) => this.netWrapper.SetVersion(this.binaryAddress, version);

        public byte GenerateSyncTimeContext(uint index)
        {
            TimeContext[index] = (byte)Math.Max(1, TimeContext[index] + 1);
            return TimeContext[index];
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
