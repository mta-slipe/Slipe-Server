using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;

namespace SlipeServer.Example;

public class ServerExampleLogic
{
    private readonly CommandService commandService;
    private readonly ChatBox chatBox;

    public ServerExampleLogic(CommandService commandService, ChatBox chatBox)
    {
        this.commandService = commandService;
        this.chatBox = chatBox;

        AddCommand("hello", player =>
        {
            this.chatBox.OutputTo(player, "Hello world");
        });

        AddCommand("toggleControls", player =>
        {
            var controls = player.Controls;
            controls.ToggleAll(false);
            controls.ForwardsEnabled = true;
            this.chatBox.OutputTo(player, "Toggle");
        });
    }

    private void AddCommand(string command, Action<Player> callback)
    {
        this.commandService.AddCommand(command).Triggered += (object? sender, Server.Events.CommandTriggeredEventArgs e) =>
        {
            callback(e.Player);
        };
    }
}
