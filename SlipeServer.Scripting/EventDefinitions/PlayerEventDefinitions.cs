using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Elements.Enums;
using System;
using System.IO;
using System.Linq;

namespace SlipeServer.Scripting.EventDefinitions;

public class PlayerEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Player>(
            "onPlayerJoin",
            (callback) =>
            {
                void callbackProxy(Player sender, EventArgs e)
                {
                    callback.CallbackDelegate(sender);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.Joined += callbackProxy,
                    Remove = (element) => element.Joined -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerWasted",
            (callback) =>
            {
                void callbackProxy(Element sender, PedWastedEventArgs e) => callback.CallbackDelegate(e.Source);
                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.Wasted += callbackProxy,
                    Remove = (element) => element.Wasted -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerChangeNick",
            (callback) =>
            {
                void callbackProxy(Element sender, ElementChangedEventArgs<string> e) => callback.CallbackDelegate(e.Source, e.OldValue, e.NewValue, false);
                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.NameChanged += callbackProxy,
                    Remove = (element) => element.NameChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerClick",
            (callback) =>
            {
                void callbackProxy(Element sender, PlayerCursorClickedEventArgs e)
                    => callback.CallbackDelegate(sender, e.Button.ToString(), e.IsDown ? "down" : "up", e.Element, e.WorldPosition, e.Position);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.CursorClicked += callbackProxy,
                    Remove = (element) => element.CursorClicked -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerCommand",
            (callback) =>
            {
                void callbackProxy(Element sender, PlayerCommandEventArgs e)
                    => callback.CallbackDelegate(sender, e.Command);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.CommandEntered += callbackProxy,
                    Remove = (element) => element.CommandEntered -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerContact",
            (callback) =>
            {
                void callbackProxy(Player sender, ElementChangedEventArgs<Player, Element?> e)
                    => callback.CallbackDelegate(sender, e.OldValue, e.NewValue);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.ContactElementChanged += callbackProxy,
                    Remove = (element) => element.ContactElementChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerDamage",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerDamagedEventArgs e)
                    => callback.CallbackDelegate(sender, e.Damager, (int)e.WeaponType, (int)e.BodyPart, e.Loss);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.Damaged += callbackProxy,
                    Remove = (element) => element.Damaged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerQuit",
            (callback) =>
            {
                void quitCallbackProxy(Player sender, PlayerQuitEventArgs e)
                {
                    if (e.Reason == QuitReason.Kick)
                        return;

                    var quitType = e.Reason switch
                    {
                        QuitReason.Quit => "Quit",
                        QuitReason.Kick => "Kicked",
                        QuitReason.Ban => "Banned",
                        QuitReason.ConnectionDesync => "Bad Connection",
                        QuitReason.Timeout => "Timed out",
                        _ => "Unknown",
                    };

                    callback.CallbackDelegate(sender, e.Reason.ToString(), null, null);
                }

                void kickCallbackProxy(Player sender, PlayerKickEventArgs e)
                {
                    callback.CallbackDelegate(sender, "Kicked", e.Reason, e.ResponsibleElement);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) =>
                    {
                        element.Disconnected += quitCallbackProxy;
                        element.Kicked += kickCallbackProxy;
                    },
                    Remove = (element) =>
                    {
                        element.Disconnected -= quitCallbackProxy;
                        element.Kicked -= kickCallbackProxy;
                    }
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerScreenShot",
            (callback) =>
            {
                void callbackProxy(Player sender, ScreenshotEventArgs e)
                {
                    using var reader = new StreamReader(e.Stream);
                    var data = reader.ReadToEnd();
                    callback.CallbackDelegate(sender, e.ErrorMessage ?? "ok", data, e.Stream, DateTime.Now, e.Tag);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.ScreenshotTaken += callbackProxy,
                    Remove = (element) => element.ScreenshotTaken -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerSpawn",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerSpawnedEventArgs e)
                    => callback.CallbackDelegate(sender, e.Position, e.Rotation, e.Team, e.Model, e.Interior, e.Dimension);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.Spawned += callbackProxy,
                    Remove = (element) => element.Spawned -= callbackProxy
                };
            }
        );

        //eventRuntime.RegisterEvent<Player>(
        //    "onPlayerStealthKill",
        //    (callback) =>
        //    {
        //    }
        //);

        eventRuntime.RegisterEvent<Player>(
            "onPlayerTarget",
            (callback) =>
            {
                void callbackProxy(Ped sender, ElementChangedEventArgs<Ped, Element?> e)
                    => callback.CallbackDelegate(sender, e.NewValue);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.TargetChanged += callbackProxy,
                    Remove = (element) => element.TargetChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerTeamChange",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerTeamChangedArgs e)
                    => callback.CallbackDelegate(sender, e.PreviousTeam, e.NewTeam);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.TeamChanged += callbackProxy,
                    Remove = (element) => element.TeamChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerMute",
            (callback) =>
            {
                void callbackProxy(Player sender, ElementChangedEventArgs<Player, bool> e)
                {
                    if (e.NewValue == true)
                        callback.CallbackDelegate(sender);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.IsChatMutedChanged += callbackProxy,
                    Remove = (element) => element.IsChatMutedChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerUnmute",
            (callback) =>
            {
                void callbackProxy(Player sender, ElementChangedEventArgs<Player, bool> e)
                {
                    if (e.NewValue == false)
                        callback.CallbackDelegate(sender);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.IsChatMutedChanged += callbackProxy,
                    Remove = (element) => element.IsChatMutedChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerVehicleEnter",
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

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.VehicleChanged += callbackProxy,
                    Remove = (element) => element.VehicleChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerVehicleExit",
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

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.VehicleChanged += callbackProxy,
                    Remove = (element) => element.VehicleChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerPickupHit",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerPickupHitEventArgs e)
                    => callback.CallbackDelegate(sender, e.Pickup, sender.Dimension == e.Pickup.Dimension);
                return new EventHandlerActions<Player>()
                {
                    Add = (player) => player.PickupHit += callbackProxy,
                    Remove = (player) => player.PickupHit -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerPickupLeave",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerPickupLeftEventArgs e)
                    => callback.CallbackDelegate(sender, e.Pickup, sender.Dimension == e.Pickup.Dimension);
                return new EventHandlerActions<Player>()
                {
                    Add = (player) => player.PickupLeft += callbackProxy,
                    Remove = (player) => player.PickupLeft -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerPickupUse",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerPickupUsedEventArgs e)
                    => callback.CallbackDelegate(sender, e.Pickup);
                return new EventHandlerActions<Player>()
                {
                    Add = (player) => player.PickupUsed += callbackProxy,
                    Remove = (player) => player.PickupUsed -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerVoiceStart",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerVoiceStartArgs e)
                    => callback.CallbackDelegate(sender);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.VoiceDataReceived += callbackProxy,
                    Remove = (element) => element.VoiceDataReceived -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerVoiceStop",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerVoiceEndArgs e)
                    => callback.CallbackDelegate(sender);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.VoiceDataEnded += callbackProxy,
                    Remove = (element) => element.VoiceDataEnded -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerACInfo",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerACInfoArgs e)
                    => callback.CallbackDelegate(sender, e.DetectedACList.ToList(), e.D3D9Size, e.D3D9MD5, e.D3D9SHA256);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.AcInfoReceived += callbackProxy,
                    Remove = (element) => element.AcInfoReceived -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerModInfo",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerModInfoArgs e)
                    => callback.CallbackDelegate(sender, e.InfoType, e.ModInfoItems.ToList());

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.ModInfoReceived += callbackProxy,
                    Remove = (element) => element.ModInfoReceived -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerNetworkStatus",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerNetworkStatusArgs e)
                    => callback.CallbackDelegate(sender, (int)e.PlayerNetworkStatus, e.Ticks);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.NetworkStatusReceived += callbackProxy,
                    Remove = (element) => element.NetworkStatusReceived -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerWeaponSwitch",
            (callback) =>
            {
                void callbackProxy(Ped sender, ElementChangedEventArgs<Ped, WeaponSlot> e)
                {
                    var previousWeaponId = (int)(sender.Weapons.Get(e.OldValue)?.Type ?? WeaponId.Fist);
                    var currentWeaponId = (int)(sender.Weapons.Get(e.NewValue)?.Type ?? WeaponId.Fist);
                    callback.CallbackDelegate(sender, previousWeaponId, currentWeaponId);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.WeaponSlotChanged += callbackProxy,
                    Remove = (element) => element.WeaponSlotChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerWeaponReload",
            (callback) =>
            {
                void callbackProxy(Ped sender, EventArgs e)
                {
                    var weapon = sender.CurrentWeapon;
                    if (weapon != null)
                        callback.CallbackDelegate(sender, (int)weapon.Type, weapon.AmmoInClip, weapon.Ammo);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.WeaponReloaded += callbackProxy,
                    Remove = (element) => element.WeaponReloaded -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerWeaponFire",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerWeaponFiredEventArgs e)
                    => callback.CallbackDelegate(sender, (int)e.Weapon, e.EndPosition.X, e.EndPosition.Y, e.EndPosition.Z, e.HitElement, e.StartPosition.X, e.StartPosition.Y, e.StartPosition.Z);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.WeaponFired += callbackProxy,
                    Remove = (element) => element.WeaponFired -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerDetonateSatchels",
            (callback) =>
            {
                void callbackProxy(Player sender, EventArgs e)
                    => callback.CallbackDelegate(sender);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.SatchelsDetonated += callbackProxy,
                    Remove = (element) => element.SatchelsDetonated -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerChat",
            (callback) =>
            {
                void callbackProxy(Element sender, PlayerCommandEventArgs e)
                {
                    if (e.Command == "say")
                        callback.CallbackDelegate(sender, string.Join(' ', e.Arguments), 0);
                }

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.CommandEntered += callbackProxy,
                    Remove = (element) => element.CommandEntered -= callbackProxy
                };
            }
        );
    }
}
