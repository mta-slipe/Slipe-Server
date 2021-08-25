using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.ElementConcepts
{
    public class Controls
    {
        private readonly Player player;

        public Controls(Player player)
        {
            this.player = player;
            StateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(Player player, PlayerControlsChangedArgs args)
        {
            player.Client.SendPacket(new ToggleControlAbility(args.Control, args.NewState));
        }

        private bool fire = true;
        public bool FireEnabled
        {
            get => this.fire;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "fire", value);
                this.fire = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool aimWeapon = true;
        public bool AimWeaponEnabled
        {
            get => this.aimWeapon;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "aim_weapon", value);
                this.aimWeapon = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool nextWeapon = true;
        public bool NextWeaponEnabled
        {
            get => this.nextWeapon;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "next_weapon", value);
                this.nextWeapon = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool previousWeapon = true;
        public bool PreviousWeaponEnabled
        {
            get => this.previousWeapon;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "previous_weapon", value);
                this.previousWeapon = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool forwards = true;
        public bool ForwardsEnabled
        {
            get => this.forwards;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "forwards", value);
                this.forwards = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool backwards = true;
        public bool BackwardsEnabled
        {
            get => this.backwards;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "backwards", value);
                this.backwards = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool left = true;
        public bool LeftEnabled
        {
            get => this.left;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "left", value);
                this.left = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool right = true;
        public bool RightEnabled
        {
            get => this.right;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "right", value);
                this.right = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool zoomIn = true;
        public bool ZoomInEnabled
        {
            get => this.zoomIn;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "zoom_in", value);
                this.zoomIn = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool zoomOut = true;
        public bool ZoomOutEnabled
        {
            get => this.zoomOut;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "zoom_out", value);
                this.zoomOut = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool changeCamera = true;
        public bool ChangeCameraEnabled
        {
            get => this.changeCamera;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "change_camera", value);
                this.changeCamera = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool jump = true;
        public bool JumpEnabled
        {
            get => this.jump;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "jump", value);
                this.jump = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool sprint = true;
        public bool SprintEnabled
        {
            get => this.sprint;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "sprint", value);
                this.sprint = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool lookBehind = true;
        public bool LookBehindEnabled
        {
            get => this.lookBehind;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "look_behind", value);
                this.lookBehind = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool crouch = true;
        public bool CrouchEnabled
        {
            get => this.crouch;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "crouch", value);
                this.crouch = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool action = true;
        public bool ActionEnabled
        {
            get => this.action;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "action", value);
                this.action = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool walk = true;
        public bool WalkEnabled
        {
            get => this.walk;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "walk", value);
                this.walk = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool conversationYes = true;
        public bool ConversationYesEnabled
        {
            get => this.conversationYes;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "conversation_yes", value);
                this.conversationYes = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool conversationNo = true;
        public bool ConversationNoEnabled
        {
            get => this.conversationNo;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "conversation_no", value);
                this.conversationNo = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool groupControlForwards = true;
        public bool GroupControlForwardsEnabled
        {
            get => this.groupControlForwards;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "group_control_forwards", value);
                this.groupControlForwards = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool groupControlBack = true;
        public bool GroupControlBackEnabled
        {
            get => this.groupControlBack;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "group_control_back", value);
                this.groupControlBack = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool vehicleFire = true;
        public bool VehicleFireEnabled
        {
            get => this.vehicleFire;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "vehicle_fire", value);
                this.vehicleFire = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool vehicleSecondaryFire = true;
        public bool VehicleSecondaryFireEnabled
        {
            get => this.vehicleSecondaryFire;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "vehicle_secondary_fire", value);
                this.vehicleSecondaryFire = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool vehicleLeft = true;
        public bool VehicleLeftEnabled
        {
            get => this.vehicleLeft;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "vehicle_left", value);
                this.vehicleLeft = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool vehicleRight = true;
        public bool VehicleRightEnabled
        {
            get => this.vehicleRight;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "vehicle_right", value);
                this.vehicleRight = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool steerForward = true;
        public bool SteerForwardEnabled
        {
            get => this.steerForward;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "steer_forward", value);
                this.steerForward = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool steerBack = true;
        public bool SteerBackEnabled
        {
            get => this.steerBack;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "steer_back", value);
                this.steerBack = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool accelerate = true;
        public bool AccelerateEnabled
        {
            get => this.accelerate;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "accelerate", value);
                this.accelerate = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool brakeReverse = true;
        public bool BrakeReverseEnabled
        {
            get => this.brakeReverse;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "brake_reverse", value);
                this.brakeReverse = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radioNext = true;
        public bool RadioNextEnabled
        {
            get => this.radioNext;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radio_next", value);
                this.radioNext = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radioPrevious = true;
        public bool RadioPreviousEnabled
        {
            get => this.radioPrevious;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radio_previous", value);
                this.radioPrevious = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radioUserTrackSkip = true;
        public bool RadioUserTrackSkipEnabled
        {
            get => this.radioUserTrackSkip;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radio_user_track_skip", value);
                this.radioUserTrackSkip = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool horn = true;
        public bool HornEnabled
        {
            get => this.horn;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "horn", value);
                this.horn = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool subMission = true;
        public bool SubMissionEnabled
        {
            get => this.subMission;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "sub_mission", value);
                this.subMission = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool handbrake = true;
        public bool HandbrakeEnabled
        {
            get => this.handbrake;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "handbrake", value);
                this.handbrake = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool vehicleLookLeft = true;
        public bool VehicleLookLeftEnabled
        {
            get => this.vehicleLookLeft;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "vehicle_look_left", value);
                this.vehicleLookLeft = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool vehicleLookRight = true;
        public bool VehicleLookRightEnabled
        {
            get => this.vehicleLookRight;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "vehicle_look_right", value);
                this.vehicleLookRight = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool vehicleLookBehind = true;
        public bool VehicleLookBehindEnabled
        {
            get => this.vehicleLookBehind;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "vehicle_look_behind", value);
                this.vehicleLookBehind = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool vehicleMouseLook = true;
        public bool VehicleMouseLookEnabled
        {
            get => this.vehicleMouseLook;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "vehicle_mouse_look", value);
                this.vehicleMouseLook = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool specialControlLeft = true;
        public bool SpecialControlLeftEnabled
        {
            get => this.specialControlLeft;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "special_control_left", value);
                this.specialControlLeft = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool specialControlRight = true;
        public bool SpecialControlRightEnabled
        {
            get => this.specialControlRight;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "special_control_right", value);
                this.specialControlRight = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool specialControlDown = true;
        public bool SpecialControlDownEnabled
        {
            get => this.specialControlDown;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "special_control_down", value);
                this.specialControlDown = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool specialControlUp = true;
        public bool SpecialControlUpEnabled
        {
            get => this.specialControlUp;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "special_control_up", value);
                this.specialControlUp = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool enterExit = true;
        public bool EnterExitEnabled
        {
            get => this.enterExit;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "enter_exit", value);
                this.enterExit = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool enterPassenger = true;
        public bool EnterPassengerEnabled
        {
            get => this.enterPassenger;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "enter_passenger", value);
                this.enterPassenger = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool screenshot = true;
        public bool ScreenshotEnabled
        {
            get => this.screenshot;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "screenshot", value);
                this.screenshot = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool chatbox = true;
        public bool ChatboxEnabled
        {
            get => this.chatbox;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "chatbox", value);
                this.chatbox = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radar = true;
        public bool RadarEnabled
        {
            get => this.radar;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radar", value);
                this.radar = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radarZoomIn = true;
        public bool RadarZoomInEnabled
        {
            get => this.radarZoomIn;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radar_zoom_in", value);
                this.radarZoomIn = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radarZoomOut = true;
        public bool RadarZoomOutEnabled
        {
            get => this.radarZoomOut;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radar_zoom_out", value);
                this.radarZoomOut = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radarMoveNorth = true;
        public bool RadarMoveNorthEnabled
        {
            get => this.radarMoveNorth;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radar_move_north", value);
                this.radarMoveNorth = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radarMoveSouth = true;
        public bool RadarMoveSouthEnabled
        {
            get => this.radarMoveSouth;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radar_move_south", value);
                this.radarMoveSouth = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radarMoveEast = true;
        public bool RadarMoveEastEnabled
        {
            get => this.radarMoveEast;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radar_move_east", value);
                this.radarMoveEast = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radarMoveWest = true;
        public bool RadarMoveWestEnabled
        {
            get => this.radarMoveWest;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radar_move_west", value);
                this.radarMoveWest = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        private bool radarAttach = true;
        public bool RadarAttachEnabled
        {
            get => this.radarAttach;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "radar_attach", value);
                this.radarAttach = value;
                StateChanged?.Invoke(this.player, args);
            }
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
}
