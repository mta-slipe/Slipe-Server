using System.Net;
using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the on-disk capture format and the routing verdicts it records. These tests exist because
/// the capture is the only artefact a real-client failure leaves behind: if the format or the
/// verdict drifts, the diagnosis that depends on it silently becomes wrong.
/// </summary>
public class PacketCaptureTests : IDisposable
{
    private readonly string directory;

    public PacketCaptureTests()
    {
        this.directory = Path.Combine(Path.GetTempPath(), "mtanet-tests-" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(this.directory))
                Directory.Delete(this.directory, recursive: true);
        }
        catch (IOException)
        {
            // A leaked temp directory is not worth failing a test run over.
        }
    }

    private static readonly IPEndPoint Remote = new(IPAddress.Parse("192.0.2.10"), 57186);

    private static readonly byte[] ClientGuid =
    {
        0x4D, 0x74, 0x61, 0x4E, 0x65, 0x74, 0x50, 0x6F,
        0x63, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x31,
    };

    private static byte[] OpenConnectionRequest()
    {
        var datagram = new byte[34];
        datagram[0] = MtaProtocol.Rid.OpenConnectionRequest;
        datagram[1] = 0x04;
        Array.Copy(ClientGuid, 0, datagram, 2, 16);
        Array.Copy(MtaProtocol.OfflineMagic, 0, datagram, 18, 16);
        return datagram;
    }

    /// <summary>A 33-byte id-4 frame whose magic is deliberately wrong.</summary>
    private static byte[] ConnectionRequestWithBadMagic()
    {
        var datagram = new byte[33];
        datagram[0] = MtaProtocol.Rid.ConnectionRequest;
        Array.Copy(MtaProtocol.OfflineMagic, 0, datagram, 1, 16);
        datagram[5] ^= 0xFF;
        return datagram;
    }

    [Fact]
    public void Capture_IsAppendOnlyAndReportsItsResolvedPath()
    {
        string path = Path.Combine(this.directory, "nested", "capture.log");

        using (var capture = new PacketCapture(path))
        {
            Assert.Equal(Path.GetFullPath(path), capture.Path);
            Assert.True(Directory.Exists(Path.GetDirectoryName(capture.Path)));

            capture.Record(CaptureDirection.Rx, Remote, OpenConnectionRequest(), 34, "route=offline id=0x09");
            capture.Record(CaptureDirection.Tx, Remote, new byte[] { 0x0A, 0x00, 0xFF }, 3, "tx=offline id=0x0A");
            Assert.Equal(2, capture.RecordCount);
        }

        using (var capture = new PacketCapture(path))
        {
            // A second session appends rather than truncating, so a re-run does not erase evidence.
            capture.Record(CaptureDirection.Rx, Remote, new byte[] { 0x04, 0x01, 0x02 }, 3, "route=connected");
            Assert.Equal(1, capture.RecordCount);
        }

        string[] lines = File.ReadAllLines(path);
        Assert.Equal(PacketCapture.FormatVersion, lines[0].TrimStart('#', ' '));
        Assert.Equal(2, lines.Count(line => line.StartsWith('#'))); // the header is written once
        Assert.Equal(5, lines.Length); // two header lines plus three records
    }

    [Fact]
    public void Capture_HonoursByteLengthNotArrayLength()
    {
        // The socket path reuses one 1492-byte receive buffer, so the array is nearly always
        // longer than the datagram. Recording datagram.Length would write trailing zeroes.
        var buffer = new byte[1492];
        buffer[0] = 0x09;
        buffer[1] = 0x04;
        buffer[2] = 0xAB;

        string path = Path.Combine(this.directory, "capture.log");
        using (var capture = new PacketCapture(path))
            capture.Record(CaptureDirection.Rx, Remote, buffer, byteLength: 3, classification: "route=offline");

        CaptureRecord? record = CaptureDecoder.TryParseLine(File.ReadAllLines(path)[2]);
        Assert.NotNull(record);
        Assert.Equal(3, record!.ByteLength);
        Assert.Equal("0904AB", record.Hex);
    }

    [Fact]
    public void Capture_RoundTripsEveryField()
    {
        string path = Path.Combine(this.directory, "capture.log");
        byte[] datagram = OpenConnectionRequest();

        using (var capture = new PacketCapture(path))
            capture.Record(CaptureDirection.Rx, Remote, datagram, datagram.Length, CaptureDescribe.Route(datagram, datagram.Length, hasSessionRecord: false));

        CaptureRecord? record = CaptureDecoder.TryParseLine(File.ReadAllLines(path)[2]);

        Assert.NotNull(record);
        Assert.Equal(1, record!.Sequence);
        Assert.Equal(CaptureDirection.Rx, record.Direction);
        Assert.Equal("192.0.2.10:57186", record.Endpoint);
        Assert.Equal(34, record.ByteLength);
        Assert.Equal(Convert.ToHexString(datagram), record.Hex);
        Assert.EndsWith("Z", record.Timestamp);
        Assert.Contains("route=offline", record.Classification);
    }

    [Fact]
    public void TryParseLine_RejectsHeadersAndGarbage()
    {
        Assert.Null(CaptureDecoder.TryParseLine("# mtanet-capture/1"));
        Assert.Null(CaptureDecoder.TryParseLine(string.Empty));
        Assert.Null(CaptureDecoder.TryParseLine("not a record"));
        Assert.Null(CaptureDecoder.TryParseLine("1 XX 2026-10-17T10:53:01.123Z 127.0.0.1:1 4 00000000 | x"));
    }

    [Fact]
    public void Decoder_CollapsesARetryLoopIntoOneGroup()
    {
        // The real client's failure mode is "the same thing eleven times, then a new source port".
        // The decoder must surface that as one row with a count, not eleven rows.
        string path = Path.Combine(this.directory, "capture.log");
        byte[] datagram = ConnectionRequestWithBadMagic();

        using (var capture = new PacketCapture(path))
        {
            for (int i = 0; i < 11; i++)
            {
                string classification = CaptureDescribe.Route(datagram, datagram.Length, hasSessionRecord: true);
                capture.Record(CaptureDirection.Rx, Remote, datagram, datagram.Length, classification);
            }
        }

        var output = new StringWriter();
        Assert.Equal(11, CaptureDecoder.Decode(path, output));

        string report = output.ToString();
        Assert.Contains("RX x11", report);
        Assert.DoesNotContain("RX x1 ", report);
    }

    [Fact]
    public void Signature_StopsAtTheFirstPerDatagramToken()
    {
        Assert.Equal(
            "route=connected id=0x04 RID_CONNECTION_REQUEST magic@1 MISMATCH",
            CaptureDecoder.Signature(
                "route=connected id=0x04 RID_CONNECTION_REQUEST magic@1 MISMATCH len=33 expected=33 datagram acks=0"));

        Assert.Equal("<none>", CaptureDecoder.Signature(string.Empty));
        Assert.Equal("route=offline", CaptureDecoder.Signature("route=offline len=34"));
    }

    [Fact]
    public void Route_SaysOfflineForAValidOpenConnectionRequest()
    {
        byte[] datagram = OpenConnectionRequest();
        string route = CaptureDescribe.Route(datagram, datagram.Length, hasSessionRecord: false);

        Assert.StartsWith("route=offline", route);
        Assert.Contains("RID_OPEN_CONNECTION_REQUEST", route);
        Assert.Contains("magic@18 ok", route);
    }

    [Fact]
    public void Route_FallsThroughToConnectedWhenAnIdHasBadMagicAndASessionExists()
    {
        byte[] datagram = ConnectionRequestWithBadMagic();
        string route = CaptureDescribe.Route(datagram, datagram.Length, hasSessionRecord: true);

        Assert.StartsWith("route=connected", route);
        Assert.Contains("RID_CONNECTION_REQUEST", route);
        Assert.Contains("magic@1 MISMATCH", route);

        // The declared length is reported only when it disagrees with the id's fixed geometry, so a
        // wrong-length frame from a real client is immediately visible.
        Assert.DoesNotContain("expected=", route);

        var padded = new byte[40];
        Array.Copy(datagram, padded, datagram.Length);
        Assert.Contains("expected=33", CaptureDescribe.Route(padded, padded.Length, hasSessionRecord: true));
    }

    [Fact]
    public void Route_DropsTheSameDatagramWithoutASession()
    {
        byte[] datagram = ConnectionRequestWithBadMagic();
        Assert.StartsWith("route=drop", CaptureDescribe.Route(datagram, datagram.Length, hasSessionRecord: false));
    }

    [Fact]
    public void Route_ReportsShortDatagramsWithoutParsingThem()
    {
        Assert.Equal("route=drop too-short len=2", CaptureDescribe.Route(new byte[] { 0x04, 0x00 }, 2, false));
    }

    [Fact]
    public void Route_DoesNotMislableADatagramHeaderAsAMessageId()
    {
        // A reliability-layer datagram's first byte is the header bit ladder. Naming it after an
        // RID_ constant would send the reader looking for a handler that does not exist.
        byte[] datagram = { 0x00, 0x40, 0x00, 0x00, 0x18, 0x00, 0x40, 0x00, 0x00, 0x10, 0x80, 0x7A, 0xA8 };
        string route = CaptureDescribe.Route(datagram, datagram.Length, hasSessionRecord: true);

        Assert.StartsWith("route=connected first=0x00", route);
        Assert.DoesNotContain("RID_INTERNAL_PING", route);
    }

    [Fact]
    public void Outbound_DistinguishesAnOfflineReplyFromADatagram()
    {
        var reply = new byte[33];
        reply[0] = MtaProtocol.Rid.OpenConnectionReply;
        Array.Copy(MtaProtocol.OfflineMagic, 0, reply, 1, 16);

        Assert.StartsWith("tx=offline id=0x0A", CaptureDescribe.Outbound(reply, reply.Length));
        Assert.StartsWith("tx=too-short", CaptureDescribe.Outbound(new byte[] { 0x01, 0x02 }, 2));
    }

    [Fact]
    public void TryValidateMagic_HonoursByteLengthOverArrayLength()
    {
        // Regression: the socket reuses a 1492-byte buffer, so datagram.Length is not the datagram
        // length. Every id with a fixed expected length used to fail the geometry guard because of it.
        var buffer = new byte[1492];
        byte[] request = OpenConnectionRequest();
        Array.Copy(request, buffer, request.Length);

        Assert.True(MtaProtocol.TryValidateMagic(buffer, request.Length, MtaProtocol.Rid.OpenConnectionRequest, out _));
        Assert.False(MtaProtocol.TryValidateMagic(buffer, request.Length, MtaProtocol.Rid.ConnectionRequest, out _));
    }

    [Fact]
    public void Route_DoesNotReadAPackedHeaderAsAnOfflineMessageId()
    {
        // Regression: a real client's RID_INTERNAL_PING datagram starts with the packed header byte 0x5F,
        // which is also RID_INCOMPATIBLE_PROTOCOL_VERSION (a 34-byte offline frame). The decoder used to
        // report it as "id=0x5F ... magic@2 MISMATCH len=23 expected=34" instead of a ping.
        byte[] ping = Convert.FromHexString("5F018C0000000000014000001248007006300000000000");

        string route = CaptureDescribe.Route(ping, ping.Length, hasSessionRecord: true);

        Assert.StartsWith("route=connected first=0x5F", route);
        Assert.Contains("wire=0x00", route);
        Assert.DoesNotContain("MISMATCH", route);
        Assert.DoesNotContain("RID_INCOMPATIBLE_PROTOCOL_VERSION", route);
    }

    /// <summary>
    /// A split part is invisible in every other view of the wire: it parses cleanly, it
    /// carries a sequence number, and it looks like an ordinary short packet whose payload happens
    /// to be nonsense. The capture has to say <c>split=index/count sid=id</c>, or a reader
    /// diagnosing a fragmented message cannot tell fragmentation from corruption.
    /// </summary>
    /// <remarks>
    /// The datagram is the second of two parts: one payload byte, split index 1 of 2, split id
    /// 0x1234, sequence 101, reliable-ordered on channel 0 with ordering index 7.
    /// </remarks>
    [Fact]
    public void ASplitPartIsReportedWithItsIndexCountAndId()
    {
        byte[] part = Convert.FromHexString("194000001801C0000026825E3E5C0074");

        string description = CaptureDescribe.DescribeDatagram(part, part.Length);

        Assert.Contains("split=1/2 sid=4660", description);
        Assert.Contains("seq=101", description);
        Assert.Contains("oi=7", description);
    }

    /// <summary>
    /// The split suffix is additive: a datagram without the split bit must render exactly as it did
    /// before the suffix existed, or every classification in every existing capture would change
    /// and <c>--decode</c> grouping would split every group into singletons.
    /// </summary>
    [Fact]
    public void ANonSplitDatagramRendersWithoutASplitSuffix()
    {
        byte[] ping = Convert.FromHexString("5F018C0000000000014000001248007006300000000000");

        string description = CaptureDescribe.DescribeDatagram(ping, ping.Length);

        Assert.StartsWith("datagram acks=0 pkts=1", description);
        Assert.DoesNotContain("split=", description);
    }
}
