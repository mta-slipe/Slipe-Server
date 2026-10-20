using System.Text;

namespace SlipeServer.Net.Managed;

/// <summary>
/// The inputs to the packet-2 (<c>PACKET_ID_SERVER_JOIN_COMPLETE</c>) payload.
/// </summary>
/// <remarks>
/// Every field is a server-side setting the native server writes into the packet. The defaults are
/// the values of a stock server with nothing configured.
/// </remarks>
public sealed class ServerJoinCompleteOptions
{
    /// <summary>The client serial recorded for the joining player.</summary>
    public byte[] ClientSerial { get; set; } = Array.Empty<byte>();

    /// <summary>The advertised bitstream version. Must be at least <c>0x66</c> for the full block.</summary>
    public ushort BitStreamVersion { get; set; } = 0x90;

    /// <summary>
    /// Low half of the 64-bit anti-cheat disable bitmap.
    /// </summary>
    /// <remarks>
    /// Bit <em>n</em> is set for every numeric entry of the server's <c>disableac</c> list. A stock
    /// server always merges in the default disabled list (checks 12, 14, 15, 16, 20, 22, 23, 28, 31 and
    /// 32–36), so it never sends zero here. The default is that projection for the low 32 bits —
    /// bits 12, 14, 15, 16, 20, 22, 23, 28 and 31 — giving the stock wire value <c>0x90D1D000</c>.
    /// </remarks>
    public uint AcDisableBitmapLow { get; set; } = 0x90D1D000;

    /// <summary>
    /// High half of the 64-bit anti-cheat disable bitmap: bits 32–36 of the same default list, i.e.
    /// <c>0x1F</c>.
    /// </summary>
    public uint AcDisableBitmapHigh { get; set; } = 0x0000001F;

    /// <summary>Three-bit image-mod category flag (bit0 peds, bit1 vehicles, bit2 world).</summary>
    /// <remarks>
    /// Bits are set only for categories named by the <c>allow_gta3_img_mods</c> server option. A stock
    /// server leaves it unset, so zero is the stock value.
    /// </remarks>
    public uint ImageModFlags { get; set; }

    /// <summary>
    /// The client-check bitmap. A stock server has all bits set (<c>-1</c>); a server that wants
    /// specific checks off clears bits. This port sends no anti-cheat challenges either way, so the
    /// field is informational to the client.
    /// </summary>
    public uint EnableClientChecks { get; set; } = 0xFFFFFFFF;

    /// <summary>The connection banner; truncated to 128 characters exactly as the native server does.</summary>
    public string ConnectionText { get; set; } = string.Empty;

    /// <summary>The sortable server version string.</summary>
    public string ServerVersionSortable { get; set; } = string.Empty;

