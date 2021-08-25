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
        public bool Fire
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
        public bool AimWeapon
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
        public bool NextWeapon
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
        public bool PreviousWeapon
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
        public bool Forwards
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
        public bool Backwards
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
        public bool Left
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
        public bool Right
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
        public bool ZoomIn
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
        public bool ZoomOut
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
        public bool ChangeCamera
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
        public bool Jump
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
        public bool Sprint
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
        public bool LookBehind
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
        public bool Crouch
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
        public bool Action
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
        public bool Walk
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
        public bool ConversationYes
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
        public bool ConversationNo
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
        public bool GroupControlForwards
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
        public bool GroupControlBack
        {
            get => this.groupControlBack;
            set
            {
                var args = new PlayerControlsChangedArgs(this.player, "group_control_back", value);
                this.groupControlBack = value;
                StateChanged?.Invoke(this.player, args);
            }
        }

        public event ElementEventHandler<Player, PlayerControlsChangedArgs>? StateChanged;
    }
}
