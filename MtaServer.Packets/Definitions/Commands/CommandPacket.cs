using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MtaServer.Packets.Reader;

namespace MtaServer.Packets.Definitions.Commands
{
    public class CommandPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_COMMAND;
        public override PacketFlags Flags => PacketFlags.PACKET_HIGH_PRIORITY;

        public string Command { get; private set; }
        public string[] Arguments { get; private set; }

        public CommandPacket()
        {

        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
            string[] commandArgs = reader.GetStringCharacters(bytes.Length).Split(' ');
            Command = commandArgs[0];
            Arguments = commandArgs.Skip(1).ToArray();
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }
    }
}
