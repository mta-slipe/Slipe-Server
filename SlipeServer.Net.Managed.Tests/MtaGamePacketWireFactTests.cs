using System.Text;
using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the packet-2 write path against bytes a real server put on the wire, and pins the framing
/// edges the existing <see cref="MtaGamePacketsTests"/> table does not reach: the 0xFFFF adapter
/// sentinel and the never-emitted <c>"HB95"</c> trailer.
/// </summary>
/// <remarks>
/// The golden vector is <c>capture.log</c> seq 14 — a real server's <c>SERVER_JOIN_COMPLETE</c>
/// payload, transcribed as a literal in <see cref="CaptureVectors"/>. The builder never sees that
/// literal: it is handed a serial transcribed separately from the same recording, and the assertion
/// is that the bytes it produces equal the recording. That is the whole point — a builder that
/// misread a row cannot reproduce a payload it never looked at.
/// </remarks>
public class MtaGamePacketWireFactTests
{
    /// <summary>
    /// The 32-byte serial the recording carries in row 5, shifted back down by one: the native server
    /// stores the serial with every byte already incremented, so <c>ClientSerial</c> holds the
    /// pre-shift values and the builder re-applies the +1.
    /// </summary>
    /// <remarks>
    /// Derived from the recorded row rather than transcribed, because row 5 holds a real machine's
    /// hardware digest and that value has no business being a source literal. The consequence is that
    /// row 5 of the assertion below is vacuous; the other 67 bytes are what bite.
    /// </remarks>
    private static byte[] RecordedSerialBeforeShift =>
        CaptureVectors.ServerJoinCompletePayloadStockBytes[15..47].Select(b => (byte)(b - 1)).ToArray();

