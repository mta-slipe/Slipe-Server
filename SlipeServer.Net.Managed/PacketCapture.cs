using System.Globalization;
using System.Net;
using System.Text;

namespace SlipeServer.Net.Managed;

/// <summary>Which way a captured datagram travelled.</summary>
public enum CaptureDirection
{
    Rx,
    Tx,
}

/// <summary>
/// Append-only on-disk recorder for every datagram that crosses the socket, in both directions.
///
/// Intended for diagnosis against a real MTA:SA client: it is the only durable record of the wire.
/// The capture is written <em>before</em> any handler runs, so a datagram that later fails to
/// parse -- or throws -- is still on disk with its raw bytes.
///
/// The format is one record per line and deliberately boring to parse:
///
/// <code>
/// # mtanet-capture/1
///      1 RX 2026-10-17T10:53:01.123Z 192.168.178.30:57186 34 0904... | route=offline id=0x09 ...
/// </code>
///
/// Fields are <c>seq dir utc endpoint bytes hex | classification</c>. The classification is
/// produced by <see cref="CaptureDescribe.Route"/> from the same values the router sees, so the
/// line records the routing decision and the parse verdict next to the bytes that caused them.
/// </summary>
public sealed class PacketCapture : IDisposable
{
    /// <summary>Bumped only if the line grammar changes.</summary>
    public const string FormatVersion = "mtanet-capture/1";

    private readonly object gate = new();
    private readonly StreamWriter writer;
    private long recordCount;
    private bool disposed;

    public PacketCapture(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        this.Path = System.IO.Path.GetFullPath(path);

        string? directory = System.IO.Path.GetDirectoryName(this.Path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var stream = new FileStream(this.Path, FileMode.Append, FileAccess.Write, FileShare.Read);
        this.writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
        {
            AutoFlush = true,
        };

        if (stream.Length == 0)
        {
            this.writer.WriteLine($"# {FormatVersion}");
            this.writer.WriteLine("# seq dir utc endpoint bytes hex | classification");
        }
    }

    /// <summary>The absolute path this capture appends to.</summary>
    public string Path { get; }

    public long RecordCount
    {
        get
        {
            lock (this.gate)
                return this.recordCount;
        }
    }

    /// <summary>
    /// Records one datagram. <paramref name="byteLength"/> is honoured rather than
    /// <c>datagram.Length</c>, because the receive buffer is a reused 1492-byte array.
    /// </summary>
    public void Record(
        CaptureDirection direction,
        IPEndPoint? remote,
        byte[] datagram,
        int byteLength,
        string classification)
    {
        if (datagram is null)
            return;

        int length = Math.Min(byteLength, datagram.Length);
        if (length <= 0)
            return;

        string hex = Convert.ToHexString(datagram, 0, length);
        string endpoint = remote is null ? "-" : $"{remote.Address}:{remote.Port}";
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

        lock (this.gate)
        {
            if (this.disposed)
                return;

            this.recordCount++;
            this.writer.WriteLine(FormattableString.Invariant(
                $"{this.recordCount,6} {(direction == CaptureDirection.Rx ? "RX" : "TX")} {timestamp} {endpoint} {length} {hex} | {classification}"));
        }
    }

    /// <summary>A free-form annotation line, used for session lifecycle events.</summary>
    public void Note(string text)
    {
        lock (this.gate)
        {
            if (this.disposed)
                return;

            this.writer.WriteLine($"# note {text}");
        }
    }

    public void Dispose()
    {
        lock (this.gate)
        {
            if (this.disposed)
                return;

            this.disposed = true;
            this.writer.Flush();
            this.writer.Dispose();
        }
    }
}

/// <summary>
/// Renders the router's decision and the datagram parse result as the capture's trailing field.
/// This is the part that turns a hex blob into an answer: it says whether the magic geometry
/// matched, and -- when it did not -- why.
/// </summary>
public static class CaptureDescribe
{
    public static string RakNetIdName(byte id) => MtaProtocol.RidName(id);

