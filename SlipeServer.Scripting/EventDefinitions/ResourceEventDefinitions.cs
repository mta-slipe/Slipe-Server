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
        eventRuntime.RegisterEvent<DummyElement>(
            "onResourceStart",
            (element, callback) =>
            {
                void callbackProxy(Resource e) => callback.CallbackDelegate(e.Root, e);
                return new EventHandlerActions<DummyElement>()
                {
                    Add = (element) => serverResourceService.Started += callbackProxy,
                    Remove = (element) => serverResourceService.Stopped -= callbackProxy
                };
            }
        );
    }
}