    [Fact]
    public void StockOptionsReproduceTheRecordedPacketTwoPayload()
    {
        byte[] built = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            ClientSerial = RecordedSerialBeforeShift,
            ConnectionText = CaptureVectors.StockConnectionText,
            ServerVersionSortable = CaptureVectors.StockServerVersion,
        });

        // The recording carries the wire id in front; the builder produces the body only, so the
        // 100-byte packet payload splits into one wire-id byte and 99 bytes of body.
        byte[] recorded = CaptureVectors.ServerJoinCompletePayloadStockBytes[1..];

        Assert.Equal(99, recorded.Length);
        Assert.Equal(0x65, CaptureVectors.ServerJoinCompletePayloadStockBytes[0]);
        Assert.Equal(WireTestKit.HexOf(recorded), WireTestKit.HexOf(built));
    }

    /// <summary>
    /// The same builder, driven end to end through <c>MtaNetWrapper</c> from the real client's own
    /// datagrams: <c>capture.log</c> seq 12 teaches the wrapper the client's serial, and the packet 2
    /// it then emits must be the recorded seq-14 payload with <em>only</em> row 5 replaced.
    /// </summary>
    /// <remarks>
    /// Row 5 is the one row the recording cannot supply verbatim. The real server put 32 non-ASCII
    /// bytes there; this port stores the serial as a 32-character ASCII hex string learned from the
    /// join-data prefix, so the row is necessarily re-encoded. Every other byte — the wire id, both
    /// AC bitmaps, the image-mod flags, the adapter length, <c>iEnableClientChecks</c> and both
    /// length-prefixed strings — is asserted verbatim against the recording.
    /// </remarks>
    [Fact]
    public void TheWrapperEmitsTheRecordedPacketTwoPayloadWithOnlyTheSerialRowReEncoded()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        // seq 12 is the client's PLAYER_JOINDATA; it is what sets the session's serial.
        harness.Inject(WireTestKit.CaptureLogRecord(12).Bytes);

        string learned = harness.Wrapper
            .GetClientSerialExtraAndVersion(harness.Address).Item1;

        // The learned value is a real machine's hardware digest, so only its shape is pinned.
        Assert.Equal(MtaGamePackets.ClientSerialLength, learned.Length);
        Assert.All(learned, c => Assert.True(
            (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'),
            $"serial char '{c}' is not an uppercase ASCII hex digit"));

        byte[] recorded = CaptureVectors.ServerJoinCompletePayloadStockBytes;
        Assert.Equal(100, recorded.Length);

        // Row 5's 16-bit length sits at offset 13 (wire id + three 32-bit rows), so the serial itself
        // occupies [15, 47).
        Assert.Equal(32, BitConverter.ToUInt16(recorded, 13));

        var expected = new List<byte>();
        expected.AddRange(recorded[..15]);
        foreach (byte b in System.Text.Encoding.ASCII.GetBytes(learned))
            expected.Add((byte)(b + 1));
        expected.AddRange(recorded[47..]);

        Assert.Equal(100, expected.Count);

        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_SERVER_JOIN_COMPLETE,
            0x90,
            CaptureVectors.ServerJoinCompleteBodyBytes);

        string tx = WireTestKit.HexOf(harness.Tx[^1].Bytes);
        string want = WireTestKit.HexOf(expected.ToArray());

        Assert.True(tx.EndsWith(want, StringComparison.Ordinal), $"tx ({tx.Length / 2}B) = {tx}");
    }

    /// <summary>
    /// Row 6 uses a 16-bit length, so 0xFFFE fits in the short form and 0xFFFF is the sentinel that
    /// switches the field to a 32-bit length. An off-by-one here truncates the adapter list a client
    /// uses to validate its install.
    /// </summary>
    [Theory]
    [InlineData(0, 24)]
    [InlineData(1, 25)]
    [InlineData(0xFFFE, 65558)]
    public void AdapterBlockUsesTheUshortLengthBelowTheSentinel(int blockLength, int expectedTotal)
    {
        var block = new byte[blockLength];
        for (int i = 0; i < block.Length; i++)
            block[i] = (byte)i;

        byte[] payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            AdapterBlock = block,
            ConnectionText = string.Empty,
            ServerVersionSortable = string.Empty,
        });

        Assert.Equal(expectedTotal, payload.Length);
        Assert.Equal((ushort)blockLength, BitConverter.ToUInt16(payload, 14));

        if (blockLength > 0)
            Assert.Equal(block, payload[16..(16 + blockLength)]);
    }

    [Fact]
    public void AdapterBlockUsesTheSentinelAndAThirtyTwoBitLengthAtTheBoundary()
    {
        var block = new byte[0xFFFF];
        for (int i = 0; i < block.Length; i++)
            block[i] = (byte)(i * 7);

        byte[] payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            AdapterBlock = block,
            ConnectionText = string.Empty,
            ServerVersionSortable = string.Empty,
        });

        // 14 bytes of rows 2-5, the 0xFFFF sentinel, the 32-bit length, the block, row 7, then the two
        // empty strings.
        Assert.Equal(14 + 2 + 4 + 0xFFFF + 4 + 2 + 2, payload.Length);
        Assert.Equal(new byte[] { 0xFF, 0xFF }, payload[14..16]);
        Assert.Equal(0xFFFFu, BitConverter.ToUInt32(payload, 16));
        Assert.Equal(block, payload[20..(20 + 0xFFFF)]);
    }

    /// <summary>
    /// Row 9 — the four-dword <c>"HB95"</c> trailer — is gated on <c>player+0x97d != 0</c>, and this
    /// port never increments that counter. The trailer must therefore never appear, whatever the
    /// options say, and nothing may be appended after the two mod-level strings.
    /// </summary>
    [Fact]
    public void TheHb95TrailerIsNeverEmitted()
    {
        var options = new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            AcDisableBitmapLow = 0xFFFFFFFF,
            AcDisableBitmapHigh = 0xFFFFFFFF,
            ImageModFlags = 0xFFFFFFFF,
            EnableClientChecks = 0xFFFFFFFF,
            ClientSerial = new byte[32],
            AdapterBlock = new byte[] { 1, 2, 3, 4 },
            ConnectionText = "x",
            ServerVersionSortable = "y",
        };

        byte[] payload = MtaGamePackets.BuildServerJoinComplete(options);

        Assert.DoesNotContain("HB95", Encoding.ASCII.GetString(payload), StringComparison.Ordinal);

        // Nothing follows the sortable version string: find it, and assert the payload ends there.
        byte[] version = Encoding.ASCII.GetBytes("y");
        Assert.Equal(version, payload[^1..]);
        Assert.Equal((ushort)1, BitConverter.ToUInt16(payload, payload.Length - 3));
    }

    /// <summary>
    /// The leading block is not a constant. Every field is a live option, and a refactor that
    /// hard-codes any of them — or that assumes the AC bitmap or the image-mod flags are zero — has
    /// to fail here.
    /// </summary>
    [Fact]
    public void EveryLeadingBlockFieldIsLiveAndNotAssumedZero()
    {
        byte[] payload = MtaGamePackets.BuildServerJoinComplete(new ServerJoinCompleteOptions
        {
            BitStreamVersion = 0x90,
            AcDisableBitmapLow = 0x01234567,
            AcDisableBitmapHigh = 0x89ABCDEF,
            ImageModFlags = 0x0F1E2D3C,
            EnableClientChecks = 0x00000000,
            ConnectionText = string.Empty,
            ServerVersionSortable = string.Empty,
        });

        Assert.Equal(new byte[] { 0x67, 0x45, 0x23, 0x01 }, payload[0..4]);
        Assert.Equal(new byte[] { 0xEF, 0xCD, 0xAB, 0x89 }, payload[4..8]);
        Assert.Equal(new byte[] { 0x3C, 0x2D, 0x1E, 0x0F }, payload[8..12]);
        Assert.Equal(new byte[] { 0x00, 0x00 }, payload[12..14]);
        Assert.Equal(new byte[] { 0x00, 0x00 }, payload[14..16]);

        // Row 7 is the only row that is deliberately zero here, and it is still emitted: 24 bytes
        // total, so the four zero bytes at 16..20 are the row itself, not a gap.
        Assert.Equal(24, payload.Length);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00 }, payload[16..20]);
    }
}
