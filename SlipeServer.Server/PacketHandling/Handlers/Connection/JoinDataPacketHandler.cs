using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Bans;
using SlipeServer.Server.Clients;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace SlipeServer.Server.PacketHandling.Handlers.Connection;

public class JoinDataPacketHandler(IMtaServer mtaServer, IBanRepository banRepository) : IPacketHandler<PlayerJoinDataPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_JOINDATA;

    public void HandlePacket(IClient client, PlayerJoinDataPacket packet)
    {
        if (mtaServer.Password != null)
        {
            var hash = MD5.HashData(Encoding.ASCII.GetBytes(mtaServer.Password));
            if (!hash.SequenceEqual(packet.Password))
            {
                client.SendPacket(new PlayerDisconnectPacket(PlayerDisconnectType.INVALID_PASSWORD));
                return;
            }
        }

        client.FetchSerial();
        client.FetchIp();
        if (banRepository.IsIpOrSerialBanned(client.Serial, client.IPAddress, out var ban))
        {
            var reason = ban?.Serial != null ? PlayerDisconnectType.BANNED_SERIAL : PlayerDisconnectType.BANNED_IP;
            client.SendPacket(new PlayerDisconnectPacket(reason, $"{ban?.Reason ?? "Unknown"}"));
            return;
        }

        client.SetVersion(packet.BitStreamVersion);

        string osName =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Mac OS" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? "Free BSD" :
            "Unknown";
        client.SendPacket(new JoinCompletePacket($"Slipe Server 0.1.0 [{osName} {RuntimeInformation.ProcessArchitecture}]", "1.6.0-9.0.0"));

        client.Player.RunAsSync(() =>
        {
            client.Player.Name = packet.Nickname;
        });
    }
}
