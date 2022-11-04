using SlipeServer.Server.Elements;

namespace SlipeServer.Scripting.EventDefinitions;

public class ElementEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Element>(
            "onElementDestroyed",
            (element, callback) =>
            {
                void callbackProxy(Element e) => callback.CallbackDelegate(e);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.Destroyed += callbackProxy,
                    Remove = (element) => element.Destroyed -= callbackProxy
                };
            }
        );
    }
}