    /// <summary>
    /// The adapter payload block. Its length framing is known, but the strings the native server
    /// formats into it are not, so this is caller-supplied content rather than a reproduction.
    /// </summary>
    public byte[] AdapterBlock { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Wire builders for the MTA game packets the server emits. Currently only packet 2.
/// </summary>
public static class MtaGamePackets
{
    /// <summary><c>PACKET_ID_SERVER_JOIN_COMPLETE</c>.</summary>
    public const byte ServerJoinCompleteMtaId = 2;

    /// <summary><c>PACKET_ID_PLAYER_JOIN</c> — what a RakNet <c>0x11</c> is turned into.</summary>
    public const byte PlayerJoinMtaId = 3;

    /// <summary>
    /// <c>PACKET_ID_PLAYER_JOINDATA</c> — the mod-level join request the client sends once the
    /// RakNet connection is up. Wire id <c>0x76</c>: ids 4 and <c>0x13</c> are swapped in both
    /// directions, so mtaId 4 travels as <c>0x13 + 99</c>.
    /// </summary>
    public const byte PlayerJoinDataMtaId = 4;

    /// <summary><c>PACKET_ID_PLAYER_QUIT</c> — what a RakNet <c>0x13</c> is turned into.</summary>
    public const byte PlayerQuitMtaId = 5;

    /// <summary>
    /// <c>PACKET_ID_PLAYER_TIMEOUT</c> — what a RakNet <c>0x14</c> is turned into; the native side
    /// logs "ConnectionLost" and raises this id.
    ///
    /// <para>
    /// A dead peer never sends <c>0x14</c>: RakNet's own update cycle fabricates the event locally
    /// when a peer's last datagram ages past the timeout, so this id is also what the port raises
    /// from <c>MtaNetWrapper.TickTimeouts</c>.
    /// </para>
    /// </summary>
    public const byte PlayerTimeoutMtaId = 6;

    /// <summary>
    /// <c>PACKET_ID_PLAYER_NO_SOCKET</c> — what RakNet <c>0x21</c> is turned into. This port never
    /// raises it: what makes the native server emit <c>0x66</c> is unknown, and guessing would be
    /// worse than the gap.
    /// </summary>
    public const byte PlayerNoSocketMtaId = 0x66;

    /// <summary>
    /// <c>PACKET_ID_PLAYER_LIST</c> — the roster broadcast, wire id <c>0x7C</c>. The client reads two
    /// strings before the just-joined bit, and both come from the anticheat block; see
    /// <see cref="PrependPlayerListAcBlock"/>.
    /// </summary>
    public const byte PlayerListMtaId = 25;

    /// <summary>
    /// Bitstream version at which the server starts writing the 16-bit registry value into the
    /// <c>PLAYER_LIST</c> anticheat block. Below it the value is omitted entirely, which changes the
    /// block's length. See <see cref="PrependPlayerListAcBlock"/>.
    /// </summary>
    public const ushort PlayerListAcBlockVersion = 0x3a;

    /// <summary>
    /// Bitstream version at which the server starts writing the same 16-bit registry value into
    /// <c>PACKET_ID_PLAYER_QUIT</c> (mtaId <c>5</c>, wire <c>0x68</c>).
    /// <para>
    /// The quit packet uses a <b>different</b> threshold from the roster (versions <c>&gt;= 0x4a</c>)
    /// and tests only the packet's own version, never a per-player one. It carries no anticheat
    /// strings; the value alone is the whole net-layer prefix. See
    /// <see cref="PrependPlayerQuitRegistryValue"/>.
    /// </para>
    /// </summary>
    public const ushort PlayerQuitRegistryVersion = 0x4a;

    /// <summary>
    /// The block's first string, the client-check text, for a stock configuration: <c>"0"</c>.
    /// <para>
    /// The native server builds this string from the decimal client-check bitmap, then appends
    /// <c>","</c> plus one entry per element of the disabled-check set, so <b>it can never be
    /// empty</b>. The sibling enable-check string is built the same way from its own set.
    /// </para>
    /// <para>
    /// The combo-check list (<c>"12=&amp;14=…"</c>) never reaches either string; it is server-side
    /// tracking only, which is why the block carries no per-check entries.
    /// </para>
    /// <para>
    /// The reference wrapper configures the checks with an empty disabled set, an empty enable set,
    /// a client-check bitmap of 0 and anticheat reporting not hidden. Both append loops therefore run
    /// zero times, the seed is <c>"0"</c>, and the real-data branch is taken: the wire carries exactly
    /// <c>"0"</c> and <c>""</c>.
    /// </para>
    /// </summary>
    public const string DefaultAcInfoText = "0";

    /// <summary>The same value as raw bytes; the strings travel as raw bytes, not characters.</summary>
    public static readonly byte[] DefaultAcInfoBytes = "0"u8.ToArray();

    /// <summary>
    /// The block's second string, the enable-check text: empty, because the enable set is empty and
    /// the join of an empty set is the empty string.
    /// </summary>
    public const string DefaultSdInfoText = "";

    /// <summary><see cref="DefaultSdInfoText"/> as raw bytes.</summary>
    public static readonly byte[] DefaultSdInfoBytes = [];

    /// <summary>Maximum connection-text length the client clamps to.</summary>
    public const int MaxConnectionTextLength = 128;

    /// <summary>Bitstream version at which the high bitmap half and image-mod flags join the leading block.</summary>
    public const ushort LegacyLeadingBlockVersion = 0x60;

    /// <summary>Bitstream version at which the per-byte-shifted client serial joins the leading block.</summary>
    public const ushort ClientSerialVersion = 0x66;

    /// <summary>Nickname width the client zero-pads to.</summary>
    public const int MaxPlayerNickLength = 22;

    /// <summary>
    /// Community-data width the client zero-pads to. This is <b>not</b> the client serial: the client
    /// writes an empty community string there. See <see cref="ClientSerialLength"/>.
    /// </summary>
    public const int MaxPlayerSerialLength = 32;

    /// <summary>
    /// Client serial width: 32 ASCII hex characters, i.e. a 16-byte digest rendered as text.
    /// </summary>
    public const int ClientSerialLength = 32;

    /// <summary>
    /// <c>0xD1</c> is the XOR key the net layer applies to the client serial before it reaches the wire. The
    /// transform is per byte and also mixes in the byte index and its low three bits; see
    /// <see cref="TryReadPlayerJoinDataSerial"/>.
    /// </summary>
    public const byte ClientSerialObfuscationKey = 0xD1;

    /// <summary>Longest plausible player-version string ("1.6.0-9.24149.0" is 15 bytes).</summary>
    public const int MaxPlayerVersionLength = 64;

    /// <summary>
    /// Bits the join body occupies after the player-version string: the update flag (1), the game version
    /// (8), the nickname (<see cref="MaxPlayerNickLength"/> × 8), the MD5 password (16 × 8) and the
    /// community block (<see cref="MaxPlayerSerialLength"/> × 8).
    /// </summary>
    private const int PlayerJoinDataTailBits =
        1 + 8 + (MaxPlayerNickLength * 8) + (16 * 8) + (MaxPlayerSerialLength * 8);

    /// <summary>Shortest plausible join body: three version words, a length-prefixed version, then the tail.</summary>
    public const int MinPlayerJoinDataBlockBytes =
        6 + 2 + 1 + ((PlayerJoinDataTailBits + 7) >> 3);

    /// <summary>Offset of the obfuscated client serial inside the net layer's prefix: past the u16 version.</summary>
    private const int PlayerJoinDataSerialOffset = 2;

    /// <summary>
    /// Locates the <c>PACKET_ID_PLAYER_JOINDATA</c> mod-level body inside a received payload, returning
    /// its byte offset or <c>-1</c> when the payload holds no plausible body.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The client writes only the join fields — 95 bytes for a stock 1.6 client — and hands them to
    /// the net layer, which puts an opaque, high-entropy prefix in front of that body (775 bytes
    /// observed; it measures about 7.7 bits/byte of entropy and contains no ASCII at any bit shift).
    /// The mod-level reader cannot see that prefix, so the net layer strips it, and its exact length
    /// is not pinned down.
    /// </para>
    /// <para>
    /// Rather than assume a length, this locates the body from its own self-describing shape:
    /// three non-zero version words, a length-prefixed printable version string, then exactly
    /// <see cref="PlayerJoinDataTailBits"/> bits of flag, game version, zero-padded nickname, password
    /// and serial, all of which must fit inside the payload. A payload that is already a bare body
    /// validates at offset 0, so callers slice only when this returns a positive offset.
    /// </para>
    /// </remarks>
    /// <param name="payload">The wire payload with the wire id already removed.</param>
    /// <returns>The body offset, or <c>-1</c> when no plausible body is present.</returns>
    public static int FindPlayerJoinDataBlockOffset(ReadOnlySpan<byte> payload)
    {
        for (int offset = 0; offset + MinPlayerJoinDataBlockBytes <= payload.Length; offset++)
        {
            if (IsPlayerJoinDataBlock(payload[offset..]))
                return offset;
        }

        return -1;
    }

    /// <summary>
    /// Returns the <c>PACKET_ID_PLAYER_JOINDATA</c> body, dropping the net layer's opaque prefix when
    /// one is present. Returns <paramref name="payload"/> unchanged when no body is located.
    /// </summary>
    /// <param name="payload">The wire payload with the wire id already removed.</param>
    public static byte[] ExtractPlayerJoinDataBlock(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        int offset = FindPlayerJoinDataBlockOffset(payload);
        if (offset <= 0)
            return payload;

        var body = new byte[payload.Length - offset];
        Array.Copy(payload, offset, body, 0, body.Length);
        return body;
    }

    /// <summary>
    /// Reads the client serial out of the net layer's prefix, where it travels XOR-obfuscated as
    /// <see cref="ClientSerialLength"/> ASCII hex characters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The serial is <b>not</b> in the mod-level join body: the client writes an empty community
    /// string into the body's trailing 32 bytes, and the server's join reader never looks there. It
    /// instead takes a 16-bit bitstream version and then 32 bytes that it de-obfuscates into the
    /// player's serial field, which packet 2 later emits. Each byte is XORed with <c>0xD1</c> and
    /// with its index, and bit <c>(index &amp; 7)</c> is complemented.
    /// </para>
    /// <para>
    /// The transform is its own inverse, so the same expression both encodes and decodes. Requiring
    /// every de-obfuscated byte to be an ASCII hex digit is what makes the fixed
    /// <c>[2, 34)</c> window safe: a window of anything else passes with probability 16^-32.
    /// </para>
    /// </remarks>
    /// <param name="payload">
    /// The wire payload with the wire id already removed, i.e. <b>before</b>
    /// <see cref="ExtractPlayerJoinDataBlock"/> drops the net-layer prefix.
    /// </param>
    /// <param name="serial">The 32 ASCII hex characters of the client serial.</param>
    public static bool TryReadPlayerJoinDataSerial(ReadOnlySpan<byte> payload, out byte[] serial)
    {
        serial = Array.Empty<byte>();

        if (payload.Length < PlayerJoinDataSerialOffset + ClientSerialLength)
            return false;

        // The prefix opens with the same 16-bit bitstream version the mod-level body carries. It is
        // not the serial length, so it is only sanity-checked.
        int p = 0;
        if (!TryReadUInt16(payload, ref p, out ushort bitStreamVersion) || bitStreamVersion == 0)
            return false;

        var decoded = new byte[ClientSerialLength];
        for (int i = 0; i < ClientSerialLength; i++)
        {
            byte value = (byte)(i ^ payload[PlayerJoinDataSerialOffset + i] ^
                ClientSerialObfuscationKey ^ (1 << (i & 7)));

            if (!IsAsciiHexDigit(value))
                return false;

            decoded[i] = value;
        }

        serial = decoded;
        return true;
    }

    private static bool IsAsciiHexDigit(byte value) =>
        (value >= (byte)'0' && value <= (byte)'9') ||
        (value >= (byte)'A' && value <= (byte)'F') ||
        (value >= (byte)'a' && value <= (byte)'f');

    /// <summary>
    /// Reads the free-form player-version string out of a join body, e.g. <c>1.6.0-9.24149.0</c>.
    /// </summary>
    /// <remarks>
    /// This walks the same ladder as <see cref="IsPlayerJoinDataBlock"/> so a positive result
    /// implies the same domain checks. The serial does not live here; read it with
    /// <see cref="TryReadPlayerJoinDataSerial"/> from the unstripped payload.
    /// </remarks>
    /// <param name="block">A candidate join body, normally <see cref="ExtractPlayerJoinDataBlock"/>'s result.</param>
    /// <param name="playerVersion">The player-version string.</param>
    public static bool TryReadPlayerJoinDataVersion(ReadOnlySpan<byte> block, out string playerVersion)
    {
        playerVersion = string.Empty;

        int p = 0;
        if (!TryReadUInt16(block, ref p, out ushort netVersion) ||
            !TryReadUInt16(block, ref p, out _) ||
            !TryReadUInt16(block, ref p, out ushort bitStreamVersion))
            return false;

        if (netVersion == 0 || bitStreamVersion == 0)
            return false;

        if (!TryReadUInt16(block, ref p, out ushort versionLength))
            return false;

        if (versionLength == 0 || versionLength > MaxPlayerVersionLength)
            return false;

        if (p + versionLength > block.Length)
            return false;

        for (int i = 0; i < versionLength; i++)
        {
            byte c = block[p + i];
            if (c < 0x20 || c > 0x7E)
                return false;
        }

        playerVersion = Encoding.UTF8.GetString(block.Slice(p, versionLength));
        p += versionLength;

        long bit = (long)p * 8;
        if (!TryReadBits(block, ref bit, 1, out _) ||
            !TryReadBits(block, ref bit, 8, out uint gameVersion))
            return false;

        if (gameVersion == 0 || gameVersion > 64)
            return false;

        bool sawNickChar = false;
        bool nickEnded = false;
        for (int i = 0; i < MaxPlayerNickLength; i++)
        {
            if (!TryReadBits(block, ref bit, 8, out uint nickByte))
                return false;

            byte c = (byte)nickByte;
            if (nickEnded)
            {
                if (c != 0)
                    return false;
                continue;
            }

            if (c == 0)
            {
                nickEnded = true;
                continue;
            }

            if (c < 0x20 || c > 0x7E)
                return false;

            sawNickChar = true;
        }

        if (!sawNickChar)
            return false;

        // The optional-update flag leaves every later field off a byte boundary, so the password is
        // bit-addressed, MSB first.
        if (!TryReadBits(block, ref bit, 16 * 8, out _))
            return false;

        return true;
    }

    /// <summary>
    /// Validates the client's join-body layout at the start of
    /// <paramref name="block"/>. Every field is checked against its domain, not merely read.
    /// </summary>
    private static bool IsPlayerJoinDataBlock(ReadOnlySpan<byte> block)
    {
        int p = 0;

        if (!TryReadUInt16(block, ref p, out ushort netVersion) ||
            !TryReadUInt16(block, ref p, out _) ||
            !TryReadUInt16(block, ref p, out ushort bitStreamVersion))
        {
            return false;
        }

        if (netVersion == 0 || bitStreamVersion == 0)
            return false;

        if (!TryReadUInt16(block, ref p, out ushort versionLength))
            return false;

        if (versionLength == 0 || versionLength > MaxPlayerVersionLength)
            return false;

        if (p + versionLength > block.Length)
            return false;

        for (int i = 0; i < versionLength; i++)
        {
            byte c = block[p + i];
            if (c < 0x20 || c > 0x7E)
                return false;
        }

        p += versionLength;

        // From here on the body is bit-oriented: the flag breaks byte alignment, so the game version
        // and the nickname that follow are read MSB-first across byte boundaries.
        long bit = (long)p * 8;

        if (!TryReadBits(block, ref bit, 1, out _))
            return false;

        if (!TryReadBits(block, ref bit, 8, out uint gameVersion))
            return false;

        // GTA:SA is 11 and the three GTA III-era titles are 1..3; anything outside that is noise.
        if (gameVersion == 0 || gameVersion > 64)
            return false;

        bool sawNickChar = false;
        bool nickEnded = false;

        for (int i = 0; i < MaxPlayerNickLength; i++)
        {
            if (!TryReadBits(block, ref bit, 8, out uint c))
                return false;

            if (nickEnded)
            {
                if (c != 0)
                    return false;

                continue;
            }

            if (c == 0)
            {
                nickEnded = true;
                continue;
            }

            if (c < 0x20 || c > 0x7E)
                return false;

            sawNickChar = true;
        }

        if (!sawNickChar)
            return false;

        return bit + ((16 + MaxPlayerSerialLength) * 8) <= (long)block.Length * 8;
    }

    private static bool TryReadUInt16(ReadOnlySpan<byte> block, ref int offset, out ushort value)
    {
        if (offset + 2 > block.Length)
        {
            value = 0;
            return false;
        }

        value = (ushort)(block[offset] | (block[offset + 1] << 8));
        offset += 2;
        return true;
    }

    private static bool TryReadBits(ReadOnlySpan<byte> block, ref long bitOffset, int bitCount, out uint value)
    {
        value = 0;

        if (bitOffset + bitCount > (long)block.Length * 8)
            return false;

        for (int i = 0; i < bitCount; i++)
        {
            int current = block[(int)(bitOffset >> 3)];
            uint currentBit = (uint)((current >> (7 - (int)(bitOffset & 7))) & 1);
            value = (value << 1) | currentBit;
            bitOffset++;
        }

        return true;
    }

    /// <summary>
    /// Prefixes a <c>PLAYER_LIST</c> body with the net-layer anticheat block that the server writes
    /// ahead of the mod-level payload and that the client's net layer strips before the game layer
    /// sees the stream.
    /// </summary>
    /// <remarks>
    /// Write order:
    /// <list type="number">
    /// <item>a 16-bit registry value, written only when either bitstream version is at least
    /// <see cref="PlayerListAcBlockVersion"/> (zero bits otherwise);</item>
    /// <item>a 16-bit length plus that many bytes of the client-check text;</item>
    /// <item>a 16-bit length plus that many bytes of the disabled-check map rendering.</item>
    /// </list>
    /// <para>
    /// <b>The gate is an OR of two versions that hold the same value.</b> The server tests both the
    /// outgoing packet's version and the player's version. The packet's version is a snapshot of the
    /// player's, so the two agree in practice.
    /// </para>
    /// <para>
    /// <b>The gate and the hide are independent.</b> The registry value is written before the hide
    /// flag is considered, so hiding anticheat reporting does not shorten the block by two bytes. When
    /// <paramref name="hideAnticheat"/> is set, both strings keep their length field and are written
    /// empty, so the block is never shorter than four bytes.
    /// </para>
    /// <para>
    /// The 16-bit fields are little-endian. The two payloads are raw byte strings — the server writes
    /// the bytes of a <c>std::string</c>, not characters through a code page — so they are taken as
    /// <see cref="byte"/> arrays and never re-encoded.
    /// </para>
    /// <para>
    /// The client's net layer consumes the leading 16-bit value, so the two length-prefixed strings are
    /// the first things the game layer reads: the client-check text, then the disabled-check text.
    /// The just-joined bit it reads next is not a separate field: the mod-level payload's own leading
    /// bit (the show-in-chat flag) supplies it, and the entries follow immediately.
    /// </para>
    /// <para>
    /// <b>The block is 40 bits for the stock configuration:</b> <c>u16 1</c> + <c>"0"</c> +
    /// <c>u16 0</c>. A server whose check configuration was never applied sends a 32-bit block
    /// instead (<c>u16 0</c> + <c>u16 0</c>, i.e. the check text unwritten); a configured server
    /// cannot produce that because its check text is never empty. This port emits the configured
    /// form, which is the one observed to work with real clients.
    /// </para>
    /// <para>
    /// Do <b>not</b> add a further empty-string envelope on top of this block. Doing so costs 16
    /// bits: the just-joined bit reads as <c>false</c>, the entry loop starts 16 bits early, and the
    /// first nickname length it sees is zero, which the client rejects as a protocol error. A
    /// zero-entry list hides the misalignment because the loop never runs.
    /// </para>
    /// <para>
    /// <b><paramref name="registryValue"/> is a player count:</b> the number of player records the
    /// server's address-keyed player registry holds, clamped to <c>0xFFFF</c>. The clamp caps the
    /// field; it does not make it a non-count. Callers pass the live session count.
    /// </para>
    /// <para>
    /// The native registry has no observed erase path (an existing player object is reused and
    /// re-initialised), so it can in principle report more records than live players if an address
    /// slot is recycled. On the measured join-time path the two agree.
    /// </para>
    /// </remarks>
    public static byte[] PrependPlayerListAcBlock(
        byte[] body,
        ushort bitStreamVersion,
        ushort playerBitStreamVersion,
        bool hideAnticheat = false,
        ushort registryValue = 0,
        byte[]? acInfo = null,
        byte[]? sdInfo = null,
        bool? justJoined = null)
    {
        ArgumentNullException.ThrowIfNull(body);

        byte[] checks = acInfo ?? DefaultAcInfoBytes;
        byte[] maps = sdInfo ?? DefaultSdInfoBytes;

        if (hideAnticheat)
        {
            checks = Array.Empty<byte>();
            maps = Array.Empty<byte>();
        }

        if (checks.Length > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(acInfo), checks.Length, "The block's length field is 16 bits.");
        }

        if (maps.Length > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sdInfo), maps.Length, "The block's length field is 16 bits.");
        }

