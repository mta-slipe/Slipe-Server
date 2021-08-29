using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Server.TestTools
{
    public class TestingClient : Client
    {
        public Player TestingPlayer
        {
            get => this.Player as TestingPlayer;
            set => this.Player = value;
        }

        public TestingClient(uint address, INetWrapper netWrapper) : base(address, netWrapper)
        {

        }
    }
}
