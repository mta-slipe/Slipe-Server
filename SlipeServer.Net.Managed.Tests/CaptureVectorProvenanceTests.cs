using System;
using System.Collections.Generic;
using System.Linq;
using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Guards the golden vectors themselves.
///
/// <para>
/// Every other wire test in this project is only as trustworthy as
/// <see cref="CaptureVectors"/>. If a literal is mistranscribed, the test that consumes it does not
/// fail — it silently pins the wrong bytes and the suite starts certifying a defect. This file
/// removes that failure mode by re-deriving every vector from its cited source mechanically: for
/// recorded vectors the source is the <c>capture.log</c> record named in the comment, and for
/// hand-built vectors it is the byte layout written out in the vector comment itself.
/// </para>
///
/// <para>
/// It also sanity-checks the oracle file itself, because a truncated or re-ordered
/// <c>capture.log</c> would break the replay test in a way that looks like a port defect.
/// </para>
/// </summary>
public class CaptureVectorProvenanceTests
{
    /// <summary>
    /// Each recorded vector must be byte-identical to the <c>hex</c> field of the record its comment
    /// cites. This is the single test that makes the rest of the literal-input suite meaningful.
    /// </summary>
    [Theory]
    [InlineData(1, CaptureVectors.OpenConnectionRequest)]
    [InlineData(2, CaptureVectors.OpenConnectionReplyStock)]
    [InlineData(3, CaptureVectors.ConnectedConnectionRequest)]
    [InlineData(4, CaptureVectors.ConnectedReplyStock)]
    [InlineData(6, CaptureVectors.IncomingConnectionAndSystemPacket)]
    [InlineData(10, CaptureVectors.TextPacketStock)]
    [InlineData(14, CaptureVectors.ServerJoinCompleteStock)]
    [InlineData(18, CaptureVectors.TwoWire7BPackets)]
    [InlineData(20, CaptureVectors.RpcPacketWire83)]
    [InlineData(23, CaptureVectors.RpcPacketWire7B)]
    [InlineData(303, CaptureVectors.DisconnectionNotification)]
    public void RecordedVectorMatchesItsCitedCaptureRecord(int seq, string vector)
    {
        CaptureLine record = WireTestKit.CaptureLogRecord(seq);

        Assert.Equal(WireTestKit.HexOf(record.Bytes), WireTestKit.HexOf(WireTestKit.Hex(vector)));
        Assert.Equal(record.Length, record.Bytes.Length);
    }

    /// <summary>
    /// The payload half of a recorded vector must be the tail of the datagram that carries it. This
    /// pins the split point the end-to-end tests assert against, independently of the datagram
    /// header, so a header-layout change cannot quietly shift what "the payload" means.
    /// </summary>
    [Fact]
    public void RecordedPayloadsAreTheTailsOfTheirDatagrams()
    {
        byte[] joinComplete = WireTestKit.CaptureLogRecord(14).Bytes;
        Assert.EndsWith(
            WireTestKit.HexOf(CaptureVectors.ServerJoinCompletePayloadStockBytes),
            WireTestKit.HexOf(joinComplete));

        byte[] text = WireTestKit.CaptureLogRecord(10).Bytes;
        Assert.EndsWith(
            WireTestKit.HexOf(CaptureVectors.TextPacketPayloadStockBytes),
            WireTestKit.HexOf(text));
    }

    /// <summary>
    /// The mod-level body of the join-complete must be exactly the two length-prefixed strings that
    /// close the recorded payload — the part the packet-2 leading block leaves untouched.
    /// </summary>
    [Fact]
    public void JoinCompleteBodyIsTheTailOfTheRecordedPayload()
    {
        Assert.EndsWith(
            WireTestKit.HexOf(CaptureVectors.ServerJoinCompleteBodyBytes),
            WireTestKit.HexOf(CaptureVectors.ServerJoinCompletePayloadStockBytes));
    }

    /// <summary>
    /// The recorded join-complete's leading block, decoded field by field against
    /// the documented layout. Asserting the fields rather than the raw hex is what makes
    /// this a check on the <em>documented layout</em> and not just on the transcription.
    /// </summary>
    [Fact]
    public void RecordedJoinCompleteLeadingBlockMatchesTheDocumentedWriteOrder()
    {
        byte[] payload = CaptureVectors.ServerJoinCompletePayloadStockBytes;
        int offset = 0;

        Assert.Equal((byte)0x65, payload[offset++]);                  // wire id for mtaId 2
        Assert.Equal(0x90D1D000u, ReadUInt32(payload, offset));       // row 2: bitmap low
        offset += 4;
        Assert.Equal(0x0000001Fu, ReadUInt32(payload, offset));       // row 3: bitmap high
        offset += 4;
        Assert.Equal(0u, ReadUInt32(payload, offset));                // row 4: image-mod flags
        offset += 4;

        int serialLength = ReadUInt16(payload, offset);               // row 5: client serial
        offset += 2;
        Assert.Equal(32, serialLength);
        offset += serialLength;

        Assert.Equal(0, ReadUInt16(payload, offset));                 // row 6: adapter length 0
        offset += 2;

        Assert.Equal(uint.MaxValue, ReadUInt32(payload, offset));     // row 7: iEnableClientChecks
        offset += 4;

        Assert.Equal(CaptureVectors.ServerJoinCompleteBodyBytes, payload[offset..]); // row 8
    }

