﻿using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using System.Collections.Generic;
using SlipeServer.Server.PacketHandling.Factories;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents the player's cotrols, containing properties for easy access to enabling / disabling a player's controls.
/// </summary>
public class Controls
{
    public static string[] AllControls = ["fire", "aim_weapon", "next_weapon", "previous_weapon", "forwards", "backwards", "left", "right", "zoom_in", "zoom_out", "change_camera", "jump", "sprint", "look_behind", "crouch", "action", "walk", "conversation_yes", "conversation_no", "group_control_forwards", "group_control_back", "enter_exit", "vehicle_fire", "vehicle_secondary_fire", "vehicle_left", "vehicle_right", "steer_forward", "steer_back", "accelerate", "brake_reverse", "radio_next", "radio_previous", "radio_user_track_skip", "horn", "sub_mission", "handbrake", "vehicle_look_left", "vehicle_look_right", "vehicle_look_behind", "vehicle_mouse_look", "special_control_left", "special_control_right", "special_control_down", "special_control_up"];

    private readonly Player player;

    public Controls(Player player)
    {
        this.player = player;
    }

    public void ToggleAll(bool isEnabled, bool gtaControls = true, bool mtaControls = true)
    {
        this.player.Client.SendPacket(PlayerPacketFactory.CreateToggleAllControlsPacket(isEnabled, gtaControls, mtaControls));
        this.fireEnabled = isEnabled;
        this.aimWeaponEnabled = isEnabled;
        this.nextWeaponEnabled = isEnabled;
        this.previousWeaponEnabled = isEnabled;
        this.forwardsEnabled = isEnabled;
        this.backwardsEnabled = isEnabled;
        this.leftEnabled = isEnabled;
        this.rightEnabled = isEnabled;
        this.zoomInEnabled = isEnabled;
        this.zoomOutEnabled = isEnabled;
        this.changeCameraEnabled = isEnabled;
        this.jumpEnabled = isEnabled;
        this.sprintEnabled = isEnabled;
        this.lookBehindEnabled = isEnabled;
        this.crouchEnabled = isEnabled;
        this.actionEnabled = isEnabled;
        this.walkEnabled = isEnabled;
        this.conversationYesEnabled = isEnabled;
        this.conversationNoEnabled = isEnabled;
        this.groupControlForwardsEnabled = isEnabled;
        this.groupControlBackEnabled = isEnabled;
        this.vehicleFireEnabled = isEnabled;
        this.vehicleSecondaryFireEnabled = isEnabled;
        this.vehicleLeftEnabled = isEnabled;
        this.vehicleRightEnabled = isEnabled;
        this.steerForwardEnabled = isEnabled;
        this.steerBackEnabled = isEnabled;
        this.accelerateEnabled = isEnabled;
        this.brakeReverseEnabled = isEnabled;
        this.radioNextEnabled = isEnabled;
        this.radioPreviousEnabled = isEnabled;
        this.radioUserTrackSkipEnabled = isEnabled;
        this.hornEnabled = isEnabled;
        this.subMissionEnabled = isEnabled;
        this.handbrakeEnabled = isEnabled;
        this.vehicleLookLeftEnabled = isEnabled;
        this.vehicleLookRightEnabled = isEnabled;
        this.vehicleLookBehindEnabled = isEnabled;
        this.vehicleMouseLookEnabled = isEnabled;
        this.specialControlLeftEnabled = isEnabled;
        this.specialControlRightEnabled = isEnabled;
        this.specialControlDownEnabled = isEnabled;
        this.specialControlUpEnabled = isEnabled;
        this.enterExitEnabled = isEnabled;
        this.enterPassengerEnabled = isEnabled;
        this.screenshotEnabled = isEnabled;
        this.chatboxEnabled = isEnabled;
        this.radarEnabled = isEnabled;
        this.radarZoomInEnabled = isEnabled;
        this.radarZoomOutEnabled = isEnabled;
        this.radarMoveNorthEnabled = isEnabled;
        this.radarMoveSouthEnabled = isEnabled;
        this.radarMoveEastEnabled = isEnabled;
        this.radarMoveWestEnabled = isEnabled;
        this.radarAttachEnabled = isEnabled;
        var args = new PlayerControlsChangedArgs(this.player, AllControls, isEnabled);
        StateChanged?.Invoke(this.player, args);
    }

