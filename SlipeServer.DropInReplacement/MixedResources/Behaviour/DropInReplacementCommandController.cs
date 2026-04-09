using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.DropInReplacement.MixedResources.Behaviour;

[CommandController]
public class DropInReplacementCommandController(IDropInReplacementResourceService resourceService, IResourceProvider resourceProvider) : BaseCommandController
{
    [Command("start")]
    public void StartResource(string name) => resourceService.StartResource(name);

    [Command("stop")]
    public void StopResource(string name) => resourceService.StopResource(name);

    [Command("restart")]
    public void RestartResource(string name) => resourceService.RestartResource(name);

    [Command("refresh")]
    public void RefreshResources() => resourceProvider.Refresh();
}
