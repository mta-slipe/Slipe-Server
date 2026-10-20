using System.Text;
using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the packet-2 (<c>SERVER_JOIN_COMPLETE</c>) layout from findings/transport-core.md
/// §20.8–§20.13, including the two version gates at 0x60 and 0x66.
/// </summary>
public class MtaGamePacketsTests
{
    private static ServerJoinCompleteOptions Bare(ushort version) => new()
    {
        BitStreamVersion = version,
        ConnectionText = string.Empty,
        ServerVersionSortable = string.Empty,
    };

    [Fact]
    public void WireIdForPacket2_Is0x65()
    {
        Assert.Equal(2, MtaGamePackets.ServerJoinCompleteMtaId);
        Assert.Equal((byte)0x65, (byte)(MtaGamePackets.ServerJoinCompleteMtaId + MtaProtocol.MtaIdToWireId));
    }

    [Theory]
    // version, expected total payload bytes for otherwise-empty options. The two empty strings add a
    // constant 4 bytes (two 16-bit zero lengths), so the leading block itself is 4 bytes shorter.
    [InlineData(0x5F, 112)]   // +0x28, adapter len, +0x1888, two empty strings
    [InlineData(0x60, 176)]   // adds +0x2c and +0x30
    [InlineData(0x66, 192)]   // adds the 16-bit serial length
    [InlineData(0x90, 192)]
    public void PayloadLength_IsVersionGated(ushort version, int expectedBits)
    {
        var payload = MtaGamePackets.BuildServerJoinComplete(Bare(version));
        Assert.Equal(expectedBits / 8, payload.Length);
    }

    [Theory]
    [InlineData(0x5F, 10)]
    [InlineData(0x60, 18)]
    [InlineData(0x66, 20)]
    [InlineData(0x90, 20)]
    public void LeadingBlock_IsVersionGated(ushort version, int leadingBytes)
    {
        // The leading block ends where the first string's 16-bit length prefix begins.
        var payload = MtaGamePackets.BuildServerJoinComplete(Bare(version));
        Assert.Equal(new byte[] { 0x00, 0x00 }, payload[leadingBytes..(leadingBytes + 2)]);
        Assert.Equal(leadingBytes + 4, payload.Length);
    }