    private bool fireEnabled = true;
    public bool FireEnabled
    {
        get => this.fireEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "fire", value);
            this.player.Client.SendPacket(new ToggleControlAbility("fire", value));
            this.fireEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool aimWeaponEnabled = true;
    public bool AimWeaponEnabled
    {
        get => this.aimWeaponEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "aim_weapon", value);
            this.player.Client.SendPacket(new ToggleControlAbility("aim_weapon", value));
            this.aimWeaponEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool nextWeaponEnabled = true;
    public bool NextWeaponEnabled
    {
        get => this.nextWeaponEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "next_weapon", value);
            this.player.Client.SendPacket(new ToggleControlAbility("next_weapon", value));
            this.nextWeaponEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool previousWeaponEnabled = true;
    public bool PreviousWeaponEnabled
    {
        get => this.previousWeaponEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "previous_weapon", value);
            this.player.Client.SendPacket(new ToggleControlAbility("previous_weapon", value));
            this.previousWeaponEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool forwardsEnabled = true;
    public bool ForwardsEnabled
    {
        get => this.forwardsEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "forwards", value);
            this.player.Client.SendPacket(new ToggleControlAbility("forwards", value));
            this.forwardsEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool backwardsEnabled = true;
    public bool BackwardsEnabled
    {
        get => this.backwardsEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "backwards", value);
            this.player.Client.SendPacket(new ToggleControlAbility("backwards", value));
            this.backwardsEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool leftEnabled = true;
    public bool LeftEnabled
    {
        get => this.leftEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "left", value);
            this.player.Client.SendPacket(new ToggleControlAbility("left", value));
            this.leftEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool rightEnabled = true;
    public bool RightEnabled
    {
        get => this.rightEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "right", value);
            this.player.Client.SendPacket(new ToggleControlAbility("right", value));
            this.rightEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool zoomInEnabled = true;
    public bool ZoomInEnabled
    {
        get => this.zoomInEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "zoom_in", value);
            this.player.Client.SendPacket(new ToggleControlAbility("zoom_in", value));
            this.zoomInEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool zoomOutEnabled = true;
    public bool ZoomOutEnabled
    {
        get => this.zoomOutEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "zoom_out", value);
            this.player.Client.SendPacket(new ToggleControlAbility("zoom_out", value));
            this.zoomOutEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool changeCameraEnabled = true;
    public bool ChangeCameraEnabled
    {
        get => this.changeCameraEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "change_camera", value);
            this.player.Client.SendPacket(new ToggleControlAbility("change_camera", value));
            this.changeCameraEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool jumpEnabled = true;
    public bool JumpEnabled
    {
        get => this.jumpEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "jump", value);
            this.player.Client.SendPacket(new ToggleControlAbility("jump", value));
            this.jumpEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool sprintEnabled = true;
    public bool SprintEnabled
    {
        get => this.sprintEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "sprint", value);
            this.player.Client.SendPacket(new ToggleControlAbility("sprint", value));
            this.sprintEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool lookBehindEnabled = true;
    public bool LookBehindEnabled
    {
        get => this.lookBehindEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "look_behind", value);
            this.player.Client.SendPacket(new ToggleControlAbility("look_behind", value));
            this.lookBehindEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool crouchEnabled = true;
    public bool CrouchEnabled
    {
        get => this.crouchEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "crouch", value);
            this.player.Client.SendPacket(new ToggleControlAbility("crouch", value));
            this.crouchEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool actionEnabled = true;
    public bool ActionEnabled
    {
        get => this.actionEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "action", value);
            this.player.Client.SendPacket(new ToggleControlAbility("action", value));
            this.actionEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool walkEnabled = true;
    public bool WalkEnabled
    {
        get => this.walkEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "walk", value);
            this.player.Client.SendPacket(new ToggleControlAbility("walk", value));
            this.walkEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool conversationYesEnabled = true;
    public bool ConversationYesEnabled
    {
        get => this.conversationYesEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "conversation_yes", value);
            this.player.Client.SendPacket(new ToggleControlAbility("conversation_yes", value));
            this.conversationYesEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool conversationNoEnabled = true;
    public bool ConversationNoEnabled
    {
        get => this.conversationNoEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "conversation_no", value);
            this.player.Client.SendPacket(new ToggleControlAbility("conversation_no", value));
            this.conversationNoEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool groupControlForwardsEnabled = true;
    public bool GroupControlForwardsEnabled
    {
        get => this.groupControlForwardsEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "group_control_forwards", value);
            this.player.Client.SendPacket(new ToggleControlAbility("group_control_forwards", value));
            this.groupControlForwardsEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool groupControlBackEnabled = true;
    public bool GroupControlBackEnabled
    {
        get => this.groupControlBackEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "group_control_back", value);
            this.player.Client.SendPacket(new ToggleControlAbility("group_control_back", value));
            this.groupControlBackEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool vehicleFireEnabled = true;
    public bool VehicleFireEnabled
    {
        get => this.vehicleFireEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "vehicle_fire", value);
            this.player.Client.SendPacket(new ToggleControlAbility("vehicle_fire", value));
            this.vehicleFireEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool vehicleSecondaryFireEnabled = true;
    public bool VehicleSecondaryFireEnabled
    {
        get => this.vehicleSecondaryFireEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "vehicle_secondary_fire", value);
            this.player.Client.SendPacket(new ToggleControlAbility("vehicle_secondary_fire", value));
            this.vehicleSecondaryFireEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool vehicleLeftEnabled = true;
    public bool VehicleLeftEnabled
    {
        get => this.vehicleLeftEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "vehicle_left", value);
            this.player.Client.SendPacket(new ToggleControlAbility("vehicle_left", value));
            this.vehicleLeftEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool vehicleRightEnabled = true;
    public bool VehicleRightEnabled
    {
        get => this.vehicleRightEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "vehicle_right", value);
            this.player.Client.SendPacket(new ToggleControlAbility("vehicle_right", value));
            this.vehicleRightEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool steerForwardEnabled = true;
    public bool SteerForwardEnabled
    {
        get => this.steerForwardEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "steer_forward", value);
            this.player.Client.SendPacket(new ToggleControlAbility("steer_forward", value));
            this.steerForwardEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool steerBackEnabled = true;
    public bool SteerBackEnabled
    {
        get => this.steerBackEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "steer_back", value);
            this.player.Client.SendPacket(new ToggleControlAbility("steer_back", value));
            this.steerBackEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool accelerateEnabled = true;
    public bool AccelerateEnabled
    {
        get => this.accelerateEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "accelerate", value);
            this.player.Client.SendPacket(new ToggleControlAbility("accelerate", value));
            this.accelerateEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool brakeReverseEnabled = true;
    public bool BrakeReverseEnabled
    {
        get => this.brakeReverseEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "brake_reverse", value);
            this.player.Client.SendPacket(new ToggleControlAbility("brake_reverse", value));
            this.brakeReverseEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radioNextEnabled = true;
    public bool RadioNextEnabled
    {
        get => this.radioNextEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radio_next", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radio_next", value));
            this.radioNextEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radioPreviousEnabled = true;
    public bool RadioPreviousEnabled
    {
        get => this.radioPreviousEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radio_previous", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radio_previous", value));
            this.radioPreviousEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radioUserTrackSkipEnabled = true;
    public bool RadioUserTrackSkipEnabled
    {
        get => this.radioUserTrackSkipEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radio_user_track_skip", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radio_user_track_skip", value));
            this.radioUserTrackSkipEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool hornEnabled = true;
    public bool HornEnabled
    {
        get => this.hornEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "horn", value);
            this.player.Client.SendPacket(new ToggleControlAbility("horn", value));
            this.hornEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool subMissionEnabled = true;
    public bool SubMissionEnabled
    {
        get => this.subMissionEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "sub_mission", value);
            this.player.Client.SendPacket(new ToggleControlAbility("sub_mission", value));
            this.subMissionEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool handbrakeEnabled = true;
    public bool HandbrakeEnabled
    {
        get => this.handbrakeEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "handbrake", value);
            this.player.Client.SendPacket(new ToggleControlAbility("handbrake", value));
            this.handbrakeEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool vehicleLookLeftEnabled = true;
    public bool VehicleLookLeftEnabled
    {
        get => this.vehicleLookLeftEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "vehicle_look_left", value);
            this.player.Client.SendPacket(new ToggleControlAbility("vehicle_look_left", value));
            this.vehicleLookLeftEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool vehicleLookRightEnabled = true;
    public bool VehicleLookRightEnabled
    {
        get => this.vehicleLookRightEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "vehicle_look_right", value);
            this.player.Client.SendPacket(new ToggleControlAbility("vehicle_look_right", value));
            this.vehicleLookRightEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool vehicleLookBehindEnabled = true;
    public bool VehicleLookBehindEnabled
    {
        get => this.vehicleLookBehindEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "vehicle_look_behind", value);
            this.player.Client.SendPacket(new ToggleControlAbility("vehicle_look_behind", value));
            this.vehicleLookBehindEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool vehicleMouseLookEnabled = true;
    public bool VehicleMouseLookEnabled
    {
        get => this.vehicleMouseLookEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "vehicle_mouse_look", value);
            this.player.Client.SendPacket(new ToggleControlAbility("vehicle_mouse_look", value));
            this.vehicleMouseLookEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool specialControlLeftEnabled = true;
    public bool SpecialControlLeftEnabled
    {
        get => this.specialControlLeftEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "special_control_left", value);
            this.player.Client.SendPacket(new ToggleControlAbility("special_control_left", value));
            this.specialControlLeftEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool specialControlRightEnabled = true;
    public bool SpecialControlRightEnabled
    {
        get => this.specialControlRightEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "special_control_right", value);
            this.player.Client.SendPacket(new ToggleControlAbility("special_control_right", value));
            this.specialControlRightEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool specialControlDownEnabled = true;
    public bool SpecialControlDownEnabled
    {
        get => this.specialControlDownEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "special_control_down", value);
            this.player.Client.SendPacket(new ToggleControlAbility("special_control_down", value));
            this.specialControlDownEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool specialControlUpEnabled = true;
    public bool SpecialControlUpEnabled
    {
        get => this.specialControlUpEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "special_control_up", value);
            this.player.Client.SendPacket(new ToggleControlAbility("special_control_up", value));
            this.specialControlUpEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool enterExitEnabled = true;
    public bool EnterExitEnabled
    {
        get => this.enterExitEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "enter_exit", value);
            this.player.Client.SendPacket(new ToggleControlAbility("enter_exit", value));
            this.enterExitEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool enterPassengerEnabled = true;
    public bool EnterPassengerEnabled
    {
        get => this.enterPassengerEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "enter_passenger", value);
            this.player.Client.SendPacket(new ToggleControlAbility("enter_passenger", value));
            this.enterPassengerEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool screenshotEnabled = true;
    public bool ScreenshotEnabled
    {
        get => this.screenshotEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "screenshot", value);
            this.player.Client.SendPacket(new ToggleControlAbility("screenshot", value));
            this.screenshotEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool chatboxEnabled = true;
    public bool ChatboxEnabled
    {
        get => this.chatboxEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "chatbox", value);
            this.player.Client.SendPacket(new ToggleControlAbility("chatbox", value));
            this.chatboxEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radarEnabled = true;
    public bool RadarEnabled
    {
        get => this.radarEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radar", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radar", value));
            this.radarEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radarZoomInEnabled = true;
    public bool RadarZoomInEnabled
    {
        get => this.radarZoomInEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radar_zoom_in", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radar_zoom_in", value));
            this.radarZoomInEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radarZoomOutEnabled = true;
    public bool RadarZoomOutEnabled
    {
        get => this.radarZoomOutEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radar_zoom_out", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radar_zoom_out", value));
            this.radarZoomOutEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radarMoveNorthEnabled = true;
    public bool RadarMoveNorthEnabled
    {
        get => this.radarMoveNorthEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radar_move_north", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radar_move_north", value));
            this.radarMoveNorthEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radarMoveSouthEnabled = true;
    public bool RadarMoveSouthEnabled
    {
        get => this.radarMoveSouthEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radar_move_south", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radar_move_south", value));
            this.radarMoveSouthEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radarMoveEastEnabled = true;
    public bool RadarMoveEastEnabled
    {
        get => this.radarMoveEastEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radar_move_east", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radar_move_east", value));
            this.radarMoveEastEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radarMoveWestEnabled = true;
    public bool RadarMoveWestEnabled
    {
        get => this.radarMoveWestEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radar_move_west", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radar_move_west", value));
            this.radarMoveWestEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private bool radarAttachEnabled = true;
    public bool RadarAttachEnabled
    {
        get => this.radarAttachEnabled;
        set
        {
            var args = new PlayerControlsChangedArgs(this.player, "radar_attach", value);
            this.player.Client.SendPacket(new ToggleControlAbility("radar_attach", value));
            this.radarAttachEnabled = value;
            StateChanged?.Invoke(this.player, args);
        }
    }

    private readonly HashSet<string> enabledControlStates = [];

    public void SetControlState(string control, bool state)
    {
        if (state)
            this.enabledControlStates.Add(control);
        else
            this.enabledControlStates.Remove(control);

        new SetControlStatePacket(control, state)
            .SendTo(this.player);
    }

    public void SetControlState(Control control, bool state)
    {
        SetControlState(control.ToString().ToLower(), state);
    }

    public bool IsControlStateSet(string control)
    {
        return this.enabledControlStates.Contains(control);
    }

    public bool IsControlStateSet(Control control)
    {
        return IsControlStateSet(control.ToString().ToLower());
    }

    public void SetAllEnabled(bool newState)
    {
        this.FireEnabled = newState;
        this.AimWeaponEnabled = newState;
        this.NextWeaponEnabled = newState;
        this.PreviousWeaponEnabled = newState;
        this.ForwardsEnabled = newState;
        this.BackwardsEnabled = newState;
        this.LeftEnabled = newState;
        this.RightEnabled = newState;
        this.ZoomInEnabled = newState;
        this.ZoomOutEnabled = newState;
        this.ChangeCameraEnabled = newState;
        this.JumpEnabled = newState;
        this.SprintEnabled = newState;
        this.LookBehindEnabled = newState;
        this.CrouchEnabled = newState;
        this.ActionEnabled = newState;
        this.WalkEnabled = newState;
        this.ConversationYesEnabled = newState;
        this.ConversationNoEnabled = newState;
        this.GroupControlForwardsEnabled = newState;
        this.GroupControlBackEnabled = newState;
        this.VehicleFireEnabled = newState;
        this.VehicleSecondaryFireEnabled = newState;
        this.VehicleLeftEnabled = newState;
        this.VehicleRightEnabled = newState;
        this.SteerForwardEnabled = newState;
        this.SteerBackEnabled = newState;
        this.AccelerateEnabled = newState;
        this.BrakeReverseEnabled = newState;
        this.RadioNextEnabled = newState;
        this.RadioPreviousEnabled = newState;
        this.RadioUserTrackSkipEnabled = newState;
        this.HornEnabled = newState;
        this.SubMissionEnabled = newState;
        this.HandbrakeEnabled = newState;
        this.VehicleLookLeftEnabled = newState;
        this.VehicleLookRightEnabled = newState;
        this.VehicleLookBehindEnabled = newState;
        this.VehicleMouseLookEnabled = newState;
        this.SpecialControlLeftEnabled = newState;
        this.SpecialControlRightEnabled = newState;
        this.SpecialControlDownEnabled = newState;
        this.SpecialControlUpEnabled = newState;
        this.EnterExitEnabled = newState;
        this.EnterPassengerEnabled = newState;
        this.ScreenshotEnabled = newState;
        this.ChatboxEnabled = newState;
        this.RadarEnabled = newState;
        this.RadarZoomInEnabled = newState;
        this.RadarZoomOutEnabled = newState;
        this.RadarMoveNorthEnabled = newState;
        this.RadarMoveSouthEnabled = newState;
        this.RadarMoveEastEnabled = newState;
        this.RadarMoveWestEnabled = newState;
        this.RadarAttachEnabled = newState;
    }

    public event ElementEventHandler<Player, PlayerControlsChangedArgs>? StateChanged;
}
