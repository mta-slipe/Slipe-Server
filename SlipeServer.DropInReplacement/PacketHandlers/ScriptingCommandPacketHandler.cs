using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Enums;
using SlipeServer.Scripting;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.Services;
using System.Drawing;

namespace SlipeServer.DropInReplacement.PacketHandlers;

/// <summary>
/// Replaces both <see cref="SlipeServer.Server.PacketHandling.Handlers.Command.CommandPacketHandler"/>
/// and <see cref="SlipeServer.Server.Behaviour.DefaultChatBehaviour"/>.
/// Calls TriggerCommand (which synchronously fires onPlayerCommand, onPlayerChat, and all
/// addCommandHandler callbacks), then checks WasEventCancelled() before broadcasting chat.
/// Note: addCommandHandler callbacks run before onPlayerCommand scripting handlers fire
/// (C# event subscription order), so cancelEvent() inside onPlayerCommand cannot prevent
/// addCommandHandler execution.
/// </summary>
public class ScriptingCommandPacketHandler(
    IScriptEventRuntime eventRuntime,
    IChatBox chatBox,
    ILogger? logger = null
    ) : IPacketHandler<CommandPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_COMMAND;

    public void HandlePacket(IClient client, CommandPacket packet)
    {
        var player = client.Player;

        eventRuntime.CancelEvent(false);
        player.TriggerCommand(packet.Command, packet.Arguments);

        if (packet.Command == "say")
        {
            if (player.IsChatMuted)
            {
                chatBox.OutputTo(player, "You are muted.", Color.Red);
                return;
            }

            if (!eventRuntime.WasEventCancelled())
            {
                string message = $"{player.NametagColor.ToColorCode()}{player.Name}: #ffffff{string.Join(' ', packet.Arguments)}";
                chatBox.Output(message, Color.White, true, ChatEchoType.Player, player);
                logger?.LogInformation("{message}", message);
            }
        }
    }
}
