using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System;
using System.IO;

namespace SlipeServer.Scripting.EventDefinitions;

public class PlayerEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
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
        //        void callbackProxy(Player sender, PlayerSpawnedEventArgs e)
        //            => callback.CallbackDelegate(sender, e.Position, e.Rotation, e.Team, e.Model, e.Interior, e.Dimension);

        //        return new EventHandlerActions<Player>()
        //        {
        //            Add = (element) => element.Spawned += callbackProxy,
        //            Remove = (element) => element.Spawned -= callbackProxy
        //        };
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

        //eventRuntime.RegisterEvent<Player>(
        //    "onPlayerVehicleEnter",
        //    (callback) =>
        //    {
        //        //void callbackProxy(Ped sender, ElementChangedEventArgs<Ped, Vehicle> e)
        //        //{
        //        //    if (e.NewValue == true)
        //        //        callback.CallbackDelegate(sender);
        //        //}

        //        //return new EventHandlerActions<Player>()
        //        //{
        //        //    Add = (element) => element.VehicleChanged += callbackProxy,
        //        //    Remove = (element) => element.VehicleChanged -= callbackProxy
        //        //};
        //    }
        //);
    }
}
