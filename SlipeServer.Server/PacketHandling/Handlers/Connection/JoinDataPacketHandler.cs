using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using System.Runtime.InteropServices;

namespace SlipeServer.Server.PacketHandling.Handlers.Connection
{
    public class JoinDataPacketHandler : IPacketHandler<PlayerJoinDataPacket>
    {
        public PacketId PacketId => PacketId.PACKET_ID_PLAYER_JOINDATA;

        public void HandlePacket(Client client, PlayerJoinDataPacket packet)
        {
            string osName =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Mac OS" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? "Free BSD" :
                "Unknown";
            client.SendPacket(new JoinCompletePacket($"Slipe Server 0.1.0 [{osName}]\0", "1.5.7-9.0.0"));

            client.Player.RunAsSync(() =>
            {
                client.Player.Name = packet.Nickname;
            });
            client.SetVersion(packet.BitStreamVersion);
            client.FetchSerial();
        }
    }
}
