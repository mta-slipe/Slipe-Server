using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Scripting.EventDefinitions
{
    public class ElementEventDefinitions: IEventDefinitions
    {
        public void LoadInto(IScriptEventRuntime eventRuntime)
        {
            eventRuntime.RegisterEvent<Element>(
                "onElementDestroyed",
                (element, callback) =>
                {
                    Action<Element> callbackProxy = (Element e) => callback(e);
                    return new EventHandlerActions<Element>()
                    {
                        Add = (element) => element.Destroyed += callbackProxy,
                        Remove = (element) => element.Destroyed -= callbackProxy
                    };
                }
            );
        }
    }
}
