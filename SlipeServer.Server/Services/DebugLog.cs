using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Factories;
using System.Drawing;

namespace SlipeServer.Server.Services;

/// <summary>
/// Represents the ingame debug dialog (/debugscript 3)
/// </summary>
public class DebugLog(MtaServer server)
{
    public void Output(string message, DebugLevel level = DebugLevel.Information, Color? color = null)
    {
        server.BroadcastPacket(PlayerPacketFactory.CreateDebugEchoPacket(message, level, color ?? Color.White));
    }

    public void OutputTo(Player player, string message, DebugLevel level = DebugLevel.Information, Color? color = null)
    {
        player.Client.SendPacket(PlayerPacketFactory.CreateDebugEchoPacket(message, level, color ?? Color.White));
    }

    public void SetVisible(bool isVisible)
    {
        server.BroadcastPacket(PlayerPacketFactory.CreateToggleDebuggerPacket(isVisible));
    }

    public void SetVisibleTo(Player player, bool isVisible)
    {
        player.Client.SendPacket(PlayerPacketFactory.CreateToggleDebuggerPacket(isVisible));
    }
}
