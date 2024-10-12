using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Scripting.EventDefinitions;

public class CollisionShapeEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent(
            "onColShapeHit",
            (callback) =>
            {
                void callbackProxy(CollisionShape source, CollisionShapeHitEventArgs args) => callback.CallbackDelegate(source, args.Element, args.Element.Dimension == source.Dimension);
                return new EventHandlerActions<CollisionShape>()
                {
                    Add = (element) => element.ElementEntered += callbackProxy,
                    Remove = (element) => element.ElementEntered -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent(
            "onColShapeLeave",
            (callback) =>
            {
                void callbackProxy(CollisionShape source, CollisionShapeLeftEventArgs args) => callback.CallbackDelegate(source, args.Element, args.Element.Dimension == source.Dimension);
                return new EventHandlerActions<CollisionShape>()
                {
                    Add = (element) => element.ElementLeft += callbackProxy,
                    Remove = (element) => element.ElementLeft -= callbackProxy
                };
            }
        );
    }
}
