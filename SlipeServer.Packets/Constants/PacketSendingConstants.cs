using SlipeServer.Packets.Enums;
using System.Collections.Generic;

namespace SlipeServer.Packets.Constants;

public static class PacketSendingConstants
{
    public static HashSet<PacketId> AlwaysAllowedPackets { get; } = new HashSet<PacketId>()
    {
        PacketId.PACKET_ID_MOD_NAME,
        PacketId.PACKET_ID_SERVER_JOIN_COMPLETE,
        PacketId.PACKET_ID_SERVER_JOINEDGAME,
        PacketId.PACKET_ID_PLAYER_TIMEOUT,
        PacketId.PACKET_ID_PLAYER_QUIT
    };
}
