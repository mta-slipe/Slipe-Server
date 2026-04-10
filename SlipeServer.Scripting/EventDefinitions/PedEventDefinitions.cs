using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Scripting.EventDefinitions;

public class PedEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Ped>(
            "onPedDamage",
            (callback) =>
            {
                void callbackProxy(Ped sender, PedDamagedEventArgs e)
                    => callback.CallbackDelegate(sender, e.Loss);

                return new EventHandlerActions<Ped>()
                {
                    Add = (element) => element.Damaged += callbackProxy,
                    Remove = (element) => element.Damaged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Ped>(
            "onPedWasted",
            (callback) =>
            {
                void callbackProxy(Ped sender, PedWastedEventArgs e)
                    => callback.CallbackDelegate(sender, e.Ammo, e.Killer, (int)e.WeaponType, (int)e.BodyPart, false, (int)e.AnimationGroup, (int)e.AnimationId);

                return new EventHandlerActions<Ped>()
                {
                    Add = (element) => element.Wasted += callbackProxy,
                    Remove = (element) => element.Wasted -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Ped>(
            "onPedVehicleEnter",
            (callback) =>
            {
                void callbackProxy(Ped sender, ElementChangedEventArgs<Ped, Vehicle?> e)
                {
                    if (e.NewValue == null)
                        return;

                    var vehicle = e.NewValue;
                    var seat = sender.Seat ?? 0;
                    Ped? jacked = seat == 0 && vehicle.Driver != null && vehicle.Driver != sender ? vehicle.Driver : null;
                    callback.CallbackDelegate(sender, vehicle, seat, jacked);
                }

                return new EventHandlerActions<Ped>()
                {
                    Add = (element) => element.VehicleChanged += callbackProxy,
                    Remove = (element) => element.VehicleChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Ped>(
            "onPedVehicleExit",
            (callback) =>
            {
                void callbackProxy(Ped sender, ElementChangedEventArgs<Ped, Vehicle?> e)
                {
                    if (e.OldValue == null)
                        return;

                    var vehicle = e.OldValue;
                    var seat = sender.Seat ?? 0;
                    Ped? jacker = sender.VehicleAction == VehicleAction.Jacked ? vehicle.Driver : null;
                    callback.CallbackDelegate(sender, vehicle, seat, jacker, false);
                }

                return new EventHandlerActions<Ped>()
                {
                    Add = (element) => element.VehicleChanged += callbackProxy,
                    Remove = (element) => element.VehicleChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Ped>(
            "onPedWeaponSwitch",
            (callback) =>
            {
                void callbackProxy(Ped sender, ElementChangedEventArgs<Ped, WeaponSlot> e)
                {
                    var previousWeaponId = (int)(sender.Weapons.Get(e.OldValue)?.Type ?? WeaponId.Fist);
                    var currentWeaponId = (int)(sender.Weapons.Get(e.NewValue)?.Type ?? WeaponId.Fist);
                    callback.CallbackDelegate(sender, previousWeaponId, currentWeaponId);
                }

                return new EventHandlerActions<Ped>()
                {
                    Add = (element) => element.WeaponSlotChanged += callbackProxy,
                    Remove = (element) => element.WeaponSlotChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Ped>(
            "onPedWeaponReload",
            (callback) =>
            {
                void callbackProxy(Ped sender, EventArgs e)
                {
                    var weapon = sender.CurrentWeapon;
                    if (weapon != null)
                        callback.CallbackDelegate(sender, (int)weapon.Type, weapon.AmmoInClip, weapon.Ammo);
                }

                return new EventHandlerActions<Ped>()
                {
                    Add = (element) => element.WeaponReloaded += callbackProxy,
                    Remove = (element) => element.WeaponReloaded -= callbackProxy
                };
            }
        );
    }
}
