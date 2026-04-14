using SlipeServer.Server.Elements;

namespace SlipeServer.Scripting.EventDefinitions;

public class WeaponObjectEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<WeaponObject>(
            "onWeaponFire",
            (callback) =>
            {
                void callbackProxy(WeaponObject sender)
                    => callback.CallbackDelegate(sender);

                return new EventHandlerActions<WeaponObject>()
                {
                    Add = (element) => element.Fired += callbackProxy,
                    Remove = (element) => element.Fired -= callbackProxy
                };
            }
        );
    }
}