        // A client at or above the gate version parses the registry u16, so it must be written when
        // either version reaches it; omitting it misaligns the rest of the body.
        bool writeRegistryValue =
            bitStreamVersion >= PlayerListAcBlockVersion || playerBitStreamVersion >= PlayerListAcBlockVersion;

        int prefix = (writeRegistryValue ? 2 : 0) + 2 + checks.Length + 2 + maps.Length;

        var framed = new byte[body.Length + prefix];
        int offset = 0;

        if (writeRegistryValue)
        {
            framed[offset++] = (byte)registryValue;
            framed[offset++] = (byte)(registryValue >> 8);
        }

        framed[offset++] = (byte)checks.Length;
        framed[offset++] = (byte)(checks.Length >> 8);
        Array.Copy(checks, 0, framed, offset, checks.Length);
        offset += checks.Length;

        framed[offset++] = (byte)maps.Length;
        framed[offset++] = (byte)(maps.Length >> 8);
        Array.Copy(maps, 0, framed, offset, maps.Length);
        offset += maps.Length;

        Array.Copy(body, 0, framed, offset, body.Length);

        // The just-joined flag is the mod-level payload's own first bit. When it is false the
        // client treats the two strings above as anticheat info and prints it. Overriding it changes
        // that one bit only — no entry shifts, because every entry starts after it.
        if (justJoined is bool flag && framed.Length > offset)
        {
            framed[offset] = flag
                ? (byte)(framed[offset] | 0x80)
                : (byte)(framed[offset] & 0x7F);
        }

