using System;
using MtaServer.Packets;
using MTAServerWrapper.Server;
using System.Net;

namespace MtaServer.Server.Elements
{
    public class Client: Element
    {
        public override ElementType ElementType => ElementType.Player;

        public string Name { get; set; }
        public IPAddress IPAddress { get; set; }

        public string Serial { get; private set; }
        public string Extra { get; private set; }
        public string Version { get; private set; }

        private readonly NetWrapper netWrapper;
        private readonly uint binaryAddress;

        internal Client(uint binaryAddress, NetWrapper netWrapper): base()
        {
            this.binaryAddress = binaryAddress;
            this.netWrapper = netWrapper;
        }

        public void SendPacket(Packet packet)
        {
            this.netWrapper.SendPacket(this.binaryAddress, packet);
        }

        public void HandleJoin()
        {
            OnJoin?.Invoke(this);
        }

        public void HandleCommand(string command, string[] arguments)
        {
            this.OnCommand?.Invoke(command, arguments);
        }

        public void SetVersion(ushort version)
        {
            this.netWrapper.SetVersion(this.binaryAddress, version);
        }

        public void FetchSerial()
        {
            Tuple<string, string, string> serialExtraAndVersion = netWrapper.GetClientSerialExtraAndVersion(binaryAddress);
            this.Serial = serialExtraAndVersion.Item1;
            this.Extra = serialExtraAndVersion.Item2;
            this.Version = serialExtraAndVersion.Item3;
        }

        public static event Action<Client> OnJoin;
        public event Action<string, string[]> OnCommand;
    }
}
