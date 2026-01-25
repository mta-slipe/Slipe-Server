using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Services;
using System.Drawing;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for sending chat echo packets to mimic default MTA main chat
/// </summary>
public class DefaultChatBehaviour
{
    public DefaultChatBehaviour(MtaServer server, IChatBox chatBox, ILogger? logger)
    {
        server.PlayerJoined += (player) =>
        {
            player.CommandEntered += (sender, arguments) =>
            {
                if (arguments.Command == "say")
                {
                    if (sender.IsChatMuted)
                    {
                        chatBox.OutputTo(sender, "You are muted.", Color.Red);
                        return;
                    }

                    string message = $"{player.NametagColor.ToColorCode()}{player.Name}: #ffffff{string.Join(' ', arguments.Arguments)}";
                    chatBox.Output(message, Color.White, true, ChatEchoType.Player, player);
                    logger?.LogInformation("{message}", message);
                }
            };
        };
    }
}
