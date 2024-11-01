using SlipeServer.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Services;
using System.Text;

namespace SlipeServer.Example.Logic;

public sealed class ResourcesExampleLogic
{
    private readonly ChatBox chatBox;
    private readonly CommandService commandService;
    private readonly LuaEventService luaEventService;
    private readonly Resource? testResource;
    private readonly Resource? secondTestResource;
    private readonly Resource? thirdTestResource;
    private readonly RootElement rootElement;

    public ResourcesExampleLogic(MtaServer mtaServer, IResourceProvider resourceProvider, ChatBox chatBox, CommandService commandService, LuaEventService luaEventService)
    {
        this.chatBox = chatBox;
        this.commandService = commandService;
        this.luaEventService = luaEventService;
        this.rootElement = mtaServer.RootElement;
        this.testResource = resourceProvider.GetResource("TestResource");
        this.secondTestResource = resourceProvider.GetResource("SecondTestResource");
        this.secondTestResource.NoClientScripts[$"{secondTestResource!.Name}/testfile.lua"] =
            Encoding.UTF8.GetBytes("outputChatBox(\"I AM A NOT CACHED MESSAGE\")");
        this.secondTestResource.NoClientScripts[$"blabla.lua"] = new byte[] { };

        this.thirdTestResource = resourceProvider.GetResource("MetaXmlTestResource");

        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void AddCommands()
    {
        this.commandService.AddCommand("latent").Triggered += (source, args) =>
        {
            this.luaEventService.TriggerLatentEvent("Slipe.Test.ClientEvent", this.testResource!, this.rootElement, 1, this.rootElement, 50, "STRING");
        };
    }
    private void HandlePlayerJoined(Player player)
    {
        this.testResource?.StartFor(player);
        this.secondTestResource?.StartFor(player);
        this.thirdTestResource?.StartFor(player);
        this.chatBox.OutputTo(player, "Resources started");
    }
}
