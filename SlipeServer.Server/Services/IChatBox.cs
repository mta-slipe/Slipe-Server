using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System.Drawing;

namespace SlipeServer.Server.Services;

public interface IChatBox
{
    void Clear();
    void ClearFor(Player player);
    void Output(string message, Color? color = null, bool isColorCoded = false, ChatEchoType type = ChatEchoType.Player, Element? source = null);
    void OutputTo(Player player, string message, Color? color = null, bool isColorCoded = false, ChatEchoType type = ChatEchoType.Player, Element? source = null);
    void SetVisible(bool visible, bool? inputBlocked = null);
    void SetVisibleFor(Player player, bool visible, bool? inputBlocked = null);
}