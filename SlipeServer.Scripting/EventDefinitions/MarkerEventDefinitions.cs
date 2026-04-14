using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Scripting.EventDefinitions;

public class MarkerEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Marker>(
            "onMarkerHit",
            (callback) =>
            {
                void callbackProxy(Marker sender, MarkerHitEventArgs e)
                    => callback.CallbackDelegate(sender, e.HitElement, e.MatchingDimension);

                return new EventHandlerActions<Marker>()
                {
                    Add = (element) => element.ElementHit += callbackProxy,
                    Remove = (element) => element.ElementHit -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Marker>(
            "onMarkerLeave",
            (callback) =>
            {
                void callbackProxy(Marker sender, MarkerLeftEventArgs e)
                    => callback.CallbackDelegate(sender, e.LeftElement, e.MatchingDimension);

                return new EventHandlerActions<Marker>()
                {
                    Add = (element) => element.ElementLeft += callbackProxy,
                    Remove = (element) => element.ElementLeft -= callbackProxy
                };
            }
        );
    }
}
