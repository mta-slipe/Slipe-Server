namespace SlipeServer.Net.Managed;

/// <summary>
/// Stage 0 — the connected <c>RID_CONNECTION_REQUEST</c> (4) / <c>RID_CONNECTION_REQUEST_ACCEPTED</c>
/// (0x0E) exchange.
///
/// After the offline open-connection exchange completes, the stock client sends id 4 as a reliable
/// packet inside an ordinary connected datagram and then retransmits it until it is answered.
///
/// The request is <c>[id][16-byte modified magic][16-byte client GUID][password string]</c>. The
/// magic is skipped, never compared. The password is compared against the server's configured
/// password; a 33-byte request carries an empty password, which matches a passwordless server.
/// </summary>
public static class ConnectedHandshake
{
    /// <summary>
    /// Offset of the client GUID inside a connected <c>RID_CONNECTION_REQUEST</c>: id (1) + magic (16).
    /// </summary>
    public const int RequestGuidOffset = 17;

    /// <summary>Length of the client GUID the handler reads, as four 32-bit little-endian words.</summary>
    public const int GuidLength = 16;

    /// <summary>
    /// Body size of the reply: 1 (id) + 4 (our view of the client's address) + 2 (its port) +
    /// 2 (system index) + 10 * 6 (our own address list) = 69 bytes.
    /// </summary>
    public const int ReplyLength = 69;

    /// <summary>Slots in the server's own address list.</summary>
    public const int ReplyAddressSlots = 10;

    /// <summary>
    /// The system index written into the reply: the index the server assigned to this peer. This
    /// port keeps no address table, so it always writes 0, the index of a freshly connected first
    /// peer. The alternative, 0xFFFF (the "not found" value), is avoided because some RakNet forks
    /// would use it to index their own remote-system array.
    /// </summary>
    public const ushort AssumedSystemIndex = 0;

    /// <summary>
    /// Reads the client GUID out of a connected <c>RID_CONNECTION_REQUEST</c> payload. The magic is
    /// not validated here because the server does not validate it either.
    /// </summary>
    public static bool TryReadConnectionRequest(byte[] payload, out byte[] clientGuid)
    {
        clientGuid = new byte[GuidLength];
        if (payload is null || payload.Length < RequestGuidOffset + GuidLength)
            return false;

        Array.Copy(payload, RequestGuidOffset, clientGuid, 0, GuidLength);
        return true;
    }

    /// <summary>
    /// Builds the 69-byte <c>RID_CONNECTION_REQUEST_ACCEPTED</c> body. <paramref name="clientBinaryAddress"/>
    /// is the peer identifier this port keys its sessions on (<c>(address dword &lt;&lt; 32) + port</c>), and
    /// <paramref name="serverAddresses"/> are this server's own endpoints, in the same encoding.
    ///
    /// An unpopulated list slot is serialized as address 0 / port 0, which complements to
    /// <c>255.255.255.255:0</c>; the client ignores entries whose port is 0.
    /// </summary>
    public static byte[] BuildConnectionRequestAccepted(
        ulong clientBinaryAddress,
        IReadOnlyList<ulong> serverAddresses,
        ushort systemIndex)
    {
        ArgumentNullException.ThrowIfNull(serverAddresses);

        var body = new byte[ReplyLength];
        int offset = 0;

        body[offset++] = MtaProtocol.Rid.ConnectionRequestAccepted;

        WriteAddress(body, ref offset, clientBinaryAddress);

        body[offset++] = (byte)systemIndex;
        body[offset++] = (byte)(systemIndex >> 8);

        for (int i = 0; i < ReplyAddressSlots; i++)
        {
            WriteAddress(body, ref offset, i < serverAddresses.Count ? serverAddresses[i] : 0UL);
        }

        return body;
    }

    /// <summary>
    /// One serialized <c>SystemAddress</c>: the address dword bitwise-complemented, then the port, both
    /// little-endian. Stock RakNet hides the address this way so routers do not rewrite it; it is not
    /// an MTA modification. The read path complements it back.
    /// </summary>
    private static void WriteAddress(byte[] buffer, ref int offset, ulong binaryAddress)
    {
        uint ipDword = (uint)(binaryAddress >> 32);
        ushort port = (ushort)(binaryAddress & 0xFFFF);
        uint hidden = ~ipDword;

        buffer[offset++] = (byte)hidden;
        buffer[offset++] = (byte)(hidden >> 8);
        buffer[offset++] = (byte)(hidden >> 16);
        buffer[offset++] = (byte)(hidden >> 24);
        buffer[offset++] = (byte)port;
        buffer[offset++] = (byte)(port >> 8);
    }
}
