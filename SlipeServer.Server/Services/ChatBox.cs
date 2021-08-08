using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System;
using System.Drawing;

namespace SlipeServer.Server.Services
{
    public class ChatBox
    {
        private readonly MtaServer server;
        private readonly RootElement root;

        public ChatBox(MtaServer server, RootElement root)
        {
            this.server = server;
            this.root = root;
        }

        public void Output(string message, Color? color = null, bool isColorCoded = false, ChatEchoType type = ChatEchoType.Player, Element? source = null)
        {
            this.server.BroadcastPacket(new ChatEchoPacket(source?.Id ?? root.Id, message, color ?? Color.White, type, isColorCoded));
        }

        public void Clear()
        {
            this.server.BroadcastPacket(new ClearChatPacket());
        }

        public void OutputTo(Player player, string message, Color? color = null, bool isColorCoded = false, ChatEchoType type = ChatEchoType.Player, Element? source = null)
        {
            player.Client.SendPacket(new ChatEchoPacket(source?.Id ?? root.Id, message, color ?? Color.White, type, isColorCoded));
        }

        public void ClearFor(Player player)
        {
            player.Client.SendPacket(new ClearChatPacket());
        }
    }
}