    /// <summary>
    /// The routing verdict for one received datagram: <c>route=offline</c>, <c>route=connected</c>
    /// or <c>route=drop</c>, followed by the evidence behind it.
    /// </summary>
    public static string Route(byte[] datagram, int byteLength, bool hasSessionRecord)
    {
        if (datagram is null)
            return "route=drop null";

        int length = Math.Min(byteLength, datagram.Length);
        if (length < 3)
            return $"route=drop too-short len={length}";

        byte id = datagram[0];
        string name = RakNetIdName(id);

        if (MtaProtocol.HasMagicGeometry(id))
        {
            MtaProtocol.MagicGeometry geometry = MtaProtocol.GetMagicGeometry(id);
            bool magicOk = MtaProtocol.TryValidateMagic(datagram, length, id, out _);

            var text = new StringBuilder();
            text.Append(FormattableString.Invariant(
                $"id=0x{id:X2} {name} magic@{geometry.Offset} {(magicOk ? "ok" : "MISMATCH")} len={length}"));

            if (geometry.ExpectedLength != 0 && geometry.ExpectedLength != length)
                text.Append(FormattableString.Invariant($" expected={geometry.ExpectedLength}"));

            if (magicOk)
                return $"route=offline {text}";

            // A packed reliability header can collide with an offline message id: 0x5F is both
            // RID_INCOMPATIBLE_PROTOCOL_VERSION and a legal first header byte, and a connected
            // RID_INTERNAL_PING of 23 bytes reads as one. A datagram shorter than the id's fixed frame
            // length cannot be that message at all, so it is a header, not a bad magic. Longer frames
            // stay below: they are structurally possible but mis-sized, which is worth reporting.
            if (geometry.ExpectedLength != 0 && length < geometry.ExpectedLength)
            {
                string misrouted = hasSessionRecord ? "connected" : "drop";
                return $"route={misrouted} first=0x{id:X2} len={length} {DescribeDatagram(datagram, length)}";
            }

            // A geometry id whose magic fails falls through to the reliability layer when the peer
            // already has a session record, and is dropped otherwise.
            string fallback = hasSessionRecord ? "connected" : "drop";
            return $"route={fallback} {text} {DescribeDatagram(datagram, length)}";
        }

        // A non-geometry first byte on the connected path is a datagram header, not a message id,
        // so it is reported as raw bits rather than being mislabelled with an RID_ name.
        string route = hasSessionRecord ? "connected" : "drop";
        return $"route={route} first=0x{id:X2} len={length} {DescribeDatagram(datagram, length)}";
    }

    /// <summary>
    /// The classification for a datagram we are about to send. Mirrors <see cref="Route"/>'s
    /// offline test so the capture distinguishes an offline reply from a reliability-layer
    /// datagram without the reader having to guess from the first byte.
    /// </summary>
    public static string Outbound(byte[] datagram, int byteLength)
    {
        if (datagram is null)
            return "tx=null";

        int length = Math.Min(byteLength, datagram.Length);
        if (length < 3)
            return $"tx=too-short len={length}";

        byte id = datagram[0];

        if (MtaProtocol.HasMagicGeometry(id) && MtaProtocol.TryValidateMagic(datagram, length, id, out _))
            return $"tx=offline id=0x{id:X2} {RakNetIdName(id)} len={length}";

        // The first byte of a reliability-layer datagram is a header bit ladder, not a message id.
        return $"tx=datagram first=0x{id:X2} len={length} {DescribeDatagram(datagram, length)}";
    }

    /// <summary>
    /// Renders the RakNet datagram ladder's result, including each packet's wire id. When the
    /// ladder rejects the bytes, the rejection reason is recorded instead of being lost.
    /// </summary>
    public static string DescribeDatagram(byte[] datagram, int byteLength)
    {
        if (!RakNetDatagram.TryParse(datagram, byteLength, out RakNetDatagram parsed, out string error))
            return $"unparsed=\"{error}\"";

        var text = new StringBuilder();
        text.Append(FormattableString.Invariant(
            $"datagram acks={parsed.Acks.Count} pkts={parsed.Packets.Count} second={parsed.HasSecondField}"));

        for (int i = 0; i < parsed.Packets.Count; i++)
        {
            RakNetPacketRecord packet = parsed.Packets[i];
            byte wireId = packet.Payload.Length > 0 ? packet.Payload[0] : (byte)0;

            // Split fields are only present when the split bit is set, so non-split classifications
            // carry no split tokens.
            string split = packet.HasSplitData
                ? FormattableString.Invariant(
                    $" split={packet.SplitPacketIndex}/{packet.SplitPacketCount} sid={packet.SplitPacketId}")
                : string.Empty;

            text.Append(FormattableString.Invariant(
                $" [p{i} wire=0x{wireId:X2} seq={packet.SequenceNumber} rel={packet.Reliability} ch={packet.OrderingChannel} oi={packet.OrderingIndex}{split} bits={packet.BitCount} bytes={packet.Payload.Length}]"));
        }

        return text.ToString();
    }
}

/// <summary>One parsed capture line.</summary>
public sealed record CaptureRecord(
    long Sequence,
    CaptureDirection Direction,
    string Timestamp,
    string Endpoint,
    int ByteLength,
    string Hex,
    string Classification);

/// <summary>
/// Reads a capture back and reports it as a grouped table. This is what makes a user-supplied
/// capture answerable without the live socket: identical retries collapse into one row with a
/// count, so "the client sent this eleven times" is visible at a glance.
/// </summary>
public static class CaptureDecoder
{
    /// <summary>
    /// Tokens after which a classification stops being discriminating: they carry per-datagram
    /// values (lengths, sequence numbers) that would otherwise split every group into singletons.
    /// </summary>
    private static readonly string[] VolatileTokens = { "len=", "expected=", "datagram", "unparsed=" };

