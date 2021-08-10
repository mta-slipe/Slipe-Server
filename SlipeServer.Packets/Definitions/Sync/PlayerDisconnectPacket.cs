using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public enum PlayerDisconnectType
    {
        NO_REASON,
        INVALID_PASSWORD,
        INVALID_NICKNAME,
        BANNED_SERIAL,
        BANNED_IP,
        BANNED_ACCOUNT,
        VERSION_MISMATCH,
        JOIN_FLOOD,
        INCORRECT_PASSWORD,
        DIFFERENT_BRANCH,
        BAD_VERSION,
        SERVER_NEWER,
        SERVER_OLDER,
        NICK_CLASH,
        ELEMENT_FAILURE,
        GENERAL_REFUSED,
        SERIAL_VERIFICATION,
        CONNECTION_DESYNC,
        BAN,
        KICK,
        CUSTOM,
        SHUTDOWN
    };

    public class PlayerDisconnectPacket : Packet
    { 
        public override PacketId PacketId => PacketId.PACKET_ID_DISCONNECT_MESSAGE;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;


        public PlayerDisconnectType Type { get; set; }
        public long Duration { get; set; }
        public string Reason { get; set; }

        public PlayerDisconnectPacket(string reason)
        {
            Type = PlayerDisconnectType.CUSTOM;
            Reason = reason;
            Duration = 0;
        }
        public PlayerDisconnectPacket(PlayerDisconnectType type, string reason)
        {
            Type = type;
            Reason = reason;
            Duration = 0;
        }


        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteCapped((int)Type, 5);

            if(Type == PlayerDisconnectType.BAN || Type == PlayerDisconnectType.BANNED_SERIAL || Type == PlayerDisconnectType.BANNED_IP)
            {
                builder.Write(Duration.ToString());
            }

            if(!string.IsNullOrEmpty(Reason))
            {
                builder.Write(Reason);
            }
            return builder.Build();
        }
    }
}
