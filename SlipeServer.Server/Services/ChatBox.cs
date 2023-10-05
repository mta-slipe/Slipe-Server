using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System.Drawing;

namespace SlipeServer.Server.Services;

/// <summary>
/// Represents the ingame chat, allows you to send messages to (individual) players
/// </summary>
public class ChatBox
{
    private readonly MtaServer server;
    private readonly RootElement root;

    public ChatBox(MtaServer server, RootElement root)
    {
        this.server = server;
        this.root = root;
    }

    public void Output(string message, Color? color = null, bool isColorCoded = false, ChatEchoType type = ChatEchoType.Player, Element? source = null)
    {
        this.server.BroadcastPacket(new ChatEchoPacket(source?.Id ?? this.root.Id, message, color ?? Color.White, type, isColorCoded));
    }

    public void Clear()
    {
        this.server.BroadcastPacket(ClearChatPacket.Instance);
    }

    public void OutputTo(Player player, string message, Color? color = null, bool isColorCoded = false, ChatEchoType type = ChatEchoType.Player, Element? source = null)
    {
        player.Client.SendPacket(new ChatEchoPacket(source?.Id ?? this.root.Id, message, color ?? Color.White, type, isColorCoded));
    }

    public void ClearFor(Player player)
    {
        player.Client.SendPacket(ClearChatPacket.Instance);
    }

    public void SetVisible(bool visible, bool? inputBlocked = null)
    {
        if(inputBlocked == null)
            inputBlocked = !visible;
        this.server.BroadcastPacket(new ShowChatPacket(visible, inputBlocked.Value));
    }

    public void SetVisibleFor(Player player, bool visible, bool? inputBlocked = null)
    {
        if (inputBlocked == null)
            inputBlocked = !visible;
        player.Client.SendPacket(new ShowChatPacket(visible, inputBlocked.Value));
    }
}
