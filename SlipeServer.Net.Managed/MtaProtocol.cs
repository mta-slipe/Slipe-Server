namespace SlipeServer.Net.Managed;

/// <summary>
/// MTA wire-protocol constants: the offline magic, RakNet message ids and the admission rules.
/// </summary>
public static class MtaProtocol
{
    /// <summary>
    /// The modified offline magic. Differs from stock RakNet in byte 2 and bytes 12-15.
    /// </summary>
    public static readonly byte[] OfflineMagic =
    {
        0x00, 0xFF, 0x00, 0xFF, 0xFE, 0xFE, 0xFE, 0xFE,
        0xFD, 0xFD, 0xFD, 0xFD, 0x78, 0x56, 0x34, 0x17,
    };

    /// <summary><c>RID_USER_PACKET_ENUM</c> = 99: the first id above RakNet's own message ids.</summary>
    public const int RakNetUserPacketEnum = 99;

    /// <summary>The one wire offset applied in both directions.</summary>
    public const int MtaIdToWireId = RakNetUserPacketEnum;

    /// <summary>RakNet message ids that matter on the server side.</summary>
    public static class Rid
    {
        public const byte Ping = 1;                              // RID_PING
        public const byte PingOpenConnections = 2;               // RID_PING_OPEN_CONNECTIONS
        public const byte ConnectionRequest = 4;                 // RID_CONNECTION_REQUEST
        public const byte OpenConnectionRequest = 9;             // RID_OPEN_CONNECTION_REQUEST
        public const byte OpenConnectionReply = 10;              // RID_OPEN_CONNECTION_REPLY (client role)
        public const byte ConnectionRequestAccepted = 0x0E;      // RID_CONNECTION_REQUEST_ACCEPTED
        public const byte ConnectionAttemptFailed = 0x0F;
        public const byte AlreadyConnected = 0x10;
        public const byte NewIncomingConnection = 0x11;
        public const byte NoFreeIncomingConnections = 0x12;
        public const byte DisconnectionNotification = 0x13;
        public const byte ConnectionLost = 0x14;
        public const byte ConnectionBanned = 0x16;
        public const byte InvalidPassword = 0x17;
        public const byte ModifiedPacket = 0x18;
        public const byte Pong = 0x1A;
        public const byte MtaHandshake38 = 0x26;
        public const byte Refusal95 = 0x5F;
    }

    /// <summary>
    /// Symbolic names for the RakNet message ids this port recognises. Used in diagnostics, never on
    /// the wire.
    /// </summary>
    public static string RidName(byte id) => id switch
    {
        0 => "RID_INTERNAL_PING",
        1 => "RID_PING",
        2 => "RID_PING_OPEN_CONNECTIONS",
        3 => "RID_CONNECTED_PONG",
        4 => "RID_CONNECTION_REQUEST",
        5 => "RID_SECURED_CONNECTION_RESPONSE",
        6 => "RID_SECURED_CONNECTION_CONFIRMATION",
        7 => "RID_RPC_MAPPING",
        8 => "RID_DETECT_LOST_CONNECTIONS",
        9 => "RID_OPEN_CONNECTION_REQUEST",
        0x0A => "RID_OPEN_CONNECTION_REPLY",
        0x0B => "RID_RPC",
        0x0C => "RID_RPC_REPLY",
        0x0D => "RID_OUT_OF_BAND_INTERNAL",
        0x0E => "RID_CONNECTION_REQUEST_ACCEPTED",
        0x0F => "RID_CONNECTION_ATTEMPT_FAILED",
        0x10 => "RID_ALREADY_CONNECTED",
        0x11 => "RID_NEW_INCOMING_CONNECTION",
        0x12 => "RID_NO_FREE_INCOMING_CONNECTIONS",
        0x13 => "RID_DISCONNECTION_NOTIFICATION",
        0x14 => "RID_CONNECTION_LOST",
        0x15 => "RID_RSA_PUBLIC_KEY_MISMATCH",
        0x16 => "RID_CONNECTION_BANNED",
        0x17 => "RID_INVALID_PASSWORD",
        0x18 => "RID_MODIFIED_PACKET",
        0x19 => "RID_TIMESTAMP",
        0x1A => "RID_PONG",
        0x1B => "RID_ADVERTISE_SYSTEM",
        0x1C => "RID_REMOTE_DISCONNECTION_NOTIFICATION",
        0x1D => "RID_REMOTE_CONNECTION_LOST",
        0x1E => "RID_REMOTE_NEW_INCOMING_CONNECTION",
        0x1F => "RID_DOWNLOAD_PROGRESS",
        0x26 => "MTA_HANDSHAKE_0x26",
        0x5F => "RID_INCOMPATIBLE_PROTOCOL_VERSION",
        _ => "?",
    };

