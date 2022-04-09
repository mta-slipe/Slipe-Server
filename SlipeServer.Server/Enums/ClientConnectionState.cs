using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.Enums;

public enum ClientConnectionState : byte
{
    Disconnected,
    ModNameSent = PacketId.PACKET_ID_MOD_NAME,
    ServerVersionSent = PacketId.PACKET_ID_SERVER_JOIN_COMPLETE,
    Joined = PacketId.PACKET_ID_SERVER_JOINEDGAME,
    TimedOut = PacketId.PACKET_ID_PLAYER_TIMEOUT,
    Quit = PacketId.PACKET_ID_PLAYER_QUIT
}
