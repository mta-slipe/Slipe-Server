using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class PlayerCommandEventArgs : EventArgs
    {
        public Player Source { get; }
        public string Command { get; set; }
        public string[] Arguments { get; set; }

        public PlayerCommandEventArgs(Player source,string command, string[] arguments)
        {
            this.Source = source;
            this.Command = command;
            this.Arguments = arguments;
        }
    }
}
