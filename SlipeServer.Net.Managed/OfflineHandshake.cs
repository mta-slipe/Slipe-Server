namespace SlipeServer.Net.Managed;

/// <summary>Where an inbound datagram should be routed.</summary>
public enum OfflineRoute
{
    /// <summary>No magic match and no session record — dropped.</summary>
    Drop,

    /// <summary>Magic matched at the per-ID offset — dispatch as an offline (out-of-band) message.</summary>
    Offline,

    /// <summary>No magic match, but a session record exists — the RakNet reliability layer owns it.</summary>
    Connected,
}

/// <summary>
/// The offline (out-of-band) handshake, before any connected session exists.
///
/// The server answers three offline message ids:
///   * <c>RID_PING</c> (1)
///   * <c>RID_OPEN_CONNECTION_REQUEST</c> (9)
///   * MTA handshake 38 (0x26), subtype 1
///
/// The ids 10, 0x0f, 0x10, 0x12, 0x16, 0x17 and 0x5f are client-role messages that a server never
/// receives, so a server ignores them.
///
/// No encryption is involved: the open-connection reply carries no cipher configuration, so it is
/// exactly 33 bytes.
/// </summary>
public static class OfflineHandshake
{
    /// <summary>Offline-message version byte expected at <c>data[1]</c> of an open-connection request.</summary>
    public const byte OpenConnectionRequestVersion = 0x04;

    /// <summary>The server-role message ids this port answers. 0x0a is the reply, never an inbound case.</summary>
    public const byte CasePing = MtaProtocol.Rid.Ping;
    public const byte CaseOpenConnectionRequest = MtaProtocol.Rid.OpenConnectionRequest;
    public const byte CaseMtaHandshake38 = MtaProtocol.Rid.MtaHandshake38;

    /// <summary>
    /// The router: validates the magic at the per-message-ID offset and, when that fails, falls back
    /// to the connected reliability layer if and only if a session record already exists for the
    /// source address.
    /// </summary>
    /// <param name="datagram">Raw received bytes.</param>
    /// <param name="byteLength">Number of valid bytes.</param>
    /// <param name="hasSessionRecord">Whether a session record exists for the source address.</param>
    public static OfflineRoute Route(byte[] datagram, int byteLength, bool hasSessionRecord)
    {
        // Fewer than 3 bytes is dropped before anything else.
        if (datagram is null || byteLength < 3)
            return OfflineRoute.Drop;

        byte messageId = datagram[0];
        if (MtaProtocol.HasMagicGeometry(messageId) &&
            MtaProtocol.TryValidateMagic(datagram, byteLength, messageId, out _))
        {
            return OfflineRoute.Offline;
        }

        return hasSessionRecord ? OfflineRoute.Connected : OfflineRoute.Drop;
    }

    /// <summary>
    /// True when the message ID has a server-role offline handler. Other IDs either fall through to
    /// the connected path or are client-role messages.
    /// </summary>
    public static bool IsServerRoleCase(byte messageId) =>
        messageId is CasePing or CaseOpenConnectionRequest or CaseMtaHandshake38;

    // ---------------------------------------------------------------- inbound parsing

    /// <summary>
    /// Reads the 64-bit client time from an inbound <c>RID_PING</c>. The layout is
    /// <c>[0x01][client time : 8][magic : 16]</c>, 25 bytes, with only 15 magic bytes compared.
    /// </summary>
    public static bool TryReadPing(byte[] datagram, int byteLength, out ulong clientTime)
    {
        clientTime = 0;
        // 1 (id) + 8 (time) + 16 (magic) == 25.
        if (byteLength < 25)
            return false;

        // The time is byte-aligned, so it is just the eight little-endian bytes at offset 1.
        clientTime = ReadUInt64LittleEndian(datagram, 1);
        return true;
    }

    /// <summary>
    /// Reads the client GUID from an open-connection request:
    /// <c>[0x09][0x04][client GUID : 16][magic : 16]</c>.
    /// </summary>
    public static bool TryReadOpenConnectionRequest(
        byte[] datagram, int byteLength, out byte[] clientGuid, out byte versionByte)
    {
        clientGuid = new byte[16];
        versionByte = 0;

        if (byteLength < 34) // 1 + 1 + 16 + 16
            return false;

        versionByte = datagram[1];
        Array.Copy(datagram, 2, clientGuid, 0, 16);
        return true;
    }

