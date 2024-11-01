using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;

namespace SlipeServer.Example.Logic;

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
    }

    private void AddCommand(string command, Action<Player> callback)
    {
        this.commandService.AddCommand(command).Triggered += (sender, e) =>
        {
            callback(e.Player);
        };
    }
}
