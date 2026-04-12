using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;

namespace SlipeServer.Scripting.EventDefinitions;

public class PickupEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Pickup>(
            "onPickupHit",
            (callback) =>
            {
                void callbackProxy(Pickup sender, PickupHitEventArgs e)
                    => callback.CallbackDelegate(sender, e.Player, sender.Dimension == e.Player.Dimension);
                return new EventHandlerActions<Pickup>()
                {
                    Add = (pickup) => pickup.Hit += callbackProxy,
                    Remove = (pickup) => pickup.Hit -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Pickup>(
            "onPickupLeave",
            (callback) =>
            {
                void callbackProxy(Pickup sender, PickupLeftEventArgs e)
                    => callback.CallbackDelegate(sender, e.Player, sender.Dimension == e.Player.Dimension);
                return new EventHandlerActions<Pickup>()
                {
                    Add = (pickup) => pickup.Left += callbackProxy,
                    Remove = (pickup) => pickup.Left -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Pickup>(
            "onPickupSpawn",
            (callback) =>
            {
                void callbackProxy(Pickup sender, EventArgs e)
                    => callback.CallbackDelegate(sender);
                return new EventHandlerActions<Pickup>()
                {
                    Add = (pickup) => pickup.Reset += callbackProxy,
                    Remove = (pickup) => pickup.Reset -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Pickup>(
            "onPickupUse",
            (callback) =>
            {
                void callbackProxy(Pickup sender, CancellablePickupUseEventArgs e)
                {
                    callback.CallbackDelegate(sender, e.Player);
                    if (eventRuntime.WasEventCancelled())
                        e.Cancel = true;
                }
                return new EventHandlerActions<Pickup>()
                {
                    Add = (pickup) => pickup.BeforeUsed += callbackProxy,
                    Remove = (pickup) => pickup.BeforeUsed -= callbackProxy
                };
            }
        );
    }
}
