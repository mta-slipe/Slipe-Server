using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;

namespace SlipeServer.Scripting.EventDefinitions
{
    public class PlayerEventDefinitions : IEventDefinitions
    {
        public void LoadInto(IScriptEventRuntime eventRuntime)
        {
            eventRuntime.RegisterEvent<Player>(
                "onPlayerWasted",
                (element, callback) =>
                {
                    void callbackProxy(Element sender, PlayerWastedEventArgs e) => callback.CallbackDelegate(e.Source);
                    return new EventHandlerActions<Player>()
                    {
                        Add = (element) => element.Wasted += callbackProxy,
                        Remove = (element) => element.Wasted -= callbackProxy
                    };
                }
            );
        }
    }
}