    /// <summary>
    /// Where the offline magic sits in each offline message, and how the message is checked.
    /// <c>ExpectedLength == 0</c> means "any length, subject to <c>offset + 16 &lt;= length</c>".
    /// </summary>
    public readonly record struct MagicGeometry(int Offset, int ExpectedLength, int BytesCompared);

    private static readonly MagicGeometry DefaultGeometry = new(1, 0, 16);

    public static MagicGeometry GetMagicGeometry(byte messageId) => messageId switch
    {
        1 or 2 => new MagicGeometry(9, 0x19, 0x0F),
        4 or 0x0F or 0x10 or 0x12 or 0x16 => new MagicGeometry(1, 0x21, 16),
        9 or 0x0D => new MagicGeometry(0x12, 0, 16),
        0x0A or 0x26 => new MagicGeometry(1, 0, 16),
        0x5F => new MagicGeometry(2, 0x22, 16),
        _ => default,
    };

    /// <summary>
    /// True when <paramref name="messageId"/> is an id that has a magic geometry. Ids outside this
    /// set never take the offline path.
    /// </summary>
    public static bool HasMagicGeometry(byte messageId) =>
        messageId is 1 or 2 or 4 or 0x0F or 0x10 or 0x12 or 0x16 or 9 or 0x0D or 0x0A or 0x26 or 0x5F;

    /// <summary>
    /// Applies the geometry guard and the 16-byte (or 15-byte, for ping) magic compare.
    /// <paramref name="bytesCompared"/> is honoured, which is what makes <c>RID_PING</c> compare
    /// only the first 15 magic bytes.
    ///
    /// This overload measures the datagram with <c>datagram.Length</c> and exists only for callers
    /// that hold a right-sized array. The socket path must use the
    /// <see cref="TryValidateMagic(byte[], int, byte, out MagicGeometry)"/> overload instead: its
    /// receive buffer is a reused 1492-byte array, so <c>datagram.Length</c> is not the datagram
    /// length and every id with a fixed <c>ExpectedLength</c> would fail the guard.
    /// </summary>
    public static bool TryValidateMagic(byte[] datagram, byte messageId, out MagicGeometry geometry) =>
        TryValidateMagic(datagram, datagram?.Length ?? 0, messageId, out geometry);

    /// <inheritdoc cref="TryValidateMagic(byte[], byte, out MagicGeometry)"/>
    public static bool TryValidateMagic(byte[] datagram, int byteLength, byte messageId, out MagicGeometry geometry)
    {
        geometry = HasMagicGeometry(messageId) ? GetMagicGeometry(messageId) : DefaultGeometry;
        if (!HasMagicGeometry(messageId))
        {
            return false;
        }
        int offset = geometry.Offset;
        int compared = geometry.BytesCompared;
        if (offset + 0x10 > byteLength || offset + 0x10 > datagram.Length)
        {
            return false;
        }
        if (geometry.ExpectedLength != 0 && geometry.ExpectedLength != byteLength)
        {
            return false;
        }
        for (int i = 0; i < compared; i++)
        {
            if (datagram[offset + i] != OfflineMagic[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// The inbound admission whitelist. Only these three MTA ids survive admission; everything else
    /// is silently dropped.
    /// </summary>
    public static bool IsWhitelisted(byte mtaId, byte[] payload, int bitLength) => mtaId switch
    {
        0x22 => true,
        0x6C => true,
        0x18 => bitLength >= 2 && payload.Length >= 2 && payload[1] is 0 or 1,
        _ => false,
    };

    /// <summary>
    /// The <c>mtaId 4 &lt;=&gt; 0x13</c> swap, applied before every gate and in both directions.
    /// </summary>
    public static byte SwapIds(byte mtaId) => mtaId switch
    {
        0x13 => 4,
        4 => 0x13,
        _ => mtaId,
    };

    /// <summary>
    /// The wire id for an outbound game packet: the <c>+99</c> offset plus the <c>4 &lt;=&gt; 0x13</c>
    /// swap, which applies in both directions. So Slipe's
    /// <c>PACKET_ID_PLAYER_JOINDATA</c> (mtaId 4) goes out as <c>0x76</c>, not <c>0x67</c>.
    /// </summary>
    public static byte ToWireId(byte mtaId) => (byte)(SwapIds(mtaId) + MtaIdToWireId);
}
