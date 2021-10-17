using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Server.Events
{
    public class CommandTriggeredEventArgs : EventArgs
    {
        public Player Player { get; init; }
        public string[] Arguments { get; init; }

        public CommandTriggeredEventArgs(Player player, string[] arguments)
        {
            this.Player = player;
            this.Arguments = arguments;
        }
    }
}
