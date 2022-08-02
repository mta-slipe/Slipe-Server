using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;

namespace SlipeServer.Scripting.EventDefinitions;

public class ElementEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Element>(
            "onElementDestroyed",
            (element, callback) =>
            {
                void callbackProxy(Element e, ElementDestroyedEventArgs _) => callback.CallbackDelegate(e);
                return new EventHandlerActions<Element>()
                {
                    Add = (element) => element.Destroyed += callbackProxy,
                    Remove = (element) => element.Destroyed -= callbackProxy
                };
            }
        );
    }
}
