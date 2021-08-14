using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class PlayerSpawnedEventArgs : EventArgs
    {
        public Player Source { get; }

        public PlayerSpawnedEventArgs(Player source)
        {
            this.Source = source;
        }
    }
}