        return framed;
    }

    /// <summary>
    /// Prepends the version-gated registry <c>u16</c> that <c>PACKET_ID_PLAYER_QUIT</c> carries
    /// ahead of its mod-level body on a modern bitstream.
    /// </summary>
    /// <param name="body">The mod-level body, already serialised.</param>
    /// <param name="bitStreamVersion">The packet view's version — the only version the quit arm reads.</param>
    /// <param name="registryValue">The same value the roster's anticheat block carries.</param>
    /// <returns>The framed body, ready for the wire id to be prepended.</returns>
    /// <remarks>
    /// <para>
    /// On a modern bitstream (<c>&gt;= 0x4a</c>) the server writes a single 16-bit value before the
    /// mod-level body. It reads no per-player version and writes no anticheat strings: the value alone
    /// is the whole net-layer prefix. See <see cref="PlayerQuitRegistryVersion"/>.
    /// </para>
    /// <para>
    /// Omitting it makes the client read the mod-level body two bytes early, so every field after it
    /// lands at the wrong offset.
    /// </para>
    /// </remarks>
    public static byte[] PrependPlayerQuitRegistryValue(
        byte[] body, ushort bitStreamVersion, ushort registryValue)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (bitStreamVersion < PlayerQuitRegistryVersion)
        {
            return (byte[])body.Clone();
        }

        var framed = new byte[body.Length + 2];
        framed[0] = (byte)registryValue;
        framed[1] = (byte)(registryValue >> 8);
        Array.Copy(body, 0, framed, 2, body.Length);
        return framed;
    }

    /// <summary>
    /// Builds the packet-2 payload: the version-gated leading block, then the two-string mod-level body.
    /// The returned array is the body only — the wire id is prepended by the transport.
    /// </summary>
    public static byte[] BuildServerJoinComplete(ServerJoinCompleteOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var bs = new BitStream(256);

        // Leading block: the always-present low bitmap half.
        bs.Write(options.AcDisableBitmapLow);

        // The high bitmap half and image-mod flags exist only for version >= 0x60.
        if (options.BitStreamVersion >= LegacyLeadingBlockVersion)
        {
            bs.Write(options.AcDisableBitmapHigh);
            bs.Write(options.ImageModFlags);
        }

        // The serial is stored on the player record with every byte already shifted by one.
        if (options.BitStreamVersion >= ClientSerialVersion)
        {
            var serial = options.ClientSerial ?? Array.Empty<byte>();
            bs.Write((ushort)serial.Length);
            var shifted = new byte[serial.Length];
            for (int i = 0; i < serial.Length; i++)
                shifted[i] = (byte)(serial[i] + 1);
            bs.Write(shifted);
        }

        // The adapter block: 16-bit length, or a 0xFFFF sentinel plus a 32-bit length.
        var block = options.AdapterBlock ?? Array.Empty<byte>();
        if (block.Length < 0xFFFF)
        {
            bs.Write((ushort)block.Length);
            if (block.Length > 0)
                bs.Write(block);
        }
        else
        {
            bs.Write((ushort)0xFFFF);
            bs.Write((uint)block.Length);
            bs.Write(block);
        }

        // The client-check bitmap is always present and precedes the mod-level body.
        bs.Write(options.EnableClientChecks);

        // The mod-level body: connection text, then the sortable server version.
        string text = options.ConnectionText ?? string.Empty;
        if (text.Length > MaxConnectionTextLength)
            text = text.Substring(0, MaxConnectionTextLength);
        bs.WriteString(text);
        bs.WriteString(options.ServerVersionSortable ?? string.Empty);

        // The native server may append an optional trailer after the body; this port never emits it.
        return bs.ToArray();
    }

    /// <summary>
    /// Reads the mod-level body of packet 2 — the two length-prefixed strings (connection text, then
    /// server version) that both the native packet and Slipe's <c>JoinCompletePacket</c> write.
    /// </summary>
    /// <remarks>
    /// The native server writes the leading block and then appends the packet's own body bytes.
    /// Slipe's packet 2 carries only that body, so the transport has to prepend the rest. This is the
    /// inverse of the tail of <see cref="BuildServerJoinComplete"/>.
    /// </remarks>
    public static bool TryReadJoinCompleteBody(
        byte[] data,
        out string connectionText,
        out string serverVersion)
    {
        connectionText = string.Empty;
        serverVersion = string.Empty;

        if (data is null)
            return false;

        int offset = 0;
        if (!TryReadString(data, ref offset, out connectionText))
            return false;

        return TryReadString(data, ref offset, out serverVersion);
    }

    private static bool TryReadString(byte[] data, ref int offset, out string value)
    {
        value = string.Empty;

        if (offset + 2 > data.Length)
            return false;

        int length = data[offset] | (data[offset + 1] << 8);
        offset += 2;

        if (length == 0)
            return true;

        if (offset + length > data.Length)
            return false;

        value = Encoding.UTF8.GetString(data, offset, length);
        offset += length;
        return true;
    }
}
