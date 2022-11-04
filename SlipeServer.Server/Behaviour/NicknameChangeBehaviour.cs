using System.Drawing;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Services;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for allow players to change their own nickname using /nick
/// </summary>
public class NicknameChangeBehaviour
{
    public NicknameChangeBehaviour(MtaServer server, ChatBox chatBox, ILogger? logger)
    {
        server.PlayerJoined += (player) =>
        {
            player.CommandEntered += (sender, arguments) =>
            {
                if (arguments.Command == "nick")
                {
                    if (arguments.Arguments.Length == 0)
                    {
                        chatBox.OutputTo(player, "Syntax: /nick [name]", Color.Red);
                        return;
                    }
                    string newName = arguments.Arguments[0];
                    string message = $"{player.Name} has been renamed to {newName}";
                    player.Name = newName;
                    chatBox.Output(message, Color.LightYellow, true, ChatEchoType.Internal, player);
                    logger?.LogInformation(message);
                }
            };
        };
    }
}
