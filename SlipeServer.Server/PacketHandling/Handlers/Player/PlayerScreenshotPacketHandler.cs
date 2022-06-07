using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System.IO;

namespace SlipeServer.Server.PacketHandling.Handlers.Player;

public class PlayerScreenshotPacketHandler : IPacketHandler<PlayerScreenshotPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_SCREENSHOT;

    public void HandlePacket(IClient client, PlayerScreenshotPacket packet)
    {
        if (packet.PartNumber == 0)
        {
            if (packet.Status == ScreenshotStatus.Success)
            {
                if (!(client.Player.PendingScreenshots.ContainsKey(packet.ScreenshotId)))
                {
                    client.Player.PendingScreenshots[packet.ScreenshotId] = new PlayerPendingScreenshot
                    {
                        Stream = new MemoryStream((int)packet.TotalBytes),
                        TotalParts = packet.TotalParts,
                        Tag = packet.Tag,
                    };
                }
            } else
            {

                client.Player.PendingScreenshots[packet.ScreenshotId] = new PlayerPendingScreenshot
                {
                    ErrorMessage = packet.Error,
                    Tag = packet.Tag,
                };
                client.Player.ScreenshotEnd(packet.ScreenshotId);
                return;
            }
        }
        var pendingScreenshot = client.Player.PendingScreenshots[packet.ScreenshotId];
        if (pendingScreenshot.Stream != null)
        {
            pendingScreenshot.Stream.Write(packet.Buffer);
            if (pendingScreenshot.TotalParts == packet.PartNumber + 1)
            {
                pendingScreenshot.Stream.Seek(0, SeekOrigin.Begin);
                client.Player.ScreenshotEnd(packet.ScreenshotId);
            }
        }
    }
}