    /// <summary>
    /// Reads the subtype of an MTA handshake 38 message and, for subtype 1, the 32-bit value to
    /// echo. The layout is <c>[0x26][magic : 16][subtype : 1][echo : 4]</c>.
    /// </summary>
    public static bool TryReadMtaHandshake38(
        byte[] datagram, int byteLength, out byte subtype, out uint echo)
    {
        subtype = 0;
        echo = 0;

        if (byteLength < 18) // 1 + 16 + 1
            return false;

        subtype = datagram[17];
        if (subtype == 1)
        {
            if (byteLength < 22)
                return false;
            echo = ReadUInt32LittleEndian(datagram, 18);
        }
        else if (subtype == 2)
        {
            if (byteLength < 26)
                return false;
        }

        return true;
    }

    // ---------------------------------------------------------------- reply builders

    /// <summary>
    /// Ping reply — <c>[0x1A][client time : 8][server GUID : 16][magic : 16]</c> = 41 bytes.
    /// <c>0x1A</c> is <c>RID_PONG</c> (26); the MTA <c>0x26</c> message is a different handshake.
    /// </summary>
    public static byte[] BuildPong(ulong clientTime, byte[] serverGuid)
    {
        var result = new byte[1 + 8 + 16 + 16];
        result[0] = MtaProtocol.Rid.Pong;
        WriteUInt64LittleEndian(result, 1, clientTime);
        Array.Copy(serverGuid, 0, result, 9, 16);
        Array.Copy(MtaProtocol.OfflineMagic, 0, result, 25, 16);
        return result;
    }

    /// <summary>
    /// Open-connection success reply — <c>[0x0A][magic : 16][server GUID : 16]</c> = 33 bytes.
    /// The reply is exactly 33 bytes because no encryption configuration is appended.
    /// </summary>
    public static byte[] BuildOpenConnectionReply(byte[] serverGuid)
    {
        var result = new byte[1 + 16 + 16];
        result[0] = MtaProtocol.Rid.OpenConnectionReply;
        Array.Copy(MtaProtocol.OfflineMagic, 0, result, 1, 16);
        Array.Copy(serverGuid, 0, result, 17, 16);
        return result;
    }

    /// <summary>
    /// Open-connection refusal reply. Two prefix bytes for the config-failed refusal, one for the
    /// no-free-slots / already-connected refusals:
    ///   * <c>0x5F</c> — <c>[0x5F][0x04][magic : 16][server GUID : 16]</c> = 34 bytes
    ///   * <c>0x10</c> / <c>0x12</c> — <c>[id][magic : 16][server GUID : 16]</c> = 33 bytes
    /// </summary>
    public static byte[] BuildRefusal(byte messageId, byte[] serverGuid)
    {
        bool doublePrefix = messageId == MtaProtocol.Rid.Refusal95;
        var result = new byte[(doublePrefix ? 2 : 1) + 16 + 16];
        result[0] = messageId;
        int offset = 1;
        if (doublePrefix)
        {
            result[1] = OpenConnectionRequestVersion;
            offset = 2;
        }

        Array.Copy(MtaProtocol.OfflineMagic, 0, result, offset, 16);
        Array.Copy(serverGuid, 0, result, offset + 16, 16);
        return result;
    }

    /// <summary>
    /// MTA handshake 38 subtype 1 reply —
    /// <c>[0x26][magic : 16][0x02][echoed : 4][server timestamp : 4]</c> = 26 bytes.
    /// </summary>
    public static byte[] BuildMtaHandshake38Reply(byte[] serverGuid, uint echo, uint serverTimestamp)
    {
        var result = new byte[1 + 16 + 1 + 4 + 4];
        result[0] = CaseMtaHandshake38;
        Array.Copy(MtaProtocol.OfflineMagic, 0, result, 1, 16);
        result[17] = 0x02;
        WriteUInt32LittleEndian(result, 18, echo);
        WriteUInt32LittleEndian(result, 22, serverTimestamp);
        return result;
    }

    // ---------------------------------------------------------------- helpers

    private static ulong ReadUInt64LittleEndian(byte[] buffer, int offset) =>
        buffer[offset]
        | ((ulong)buffer[offset + 1] << 8)
        | ((ulong)buffer[offset + 2] << 16)
        | ((ulong)buffer[offset + 3] << 24)
        | ((ulong)buffer[offset + 4] << 32)
        | ((ulong)buffer[offset + 5] << 40)
        | ((ulong)buffer[offset + 6] << 48)
        | ((ulong)buffer[offset + 7] << 56);

    private static uint ReadUInt32LittleEndian(byte[] buffer, int offset) =>
        buffer[offset]
        | ((uint)buffer[offset + 1] << 8)
        | ((uint)buffer[offset + 2] << 16)
        | ((uint)buffer[offset + 3] << 24);

    private static void WriteUInt32LittleEndian(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }

    private static void WriteUInt64LittleEndian(byte[] buffer, int offset, ulong value)
    {
        for (int i = 0; i < 8; i++)
            buffer[offset + i] = (byte)(value >> (i * 8));
    }
}
