using Xunit;

using SlipeServer.Net.Managed;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the offline-handshake config envelope against the one recording of it that exists: the tail of
/// the real client's <c>RID_OPEN_CONNECTION_REQUEST</c> (capture.log seq 1).
///
/// <para>
/// This is a <b>literal-input</b> test. The bytes below are transcribed from the capture, not produced
/// by any part of the port, so a misreading in the port cannot cancel out. Note, however, that
/// <c>SlipeServer.Net.Managed</c> contains <b>no TLV parser and no TLV serializer</b> — the grammar is
/// knowledge about the native binary that the managed port has not needed yet. These tests therefore
/// pin the <i>recorded bytes</i> and the <i>documented grammar</i> side by side, so that whoever writes
/// the first parser has a golden vector waiting for them.
/// </para>
///
/// <para>
/// They also record a real disagreement between the recording and the documented tag table: the
/// client's envelope opens <c>0xED 0xCA 0x01 0x01 …</c>, and <c>0xCA</c> is not one of the documented
/// tags. The test asserts the bytes rather than guessing a meaning.
/// </para>
/// </summary>
public class TlvConfigGrammarTests
{
    // capture.log seq 1, RX, real client 192.0.2.10:58057. The full datagram is
    // 09 04 <client guid 16> <modified magic 16> <config envelope 17>, and the envelope is the last
    // 17 bytes.
    private const string RecordedEnvelope = "EDCA0101A804E8030000C804E8030000DE";

    private const int EnvelopeOffset = 34;

    /// <summary>
    /// The grammar shared by both parsers lives inside a <c>0xED ... 0xDE</c> envelope.
    /// </summary>
    [Fact]
    public void TheRecordedEnvelopeIsDelimitedByEdAndDe()
    {
        byte[] request = CaptureVectors.OpenRequestBytes;
        byte[] envelope = request[EnvelopeOffset..];

        Assert.Equal(17, envelope.Length);
        Assert.Equal(RecordedEnvelope, WireTestKit.HexOf(envelope));
        Assert.Equal(0xED, envelope[0]);
        Assert.Equal(0xDE, envelope[^1]);

        // The envelope is a suffix: everything before it is the fixed 34-byte request header
        // (id 09, version 04, 16-byte client GUID, 16-byte modified magic).
        Assert.Equal(34, request.Length - envelope.Length);
        Assert.Equal(0x09, request[0]);
        Assert.Equal(0x04, request[1]);
        Assert.Equal("00FF00FFFEFEFEFEFDFDFDFD78563417", WireTestKit.HexOf(request[18..34]));
    }

    /// <summary>
    /// <c>0xA8</c> and <c>0xC8</c> each carry a length byte that must be 4 followed by a 32-bit
    /// tunable. The recording carries both, both set to 1000.
    /// </summary>
    [Fact]
    public void BothTunableTagsCarryAFourByteLengthAndTheValueOneThousand()
    {
        byte[] envelope = Convert.FromHexString(RecordedEnvelope);

        AssertTunable(envelope, offset: 4, expectedTag: 0xA8, expectedValue: 1000u);
        AssertTunable(envelope, offset: 10, expectedTag: 0xC8, expectedValue: 1000u);

        // Both tunables are the same value in this recording, so a reader that swapped the two tags
        // or read the wrong one twice would still pass a value-only assertion. Assert the tag bytes.
        Assert.Equal(0xA8, envelope[4]);
        Assert.Equal(0xC8, envelope[10]);
        Assert.NotEqual(envelope[4], envelope[10]);
    }

    /// <summary>
    /// The config exchange does not turn on a cipher. The documented <c>0x80</c> flag is the only tag
    /// that could, and this recording does not contain it. Even when present the serializer reads a
    /// permanently-zero constant, so a managed server needs no RC4.
    /// </summary>
    [Fact]
    public void TheRecordedEnvelopeCarriesNoEncryptionFlag()
    {
        byte[] envelope = Convert.FromHexString(RecordedEnvelope);

        Assert.DoesNotContain((byte)0x80, envelope);
    }

    /// <summary>
    /// The documented table lists <c>0xED</c>, <c>0x80</c>, <c>0x6E</c>, <c>0x1A</c>, <c>0xA8</c>,
    /// <c>0xC8</c> and <c>0xDE</c>. The real client opens with <c>0xED 0xCA 0x01 0x01</c>, and
    /// <c>0xCA</c> is not in that set — so the table is incomplete. This test asserts the observed
    /// bytes so the gap cannot be silently "reconciled" by editing either side.
    /// </summary>
    [Fact]
    public void TheEnvelopeOpensWithATagTheDocumentedGrammarDoesNotList()
    {
        byte[] envelope = Convert.FromHexString(RecordedEnvelope);

        byte[] documentedTags = { 0xED, 0x80, 0x6E, 0x1A, 0xA8, 0xC8, 0xDE };

        Assert.Equal((byte)0xCA, envelope[1]);
        Assert.DoesNotContain((byte)0xCA, documentedTags);

        // The three bytes after the open tag are literal and unexplained; pin them so that adding a
        // meaning to them later is a deliberate, reviewable change.
        Assert.Equal("CA0101", WireTestKit.HexOf(envelope[1..4]));
    }

    /// <summary>
    /// The case-9 success reply is exactly 33 bytes —
    /// <c>[0x0A][modified magic 16][our GUID 16]</c> — with <b>no</b> <c>0xED … 0xDE</c> block
    /// appended, even though the request carried one. Driven through the real receive path.
    /// </summary>
    [Fact]
    public void TheOpenConnectionReplyAppendsNoConfigEnvelope()
    {
        using var harness = new WrapperHarness();
        int txBefore = harness.Tx.Count;

        harness.Inject(CaptureVectors.OpenRequestBytes);

        Assert.Equal(txBefore + 1, harness.Tx.Count);
        byte[] reply = harness.Tx[^1].Bytes;

        Assert.Equal(33, reply.Length);
        Assert.Equal(0x0A, reply[0]);
        // Scope the envelope check to the fixed header. Bytes 17-32 are the random server GUID and may
        // legitimately contain 0xED or 0xDE; a 17-byte envelope cannot fit in a 33-byte reply anyway.
        Assert.DoesNotContain((byte)0xED, reply[..17]);
        Assert.DoesNotContain((byte)0xDE, reply[..17]);
        Assert.Equal(WireTestKit.HexOf(harness.Wrapper.ServerGuid), WireTestKit.HexOf(reply[17..33]));
    }

    private static void AssertTunable(byte[] envelope, int offset, byte expectedTag, uint expectedValue)
    {
        Assert.Equal(expectedTag, envelope[offset]);
        Assert.Equal(4, envelope[offset + 1]);

        // The tunable is a 32-bit little-endian field (__BITSTREAM_NATIVE_END is set).
        Assert.Equal(expectedValue, BitConverter.ToUInt32(envelope, offset + 2));
    }
}