    public static CaptureRecord? TryParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line) || line[0] == '#')
            return null;

        int bar = line.IndexOf('|');
        string head = bar >= 0 ? line[..bar] : line;
        string classification = bar >= 0 ? line[(bar + 1)..].Trim() : string.Empty;

        string[] fields = head.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length < 6)
            return null;

        if (!long.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long sequence))
            return null;

        CaptureDirection direction = fields[1] switch
        {
            "RX" => CaptureDirection.Rx,
            "TX" => CaptureDirection.Tx,
            _ => (CaptureDirection)(-1),
        };

        if (!Enum.IsDefined(direction))
            return null;

        if (!int.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int length))
            return null;

        return new CaptureRecord(sequence, direction, fields[2], fields[3], length, fields[5], classification);
    }

    /// <summary>
    /// Writes a human-readable report: totals, the per-endpoint split, one row per distinct
    /// classification, and the first sample bytes of each. Returns the number of records read.
    /// </summary>
    public static int Decode(string path, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (!File.Exists(path))
        {
            output.WriteLine($"capture not found: {path}");
            return 0;
        }

        List<CaptureRecord> records = new();
        int malformed = 0;

        foreach (string line in File.ReadAllLines(path))
        {
            CaptureRecord? record = TryParseLine(line);
            if (record is null)
            {
                if (!string.IsNullOrWhiteSpace(line) && line[0] != '#')
                    malformed++;
                continue;
            }

            records.Add(record);
        }

        output.WriteLine($"capture   : {Path.GetFullPath(path)}");
        output.WriteLine($"records   : {records.Count} (rx {records.FindAll(r => r.Direction == CaptureDirection.Rx).Count}, " +
                         $"tx {records.FindAll(r => r.Direction == CaptureDirection.Tx).Count})" +
                         (malformed > 0 ? $", {malformed} malformed line(s)" : string.Empty));

        if (records.Count == 0)
            return 0;

        output.WriteLine($"window    : {records[0].Timestamp} .. {records[^1].Timestamp}");

        output.WriteLine();
        output.WriteLine("endpoints:");
        var endpoints = new SortedDictionary<string, (int Rx, int Tx)>(StringComparer.Ordinal);
        foreach (CaptureRecord record in records)
        {
            endpoints.TryGetValue(record.Endpoint, out (int Rx, int Tx) counts);
            endpoints[record.Endpoint] = record.Direction == CaptureDirection.Rx
                ? (counts.Rx + 1, counts.Tx)
                : (counts.Rx, counts.Tx + 1);
        }

        foreach (KeyValuePair<string, (int Rx, int Tx)> entry in endpoints)
            output.WriteLine($"  {entry.Key,-24} rx {entry.Value.Rx,4}  tx {entry.Value.Tx,4}");

        output.WriteLine();
        output.WriteLine("groups (dir, signature, count, observed lengths, first sample):");

        var groups = new List<GroupRow>();
        var index = new Dictionary<string, GroupRow>(StringComparer.Ordinal);

        foreach (CaptureRecord record in records)
        {
            string signature = Signature(record.Classification);
            string key = (record.Direction == CaptureDirection.Rx ? "RX " : "TX ") + signature;

            if (index.TryGetValue(key, out GroupRow? group))
            {
                group.Count++;
                group.Lengths.Add(record.ByteLength);
                continue;
            }

            group = new GroupRow(record.Direction, signature, record.Hex, record.Classification);
            group.Lengths.Add(record.ByteLength);
            index[key] = group;
            groups.Add(group);
        }

        foreach (GroupRow group in groups)
        {
            output.WriteLine();
            output.WriteLine($"  {(group.Direction == CaptureDirection.Rx ? "RX" : "TX")} x{group.Count}  {group.Signature}");
            output.WriteLine($"      bytes: {string.Join(",", group.Lengths)}");
            output.WriteLine($"      full : {group.Classification}");
            output.WriteLine($"      first: {Sample(group.Hex, 48)}");
        }

        return records.Count;
    }

    /// <summary>
    /// The stable part of a classification: everything up to the first per-datagram token. For
    /// <c>route=connected id=0x04 RID_CONNECTION_REQUEST magic@1 MISMATCH len=33 expected=33</c>
    /// this is <c>route=connected id=0x04 RID_CONNECTION_REQUEST magic@1 MISMATCH</c>.
    /// </summary>
    public static string Signature(string classification)
    {
        if (string.IsNullOrEmpty(classification))
            return "<none>";

        var kept = new List<string>();
        foreach (string token in classification.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            bool isVolatile = false;
            foreach (string marker in VolatileTokens)
            {
                if (token.StartsWith(marker, StringComparison.Ordinal))
                {
                    isVolatile = true;
                    break;
                }
            }

            if (isVolatile)
                break;

            kept.Add(token);
        }

        return kept.Count == 0 ? "<none>" : string.Join(' ', kept);
    }

    private static string Sample(string hex, int maxChars) =>
        hex.Length <= maxChars ? hex : hex[..maxChars] + "...";

    private sealed class GroupRow
    {
        public GroupRow(CaptureDirection direction, string signature, string hex, string classification)
        {
            this.Direction = direction;
            this.Signature = signature;
            this.Hex = hex;
            this.Classification = classification;
        }

        public CaptureDirection Direction { get; }

        public string Signature { get; }

        public string Hex { get; }

        public string Classification { get; }

        public SortedSet<int> Lengths { get; } = new();

        public int Count { get; set; } = 1;
    }
}
