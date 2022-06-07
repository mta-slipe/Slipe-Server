using SlipeServer.Server.Elements.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events
{
    public class PlayerBindKeyArgs : EventArgs
    {
        public Player Player { get; set; }
        public string Key { get; set; }
        public KeyState KeyState { get; set; }

        public PlayerBindKeyArgs(Player player, string key, KeyState keyState)
        {
            this.Player = player;
            this.Key = key;
            this.KeyState = keyState;
        }
    }
}
