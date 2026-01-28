using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System.Drawing;

namespace SlipeServer.Server.Services;

/// <summary>
/// Represents the ingame chat, allows you to send messages to (individual) players
/// </summary>
public class ChatBox(IMtaServer server, IRootElement root) : IChatBox
{
    public void Output(string message, Color? color = null, bool isColorCoded = false, ChatEchoType type = ChatEchoType.Player, Element? source = null)
    {
        server.BroadcastPacket(new ChatEchoPacket(source?.Id ?? root.Id, message, color ?? Color.White, type, isColorCoded));
    }

    public void Clear()
    {
        server.BroadcastPacket(ClearChatPacket.Instance);
    }

    public void OutputTo(Player player, string message, Color? color = null, bool isColorCoded = false, ChatEchoType type = ChatEchoType.Player, Element? source = null)
    {
        player.Client.SendPacket(new ChatEchoPacket(source?.Id ?? root.Id, message, color ?? Color.White, type, isColorCoded));
    }

    public void ClearFor(Player player)
    {
        player.Client.SendPacket(ClearChatPacket.Instance);
    }

    public void SetVisible(bool visible, bool? inputBlocked = null)
    {
        if (inputBlocked == null)
            inputBlocked = !visible;
        server.BroadcastPacket(new ShowChatPacket(visible, inputBlocked.Value));
    }

    public void SetVisibleFor(Player player, bool visible, bool? inputBlocked = null)
    {
        if (inputBlocked == null)
            inputBlocked = !visible;
        player.Client.SendPacket(new ShowChatPacket(visible, inputBlocked.Value));
    }
}