    /// <summary>
    /// The stock server's AC-disable bitmap defaults are the ones the port emits. If a refactor
    /// changes <c>ServerJoinCompleteOptions</c>'s defaults, a real client sees a different anti-cheat
    /// profile and this fails.
    /// </summary>
    [Fact]
    public void ServerJoinCompleteDefaultsAreTheStockServersValues()
    {
        var options = new ServerJoinCompleteOptions();

        Assert.Equal(0x90D1D000u, options.AcDisableBitmapLow);
        Assert.Equal(0x0000001Fu, options.AcDisableBitmapHigh);
        Assert.Equal(0u, options.ImageModFlags);
        Assert.Equal(uint.MaxValue, options.EnableClientChecks);
        Assert.Equal(0x90, options.BitStreamVersion);
    }

    /// <summary>
    /// <see cref="CaptureVectors.PingRequest"/> is hand-built from the geometry row
    /// <c>1 | 2 → (9, 0x19, 15)</c>: magic at offset 9, 15 bytes compared, total length 0x19.
    /// </summary>
    [Fact]
    public void HandBuiltPingMatchesTheDocumentedGeometryRow()
    {
        byte[] ping = CaptureVectors.PingRequestBytes;

        Assert.Equal(0x19, ping.Length);
        Assert.Equal(0x01, ping[0]);
        Assert.Equal(CaptureVectors.Magic, ping[9..25]);
        Assert.Equal(CaptureVectors.PingClientTime, BitConverter.ToUInt64(ping, 1));
    }

    /// <summary>
    /// The bad-version request must differ from the recorded one in exactly one byte, so the refusal
    /// test's failure cannot be blamed on an unrelated transcription change.
    /// </summary>
    [Fact]
    public void HandBuiltBadVersionRequestDiffersFromTheRecordingInOneByte()
    {
        byte[] good = CaptureVectors.OpenRequestBytes;
        byte[] bad = WireTestKit.Hex(CaptureVectors.OpenConnectionRequestBadVersion);

        Assert.Equal(0x04, good[1]);
        Assert.Equal(0x03, bad[1]);
        Assert.Equal(good.Length, bad.Length);
        Assert.Equal(good[0], bad[0]);
        Assert.Equal(good[2..], bad[2..]);
    }

    /// <summary>
    /// The oracle file's shape: two recorded sessions, one client endpoint, and every declared length
    /// agreeing with its hex. A truncated or re-ordered file would otherwise make the replay test
    /// fail in a way that reads like a port defect.
    /// </summary>
    [Fact]
    public void CaptureLogIsTheDocumentedTwoSessionRecording()
    {
        IReadOnlyList<CaptureLine> lines = WireTestKit.CaptureLog;

        Assert.Equal(304, lines.Count);
        Assert.Equal(139, lines.Count(line => line.IsRx));
        Assert.Equal(165, lines.Count(line => line.IsTx));
        Assert.Single(lines.Select(line => line.EndPoint).Distinct());
        Assert.Equal("192.0.2.10:58057", lines[0].EndPoint);
        Assert.All(lines, line => Assert.Equal(line.Length, line.Bytes.Length));
        Assert.Equal(Enumerable.Range(1, 304), lines.Select(line => line.Seq));
    }

    /// <summary>
    /// The record numbers the vectors cite must still be the records they claim to be. A re-recorded
    /// session would renumber everything, and this names the classification each citation depends on
    /// so the failure says which one moved.
    /// </summary>
    [Theory]
    [InlineData(1, "route=offline id=0x09")]
    [InlineData(3, "route=connected first=0x4E")]
    [InlineData(20, "route=connected id=0x5F RID_INCOMPATIBLE_PROTOCOL_VERSION magic@2 MISMATCH len=47 expected=34")]
    public void CitedRecordsCarryTheirDocumentedClassification(int seq, string expected)
    {
        Assert.Contains(expected, WireTestKit.CaptureLogRecord(seq).Classification, StringComparison.Ordinal);
    }

    private static ushort ReadUInt16(byte[] buffer, int offset) =>
        BitConverter.ToUInt16(buffer, offset);

    private static uint ReadUInt32(byte[] buffer, int offset) =>
        BitConverter.ToUInt32(buffer, offset);
}
