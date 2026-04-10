using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;

namespace SlipeServer.Scripting.EventDefinitions;

public class VehicleEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Vehicle>(
            "onVehicleEnter",
            (callback) =>
            {
                void callbackProxy(Element sender, VehicleEnteredEventsArgs e)
                    => callback.CallbackDelegate(sender, e.Ped, e.Seat, null);

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.PedEntered += callbackProxy,
                    Remove = (element) => element.PedEntered -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Vehicle>(
            "onVehicleExit",
            (callback) =>
            {
                void callbackProxy(Element sender, VehicleLeftEventArgs e)
                    => callback.CallbackDelegate(sender, e.Ped, e.Seat, null, e.WarpsOut);

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.PedLeft += callbackProxy,
                    Remove = (element) => element.PedLeft -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Vehicle>(
            "onVehicleStartEnter",
            (callback) =>
            {
                void callbackProxy(Element sender, VehicleStartEnterEventArgs e)
                    => callback.CallbackDelegate(sender, e.EnteringPed, e.Seat, e.Jacked, e.Door);

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.PedStartedEntering += callbackProxy,
                    Remove = (element) => element.PedStartedEntering -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Vehicle>(
            "onVehicleStartExit",
            (callback) =>
            {
                void callbackProxy(Element sender, VehicleStartExitEventArgs e)
                    => callback.CallbackDelegate(sender, e.ExitingPed, e.Seat, e.Jacker, e.Door);

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.PedStartedExiting += callbackProxy,
                    Remove = (element) => element.PedStartedExiting -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Vehicle>(
            "onVehicleExplode",
            (callback) =>
            {
                void callbackProxy(Element sender, VehicleBlownEventArgs e)
                    => callback.CallbackDelegate(sender);

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.Blown += callbackProxy,
                    Remove = (element) => element.Blown -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Vehicle>(
            "onVehicleRespawn",
            (callback) =>
            {
                void callbackProxy(Element sender, VehicleRespawnEventArgs e)
                    => callback.CallbackDelegate(sender, e.Exploded);

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.Respawned += callbackProxy,
                    Remove = (element) => element.Respawned -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Vehicle>(
            "onVehicleDamage",
            (callback) =>
            {
                void callbackProxy(Vehicle sender, ElementChangedEventArgs<Vehicle, float> e)
                {
                    if (e.NewValue < e.OldValue)
                        callback.CallbackDelegate(sender, e.OldValue - e.NewValue);
                }

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.HealthChanged += callbackProxy,
                    Remove = (element) => element.HealthChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Vehicle>(
            "onTrailerAttach",
            (callback) =>
            {
                void callbackProxy(Vehicle sender, ElementChangedEventArgs<Vehicle, Vehicle?> e)
                {
                    if (e.NewValue != null)
                        callback.CallbackDelegate(sender, e.NewValue);
                }

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.TowingVehicleChanged += callbackProxy,
                    Remove = (element) => element.TowingVehicleChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Vehicle>(
            "onTrailerDetach",
            (callback) =>
            {
                void callbackProxy(Vehicle sender, ElementChangedEventArgs<Vehicle, Vehicle?> e)
                {
                    if (e.NewValue == null && e.OldValue != null)
                        callback.CallbackDelegate(sender, e.OldValue);
                }

                return new EventHandlerActions<Vehicle>()
                {
                    Add = (element) => element.TowingVehicleChanged += callbackProxy,
                    Remove = (element) => element.TowingVehicleChanged -= callbackProxy
                };
            }
        );
    }
}