    [Fact]
    public void LeadingBlock_FieldsAppearInTheDocumentedOrder()
    {
        var payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            AcDisableBitmapLow = 0x11223344,
            AcDisableBitmapHigh = 0x55667788,
            ImageModFlags = 0x0000000A,
            EnableClientChecks = 0xDEADBEEF,
            ConnectionText = string.Empty,
            ServerVersionSortable = string.Empty,
        });

        Assert.Equal(24, payload.Length);

        // +0x28, then +0x2c, then +0x30, then the 16-bit serial length (0), then the 16-bit
        // adapter length (0), then +0x1888.
        Assert.Equal(new byte[] { 0x44, 0x33, 0x22, 0x11 }, payload[0..4]);
        Assert.Equal(new byte[] { 0x88, 0x77, 0x66, 0x55 }, payload[4..8]);
        Assert.Equal(new byte[] { 0x0A, 0x00, 0x00, 0x00 }, payload[8..12]);
        Assert.Equal(new byte[] { 0x00, 0x00 }, payload[12..14]);
        Assert.Equal(new byte[] { 0x00, 0x00 }, payload[14..16]);
        Assert.Equal(new byte[] { 0xEF, 0xBE, 0xAD, 0xDE }, payload[16..20]);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00 }, payload[20..24]);
    }

    [Fact]
    public void EnableClientChecks_DefaultsToAllBitsSet()
    {
        // CNetServer+0x1888 (iEnableClientChecks) is initialised to -1 by the native constructor, so a
        // default-constructed options object must put FF FF FF FF at row 7 — byte offset 16 of the
        // leading block. This is the field a real-client capture cannot distinguish from a bug: an
        // all-zero row 7 looks identical to a builder that skipped the row.
        Assert.Equal(0xFFFFFFFFu, new ServerJoinCompleteOptions().EnableClientChecks);

        var payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            ConnectionText = string.Empty,
            ServerVersionSortable = string.Empty,
        });

        Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, payload[16..20]);
    }

    [Fact]
    public void AcDisableBitmap_DefaultsToTheStockDefaultDisabledList()
    {
        // SetChecks resets +0x28/+0x2c and then sets bit n for each numeric entry of the merged
        // disableac list. CMainConfig always merges defaultDisabledList[] =
        // {"12","14","15","16","20","22","23","28","31","32","33","34","35","36"}, so a stock server
        // never sends zero here. Locking the projection keeps the default from drifting back to zero,
        // which would tell a client to run checks MTA normally disables.
        var options = new ServerJoinCompleteOptions();
        Assert.Equal(0x90D1D000u, options.AcDisableBitmapLow);
        Assert.Equal(0x0000001Fu, options.AcDisableBitmapHigh);

        var expected = new uint[] { 12, 14, 15, 16, 20, 22, 23, 28, 31, 32, 33, 34, 35, 36 };
        ulong bitmap = options.AcDisableBitmapLow | ((ulong)options.AcDisableBitmapHigh << 32);
        foreach (var id in expected)
            Assert.True((bitmap & (1UL << (int)id)) != 0, $"bit {id} should be set");

        // allow_gta3_img_mods is unset in a stock server, so the image-mod category stays zero.
        Assert.Equal(0u, options.ImageModFlags);
    }

    [Fact]
    public void ClientSerial_IsShiftedUpByOnePerByte()
    {
        var payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            ClientSerial = new byte[] { 0x01, 0x02, 0x00, 0xFF },
            ConnectionText = string.Empty,
            ServerVersionSortable = string.Empty,
        });

        // 4 bytes of serial replace the empty serial: 192 + 32 = 224 bits = 28 bytes.
        Assert.Equal(28, payload.Length);
        Assert.Equal(new byte[] { 0x04, 0x00 }, payload[12..14]);
        Assert.Equal(new byte[] { 0x02, 0x03, 0x01, 0x00 }, payload[14..18]);
    }

    [Fact]
    public void AdapterBlock_UsesTheUshortLengthBelow0xFFFF()
    {
        var block = new byte[] { 0xA1, 0xA2, 0xA3 };
        var payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            AdapterBlock = block,
            ConnectionText = string.Empty,
            ServerVersionSortable = string.Empty,
        });

        Assert.Equal(192 / 8 + block.Length, payload.Length);
        Assert.Equal(new byte[] { 0x03, 0x00 }, payload[14..16]);
        Assert.Equal(block, payload[16..19]);
    }

    [Fact]
    public void ConnectionText_IsClampedTo128Characters()
    {
        var longText = new string('X', 200);
        var payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            ConnectionText = longText,
            ServerVersionSortable = string.Empty,
        });

        // 20 bytes of fixed block (row 2 + rows 3/4 + the 16-bit empty serial length + the 16-bit empty
        // adapter length + row 7), then 16-bit length + 128 chars, then the 16-bit empty sortable version.
        Assert.Equal(20 + 2 + 128 + 2, payload.Length);
        Assert.Equal(new byte[] { 0x80, 0x00 }, payload[20..22]);
        Assert.All(payload[22..150], b => Assert.Equal((byte)'X', b));
    }

    [Fact]
    public void StringsAreWrittenInTheDocumentedOrder()
    {
        var payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            ConnectionText = "AB",
            ServerVersionSortable = "CD",
        });

        Assert.Equal(20 + 4 + 4, payload.Length);
        Assert.Equal(new byte[] { 0x02, 0x00, (byte)'A', (byte)'B' }, payload[20..24]);
        Assert.Equal(new byte[] { 0x02, 0x00, (byte)'C', (byte)'D' }, payload[24..28]);
    }

    /// <summary>
    /// Slipe's <c>JoinCompletePacket.Write()</c> is exactly the two length-prefixed strings of
    /// <c>CPlayerConnectCompletePacket::Write()</c> — the same body row 8 of the native payload
    /// carries. This is the loopback byte string from a real run.
    /// </summary>
    private static byte[] SlipeJoinCompleteBody() => new byte[]
    {
        0x1D, 0x00,
        (byte)'M', (byte)'T', (byte)'A', (byte)':', (byte)' ', (byte)'S', (byte)'a', (byte)'n',
        (byte)' ', (byte)'A', (byte)'n', (byte)'d', (byte)'r', (byte)'e', (byte)'a', (byte)'s',
        (byte)' ', (byte)'[', (byte)'M', (byte)'t', (byte)'a', (byte)'N', (byte)'e', (byte)'t',
        (byte)' ', (byte)'P', (byte)'O', (byte)'C', (byte)']',
        0x0B, 0x00,
        (byte)'1', (byte)'.', (byte)'6', (byte)'.', (byte)'0', (byte)'-', (byte)'9',
        (byte)'.', (byte)'0', (byte)'.', (byte)'0',
    };

    [Fact]
    public void TryReadJoinCompleteBody_ReadsTheSlipeBody()
    {
        Assert.True(MtaGamePackets.TryReadJoinCompleteBody(
            SlipeJoinCompleteBody(), out string connectionText, out string serverVersion));

        Assert.Equal("MTA: San Andreas [MtaNet POC]", connectionText);
        Assert.Equal("1.6.0-9.0.0", serverVersion);
    }

    [Fact]
    public void TryReadJoinCompleteBody_RejectsATruncatedSecondString()
    {
        var body = SlipeJoinCompleteBody();
        Assert.False(MtaGamePackets.TryReadJoinCompleteBody(
            body[..^3], out _, out _));
    }

    [Fact]
    public void TryReadJoinCompleteBody_RejectsAnEmptyBody()
    {
        Assert.False(MtaGamePackets.TryReadJoinCompleteBody(Array.Empty<byte>(), out _, out _));
    }

    /// <summary>
    /// Re-framing a Slipe body must reproduce the native payload exactly: the version-gated leading
    /// block followed by the untouched mod-level bytes.
    /// </summary>
    [Fact]
    public void ReframingTheSlipeBody_ReproducesTheNativePayload()
    {
        byte[] body = SlipeJoinCompleteBody();
        Assert.True(MtaGamePackets.TryReadJoinCompleteBody(body, out string text, out string version));

        byte[] framed = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            ConnectionText = text,
            ServerVersionSortable = version,
        });

        Assert.Equal(20 + body.Length, framed.Length);
        Assert.Equal(body, framed[20..]);
    }

    /// <summary>
    /// Below 0x66 the leading block shrinks, so a client that negotiated an older version must not be
    /// sent the serial row. The mod-level tail is unchanged.
    /// </summary>
    [Fact]
    public void ReframingBelowTheVersionGates_KeepsTheModLevelTail()
    {
        byte[] body = SlipeJoinCompleteBody();
        Assert.True(MtaGamePackets.TryReadJoinCompleteBody(body, out string text, out string version));

        foreach ((ushort negotiated, int leadingBytes) in new (ushort, int)[] { (0x5F, 10), (0x60, 18), (0x66, 20) })
        {
            byte[] framed = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
            {
                BitStreamVersion = negotiated,
                ConnectionText = text,
                ServerVersionSortable = version,
            });

            Assert.Equal(leadingBytes + body.Length, framed.Length);
            Assert.Equal(body, framed[leadingBytes..]);
        }
    }

    /// <summary>
    /// The 870-byte payload a real client sent inside its PACKET_ID_PLAYER_JOINDATA, with the wire id
    /// already removed. <c>CClientGame.cpp</c> writes only the join body, so the leading 775 bytes are
    /// net-layer framing; the mod-level block starts at 775 and is the 95-byte body Slipe's
    /// PlayerJoinDataPacket reads. The client serial lives in that framing, not in the body: it is the
    /// 32 XOR-obfuscated ASCII hex characters at <c>[2, 34)</c>, right after the u16 bitstream version.
    /// </summary>
    private static byte[] RealClientJoinDataPayload() => new byte[]
    {
        0x91, 0x00, 0xE5, 0x96, 0x93, 0x9F, 0xF5, 0xC6, 0xA1, 0x15, 0xEF, 0x9B, 0x9B, 0xE6, 0xF5, 0xCE,
        0xDC, 0x18, 0xF6, 0x86, 0xF0, 0xF9, 0x97, 0xD3, 0xC5, 0x75, 0x8A, 0xFE, 0xF9, 0xF2, 0x9B, 0xA8,
        0xBA, 0x0B, 0x9A, 0x02, 0xE2, 0x91, 0xEB, 0xE9, 0xF7, 0xC4, 0xA0, 0x61, 0xEB, 0xEC, 0xE2, 0xE1,
        0x89, 0xC9, 0xA2, 0x62, 0x87, 0xFF, 0xF2, 0xF9, 0xE4, 0xA7, 0xBB, 0x74, 0xF9, 0xF6, 0x8B, 0xF1,
        0x9E, 0xDF, 0xB8, 0x7D, 0xC5, 0xC3, 0xB3, 0xB9, 0xD5, 0xE6, 0xF6, 0x44, 0xC9, 0xBA, 0xBF, 0xC5,
        0xAB, 0x9D, 0x8D, 0x49, 0xDD, 0xD6, 0xD7, 0xD6, 0xC9, 0xF9, 0x92, 0x20, 0xA8, 0xD7, 0xAF, 0xA4,
        0xCC, 0xF0, 0x9A, 0x5F, 0x97, 0xE2, 0x93, 0x8F, 0xA4, 0xB1, 0x48, 0xF3, 0x63, 0x64, 0x10, 0x27,
        0x44, 0x48, 0x4A, 0xF6, 0x73, 0x05, 0x08, 0x37, 0x21, 0x5D, 0x4B, 0xF3, 0x10, 0x12, 0x62, 0x22,
        0x30, 0x38, 0x3F, 0x83, 0x21, 0x21, 0x56, 0x61, 0x77, 0x7E, 0x92, 0x5E, 0xD1, 0xDF, 0xBA, 0xB7,
        0xC4, 0x99, 0xE3, 0x5C, 0xB1, 0xB2, 0xB1, 0xB8, 0xD4, 0xE4, 0xF2, 0x34, 0xC9, 0xCA, 0xC8, 0xB3,
        0xAE, 0x90, 0x89, 0x32, 0x44, 0x40, 0x47, 0x38, 0x58, 0x67, 0x07, 0xC0, 0x4B, 0x3C, 0x3F, 0x46,
        0x5E, 0x61, 0x0B, 0xB8, 0x21, 0x51, 0x21, 0x2F, 0x35, 0x78, 0x11, 0xD6, 0x5E, 0x5C, 0x5D, 0x5F,
        0x4F, 0x79, 0x13, 0xDB, 0x61, 0x6F, 0x6A, 0x66, 0x71, 0x33, 0x22, 0x94, 0x6D, 0x1F, 0x1F, 0x63,
        0x78, 0x3C, 0x23, 0x9C, 0x71, 0x72, 0x71, 0x78, 0x14, 0x24, 0x35, 0xF1, 0x74, 0x78, 0x0A, 0x76,
        0x61, 0x51, 0x3F, 0xF3, 0x75, 0x00, 0x75, 0x08, 0x13, 0x26, 0x43, 0x86, 0x7D, 0x78, 0x0E, 0x01,
        0x6D, 0x2F, 0x4C, 0xFC, 0x1D, 0x10, 0x14, 0x1B, 0x03, 0x32, 0x22, 0xE6, 0x65, 0x6B, 0x66, 0x07,
        0x01, 0x4E, 0x5E, 0x9E, 0x26, 0x20, 0x56, 0x5A, 0x30, 0x06, 0x16, 0xD6, 0x5F, 0x2B, 0x2C, 0x2E,
        0x4B, 0x00, 0x6B, 0xAC, 0x30, 0x40, 0x3A, 0x39, 0x25, 0x12, 0x74, 0xC0, 0x48, 0x3E, 0x3C, 0x3F,
        0x29, 0x6A, 0x0E, 0xBD, 0xB6, 0xB7, 0xB7, 0xC6, 0xD5, 0x95, 0xF6, 0x40, 0xBF, 0xBA, 0xCC, 0xC4,
        0xD1, 0xE9, 0x8E, 0x43, 0xDD, 0xDE, 0xD3, 0xAD, 0xB4, 0xF8, 0xE6, 0x21, 0xD8, 0xD7, 0xDB, 0xD2,
        0xC1, 0x8E, 0x9E, 0x5E, 0xE6, 0xE0, 0x96, 0x9A, 0xF7, 0xC3, 0xAB, 0x64, 0x9D, 0xEE, 0xE3, 0xEF,
        0xFD, 0xC1, 0xDA, 0x6C, 0x82, 0xF0, 0xF1, 0xF8, 0xE1, 0xD4, 0xC2, 0x04, 0xF9, 0xF9, 0x8F, 0xF1,
        0xEE, 0xAE, 0xB2, 0x7C, 0x83, 0x83, 0x81, 0x8C, 0xE2, 0xD7, 0xB4, 0x7B, 0xBD, 0x9F, 0x7F, 0x01,
        0x1B, 0x26, 0x2F, 0xDD, 0x24, 0x92, 0x6D, 0x14, 0x0B, 0x47, 0x37, 0xCD, 0x51, 0x97, 0x7C, 0x01,
        0x68, 0x50, 0x5D, 0xD8, 0x70, 0xC2, 0x38, 0x41, 0x59, 0x63, 0x69, 0x9B, 0x07, 0xB4, 0xBA, 0xB7,
        0x8C, 0x99, 0x11, 0xA8, 0x29, 0x28, 0x21, 0x6B, 0x7B, 0x05, 0x72, 0xB8, 0x5E, 0x2A, 0x29, 0x1D,
        0x79, 0x02, 0x10, 0xAD, 0xC7, 0xBD, 0xCE, 0xF9, 0x93, 0x90, 0xE4, 0x5B, 0xB9, 0xCA, 0xCD, 0xFD,
        0x9D, 0xE1, 0x7E, 0xBF, 0x2D, 0x2F, 0x51, 0x4F, 0x40, 0x6E, 0x15, 0xCC, 0x5C, 0x43, 0x52, 0x48,
        0x4B, 0x7C, 0x1A, 0xDE, 0x6D, 0x78, 0x63, 0x70, 0x71, 0x40, 0x23, 0xF3, 0xE1, 0x7F, 0x7A, 0x77,
        0x68, 0x58, 0x3F, 0xFA, 0x65, 0x67, 0x62, 0x69, 0x70, 0x41, 0x20, 0xE3, 0x8D, 0x6F, 0x7B, 0xBA,
        0x78, 0x4D, 0x3D, 0xEB, 0x95, 0x03, 0x12, 0x01, 0x24, 0x09, 0x7F, 0xBC, 0x4A, 0x5F, 0xA7, 0x29,
        0x37, 0x11, 0x2B, 0x94, 0x0F, 0x6F, 0xB0, 0x62, 0x34, 0x1D, 0x6E, 0xB8, 0x0D, 0x8E, 0x0E, 0x03,
        0x18, 0x29, 0x4D, 0x8B, 0x35, 0x37, 0x32, 0x3F, 0x27, 0x11, 0x72, 0xB3, 0x3D, 0x3E, 0x32, 0x37,
        0x28, 0x19, 0x7A, 0xBB, 0x2D, 0x27, 0x22, 0x2F, 0x31, 0x01, 0x62, 0xA3, 0x2D, 0x2F, 0x2A, 0x27,
        0x38, 0x09, 0x6A, 0xAB, 0xD4, 0xD6, 0xD2, 0xDF, 0xC0, 0xF1, 0x92, 0x53, 0xDD, 0xDF, 0xDA, 0xD7,
        0xA4, 0xF9, 0xE7, 0x5B, 0xB7, 0xC7, 0xB3, 0xCF, 0xFF, 0xE1, 0x00, 0x6F, 0xCD, 0xA8, 0xDE, 0xA3,
        0xB6, 0x96, 0xC5, 0x1F, 0x67, 0x37, 0x4A, 0x5C, 0x4B, 0x14, 0x7C, 0xA2, 0xD0, 0x2D, 0x57, 0x44,
        0x22, 0x04, 0x64, 0xCF, 0xDD, 0x24, 0x5A, 0x3F, 0x2D, 0x76, 0x69, 0xC6, 0xB0, 0x68, 0x12, 0x06,
        0x16, 0x4A, 0x22, 0x89, 0x8E, 0x8D, 0x87, 0x8E, 0x90, 0xE2, 0xD2, 0x90, 0xCD, 0x9F, 0x6C, 0xF4,
        0x00, 0xEE, 0x9C, 0x01, 0x78, 0xD9, 0xD3, 0x40, 0xDF, 0x33, 0x3E, 0x98, 0xF5, 0x6E, 0xC9, 0x5D,
        0xC7, 0x5A, 0x2E, 0x80, 0xA8, 0x43, 0xE0, 0x70, 0x9C, 0x75, 0x7C, 0xAD, 0xB1, 0x2E, 0xAC, 0x38,
        0xA5, 0x4E, 0x40, 0xE6, 0xEE, 0x70, 0x29, 0xCD, 0x4D, 0xA8, 0x48, 0x63, 0x25, 0x6D, 0x42, 0x00,
        0x30, 0x7D, 0xD4, 0x83, 0xCC, 0xDB, 0x29, 0x2F, 0xA6, 0x48, 0x2D, 0x86, 0xE5, 0x93, 0xC5, 0x20,
        0xA4, 0x40, 0x03, 0x8E, 0x2F, 0x89, 0xD3, 0x38, 0xBC, 0x58, 0x3B, 0x96, 0xE4, 0x83, 0x02, 0x18,
        0x00, 0x95, 0x55, 0xE0, 0x18, 0x4C, 0x7A, 0xD4, 0x3D, 0x00, 0x13, 0xD2, 0xFB, 0x16, 0xD9, 0x89,
        0x16, 0xB7, 0xA6, 0xA5, 0x78, 0x64, 0x58, 0x60, 0xB3, 0xA6, 0x37, 0x6C, 0x58, 0x34, 0xDB, 0x80,
        0x35, 0xD4, 0x8E, 0x08, 0xD8, 0xFA, 0x80, 0x91, 0x1B, 0xC8, 0xAB, 0x00, 0x8A, 0xE5, 0x04, 0xEB,
        0x24, 0x11, 0xCA, 0xAB, 0x1A, 0xD3, 0x48, 0xE5, 0xBF, 0xAF, 0x4E, 0x47, 0x08, 0x89, 0xD8, 0x8F,
        0x01, 0x97, 0xE8, 0x4B, 0xD4, 0x2E, 0xC1, 0xDA, 0x01, 0x60, 0x01, 0x91, 0x00, 0x0F, 0x00, 0x31,
        0x2E, 0x36, 0x2E, 0x30, 0x2D, 0x39, 0x2E, 0x32, 0x34, 0x31, 0x34, 0x39, 0x2E, 0x30, 0x85, 0xA9,
        0xA0, 0xA2, 0xA9, 0x9F, 0x27, 0x30, 0xB7, 0x37, 0xA6, 0x30, 0xB8, 0x3A, 0x37, 0xB8, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x80, 0x00, 0x00, 0x48, 0x1D, 0xA4,
        0x15, 0x1F, 0x27, 0x30, 0xB7, 0x00,
    };

    [Fact]
    public void RealClientJoinDataPayload_HasTheExpectedLength()
    {
        Assert.Equal(870, RealClientJoinDataPayload().Length);
    }

    [Fact]
    public void FindPlayerJoinDataBlockOffset_LocatesTheRealClientBlock()
    {
        byte[] payload = RealClientJoinDataPayload();

        Assert.Equal(775, MtaGamePackets.FindPlayerJoinDataBlockOffset(payload));
    }

    [Fact]
    public void ExtractPlayerJoinDataBlock_ReturnsTheNinetyFiveByteBody()
    {
        byte[] payload = RealClientJoinDataPayload();

        byte[] body = MtaGamePackets.ExtractPlayerJoinDataBlock(payload);

        Assert.Equal(95, body.Length);
        Assert.Equal(new byte[] { 0xDA, 0x01, 0x60, 0x01, 0x91, 0x00 }, body[..6]);
        Assert.Equal(payload[775..], body);
    }

    /// <summary>
    /// A payload that already begins with the join body (as a bare 95-byte block does) must be passed
    /// through untouched, by reference, so the caller can tell "no prefix" from "stripped".
    /// </summary>
    [Fact]
    public void ExtractPlayerJoinDataBlock_ReturnsTheInputUnchangedWhenItStartsTheBlock()
    {
        byte[] payload = RealClientJoinDataPayload();
        byte[] bare = payload[775..];

        byte[] result = MtaGamePackets.ExtractPlayerJoinDataBlock(bare);

        Assert.Equal(0, MtaGamePackets.FindPlayerJoinDataBlockOffset(bare));
        Assert.Same(bare, result);
    }

    [Fact]
    public void FindPlayerJoinDataBlockOffset_ReturnsMinusOneWhenNoBlockIsPresent()
    {
        byte[] noise = new byte[200];
        for (int i = 0; i < noise.Length; i++)
            noise[i] = (byte)(i * 37 + 11);

        Assert.Equal(-1, MtaGamePackets.FindPlayerJoinDataBlockOffset(noise));

        byte[] result = MtaGamePackets.ExtractPlayerJoinDataBlock(noise);
        Assert.Same(noise, result);
    }

    /// <summary>
    /// Slipe's FetchSerial() asks for the PlayerVersion string, which the client does write into the
    /// join body as a length-prefixed free-form string.
    /// </summary>
    [Fact]
    public void TryReadPlayerJoinDataVersion_ReadsTheVersionFromTheRealClientBlock()
    {
        byte[] body = MtaGamePackets.ExtractPlayerJoinDataBlock(RealClientJoinDataPayload());

        Assert.True(MtaGamePackets.TryReadPlayerJoinDataVersion(body, out string playerVersion));

        Assert.Equal("1.6.0-9.24149.0", playerVersion);
    }

    /// <summary>
    /// The serial is a net-layer field, not a mod-level one: it precedes the 95-byte join body and
    /// travels XOR-obfuscated as 32 ASCII hex characters. The reference capture's client reported
    /// 5DDE026C7AD482CF6D73B7B3B460FD5E from a stock server, which is the same machine that produced
    /// this payload.
    /// </summary>
    [Fact]
    public void TryReadPlayerJoinDataSerial_ReadsTheRealClientSerialFromTheNetLayerPrefix()
    {
        Assert.True(MtaGamePackets.TryReadPlayerJoinDataSerial(RealClientJoinDataPayload(), out byte[] serial));

        Assert.Equal("5DDE026C7AD482CF6D73B7B3B460FD5E", Encoding.ASCII.GetString(serial));
    }

    /// <summary>
    /// Pins the serial's encoding and width independently of any single capture: 32 ASCII hex
    /// characters, because the client sends a 16-byte digest rendered as text.
    /// </summary>
    [Fact]
    public void PlayerJoinDataSerial_IsThirtyTwoAsciiHexCharacters()
    {
        Assert.True(MtaGamePackets.TryReadPlayerJoinDataSerial(RealClientJoinDataPayload(), out byte[] serial));

        Assert.Equal(MtaGamePackets.ClientSerialLength, serial.Length);
        Assert.All(serial, b => Assert.True(
            (b >= (byte)'0' && b <= (byte)'9') || (b >= (byte)'A' && b <= (byte)'F'),
            $"serial byte 0x{b:X2} is not an uppercase ASCII hex digit"));
    }

    /// <summary>
    /// Pins the de-obfuscation from the other direction: encode a known serial the way the client's
    /// net layer does and require the reader to recover it. This fails loudly if the width, the key
    /// or the index mix changes. The transform is its own inverse, so the same expression encodes.
    /// </summary>
    [Theory]
    [InlineData("5DDE026C7AD482CF6D73B7B3B460FD5E")]
    [InlineData("00000000000000000000000000000000")]
    [InlineData("0123456789ABCDEF0123456789ABCDEF")]
    public void TryReadPlayerJoinDataSerial_InvertsTheNetLayerObfuscation(string plaintext)
    {
        const ushort bitStreamVersion = 0x91;
        byte[] payload = new byte[2 + MtaGamePackets.ClientSerialLength + 8];
        payload[0] = (byte)bitStreamVersion;
        payload[1] = (byte)(bitStreamVersion >> 8);

        for (int i = 0; i < MtaGamePackets.ClientSerialLength; i++)
            payload[2 + i] = (byte)(i ^ plaintext[i] ^ 0xD1 ^ (1 << (i & 7)));

        Assert.True(MtaGamePackets.TryReadPlayerJoinDataSerial(payload, out byte[] serial));

        Assert.Equal(plaintext, Encoding.ASCII.GetString(serial));
    }

    /// <summary>
    /// The body's trailing 32 bytes are an empty community-info field — <c>CClientGame.cpp</c> writes
    /// <c>// Append community information (Removed)</c> there — so reading the serial out of the body
    /// is exactly the bug this guards. A bare body must not yield a serial.
    /// </summary>
    [Fact]
    public void TryReadPlayerJoinDataSerial_RejectsTheModLevelBody()
    {
        byte[] body = MtaGamePackets.ExtractPlayerJoinDataBlock(RealClientJoinDataPayload());

        Assert.False(MtaGamePackets.TryReadPlayerJoinDataSerial(body, out byte[] serial));

        Assert.Empty(serial);
    }

    [Fact]
    public void TryReadPlayerJoinDataSerial_RejectsAWindowThatIsNotHex()
    {
        byte[] payload = (byte[])RealClientJoinDataPayload().Clone();
        payload[2] ^= 0x40;

        Assert.False(MtaGamePackets.TryReadPlayerJoinDataSerial(payload, out byte[] serial));

        Assert.Empty(serial);
    }

    [Fact]
    public void TryReadPlayerJoinDataSerial_ReturnsFalseForATruncatedPayload()
    {
        Assert.False(MtaGamePackets.TryReadPlayerJoinDataSerial(RealClientJoinDataPayload()[..33], out _));
    }

    [Fact]
    public void TryReadPlayerJoinDataVersion_ReturnsFalseAndLeavesOutputEmptyForNonJoinData()
    {
        byte[] noise = new byte[200];
        for (int i = 0; i < noise.Length; i++)
            noise[i] = (byte)(i * 37 + 11);

        Assert.False(MtaGamePackets.TryReadPlayerJoinDataVersion(noise, out string playerVersion));

        Assert.Equal(string.Empty, playerVersion);
    }

    [Fact]
    public void TryReadPlayerJoinDataVersion_ReturnsFalseForATruncatedBlock()
    {
        byte[] body = MtaGamePackets.ExtractPlayerJoinDataBlock(RealClientJoinDataPayload());

        Assert.False(MtaGamePackets.TryReadPlayerJoinDataVersion(body[..40], out _));
    }
}
