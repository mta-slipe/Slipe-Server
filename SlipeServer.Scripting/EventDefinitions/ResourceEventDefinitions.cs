using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;

namespace SlipeServer.Scripting.EventDefinitions;

public class ResourceEventDefinitions : IEventDefinitions
{
    private readonly ServerResourceService serverResourceService;

    public ResourceEventDefinitions(ServerResourceService serverResourceService)
    {
        this.serverResourceService = serverResourceService;
    }

    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        // https://wiki.multitheftauto.com/wiki/OnResourceStart
        eventRuntime.RegisterEvent<Element>(
            "onResourceStart",
            (element, callback) =>
            {
                void callbackProxy(Resource e)
                {
                    using var eventName = ServerResourceContext.Current!.PushVariable("eventName", "onResourceStart");
                    callback.CallbackDelegate(e.Root, e);
                }
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => this.serverResourceService.Started += callbackProxy,
                    Remove = (element) => this.serverResourceService.Stopped -= callbackProxy
                };
            }
        );
    }
}
