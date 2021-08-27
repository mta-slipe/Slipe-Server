using SlipeServer.Server.Elements.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events
{
    public class PlayerBindCallbackArgs : EventArgs
    {
        public Player Player { get; set; }
        public BindType BindType { get; set; }
        public string Key { get; set; }
        public KeyState KeyState { get; set; }

        public PlayerBindCallbackArgs(Player player, BindType bindType, KeyState keyState, string key)
        {
            this.Player = player;
            this.BindType = bindType;
            this.KeyState = keyState;
            this.Key = key;
        }
    }
}
