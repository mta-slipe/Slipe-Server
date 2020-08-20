using MtaServer.Packets.Enums;
using MtaServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Join
{
    public class PlayerJoinDataPacket: Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_JOINDATA;
        public override PacketFlags Flags => throw new NotImplementedException();

        public ushort NetVersion { get; private set; }
        public ushort MtaVersion { get; private set; }
        public ushort BitStreamVersion { get; private set; }
        public string? PlayerVersion { get; private set; } // CMtaVersion type??
        public bool OptionalUpdateInfoRequired { get; private set; }
        public byte GameVersion { get; private set; } // unssigned
        public string? Nickname { get; private set; }
        public string? Password { get; private set; } // MD5 type??
        public string? Serial { get; private set; }
        public string? DiscordSecret { get; private set; }


        public override void Read(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            this.NetVersion = reader.GetUint16();
            this.MtaVersion = reader.GetUint16();
            this.BitStreamVersion = reader.GetUint16();
            this.PlayerVersion = reader.GetString();
            this.OptionalUpdateInfoRequired = reader.GetBit();
            this.GameVersion = reader.GetByte();
            this.Nickname = reader.GetStringCharacters(PacketConstants.MaxPlayerNickLength).TrimEnd('\0');
            this.Password = reader.GetStringCharacters(16).TrimEnd('\0');
            this.Serial = reader.GetStringCharacters(PacketConstants.MaxSerialLength);
            //this.DiscordSecret = reader.GetString();
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }
    }
}
