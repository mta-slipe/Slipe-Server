using SlipeServer.Net.Managed;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Byte literals transcribed from <c>capture.log</c> (two real MTA:SA client sessions recorded
/// against the stock server) and from byte layouts worked out by hand.
///
/// <para>
/// Every array here is an <em>independent</em> literal. None of them was produced by
/// <c>BitStream</c>, <c>RakNetDatagramWriter</c>, <c>MtaGamePackets</c> or any other part of the
/// port. That is what makes a test built on them evidence about the wire rather than a restatement
/// of the port's own reading — see the header of <c>EndToEndWireTests.cs</c>.
/// </para>
///
/// <para>
/// To re-verify a vector, open <c>capture.log</c>, find the record number in the citation, and
/// compare its <c>hex</c> field. <c>CaptureVectorProvenanceTests</c> does exactly that mechanically,
/// so a transcription slip cannot survive the suite.
/// </para>
/// </summary>
internal static class CaptureVectors
{
    /// <summary>
    /// capture.log seq 1 — the real client's offline <c>RID_OPEN_CONNECTION_REQUEST</c> (51 bytes).
    /// Classification: <c>route=offline id=0x09 RID_OPEN_CONNECTION_REQUEST magic@18 ok len=51</c>.
    ///
    /// Layout <c>[0x09][0x04][client GUID : 16][magic : 16][TLV config]</c>. The magic at offset 18
    /// is exactly what the <c>9 | 0x0D → (0x12, 0, 16)</c> geometry row in <c>MtaProtocol</c>
    /// describes. The trailing TLV block is <c>ED CA0101 A8 04E8030000 C8 04E8030000 DE</c>.
    /// </summary>
    internal const string OpenConnectionRequest =
        "0904F0A5D2BAB4E48DEC4A29518967DEDC2100FF00FFFEFEFEFEFDFDFDFD78563417" +
        "EDCA0101A804E8030000C804E8030000DE";

    /// <summary>
    /// capture.log seq 2 — the stock server's 33-byte <c>RID_OPEN_CONNECTION_REPLY</c>.
    /// Layout <c>[0x0A][magic : 16][server GUID : 16]</c>. Bytes 0-16 are the fixed shape; bytes
    /// 17-32 are that server's GUID, which this port replaces with its own.
    /// </summary>
    internal const string OpenConnectionReplyStock =
        "0A00FF00FFFEFEFEFEFDFDFDFD78563417" +
        "3051DD07BB214737861AF1746894BCEF";

    /// <summary>
    /// capture.log seq 3 — the real client's connected <c>RID_CONNECTION_REQUEST</c> (48-byte
    /// datagram). Classification: <c>route=connected first=0x4E len=48 datagram acks=0 pkts=1
    /// second=True [p0 wire=0x04 seq=0 rel=2 ch=0 oi=0 bits=264 bytes=33]</c>.
    ///
    /// The 33-byte payload is <c>[0x04][magic : 16][client GUID : 16]</c> with no password bytes.
    /// </summary>
    internal const string ConnectedConnectionRequest =
        "4E100CC000000000000000001010020400FF00FFFEFEFEFEFDFDFDFD78563417" +
        "F0A5D2BAB4E48DEC4A29518967DEDC21";

    /// <summary>
    /// capture.log seq 4 — the stock server's reply datagram (89 bytes). Classification:
    /// <c>datagram acks=1 pkts=1 second=False [p0 wire=0x0E seq=0 rel=2 ch=0 oi=0 bits=552
    /// bytes=69]</c>.
    ///
    /// The first 20 bytes are the datagram header plus the ack block for sequence 0; the last 69
    /// are the <c>RID_CONNECTION_REQUEST_ACCEPTED</c> body.
    /// </summary>
    internal const string ConnectedReplyStock =
        "FABCC8838000000063000000000000000020A008" + ConnectedReplyBodyRecorded;

    /// <summary>
    /// The 69-byte body carried by <see cref="ConnectedReplyStock"/> (offset 20 onwards): <c>0E</c>
    /// id, <c>3F574DE1</c> = <c>~0x1EB2A8C0</c> (the client address dword, complemented as stock
    /// RakNet does so routers do not rewrite it), <c>C9E2</c> = 58057 (the client port),
    /// <c>0000</c> system index, then ten 6-byte address slots. Slots 1-9 are zero-filled, which
    /// serializes as <c>FFFFFFFF0000</c> = <c>255.255.255.255:0</c>.
    /// </summary>
    internal const string ConnectedReplyBodyRecorded =
        "0E3F574DE1C9E20000FFFFFFFF0000FFFFFFFF0000FFFFFFFF0000FFFFFFFF0000FF" +
        "FFFFFF0000FFFFFFFF0000FFFFFFFF0000FFFFFFFF0000FFFFFFFF0000FFFFFFFF00" +
        "00";

    /// <summary>
    /// The same body as the port produces it: identical to
    /// <see cref="ConnectedReplyBodyRecorded"/> except for the complemented address dword, which is
    /// <c>3FFFFDF5</c> = <c>~0x0A0200C0</c> for the endpoint the suite replays from.
    /// </summary>
    internal const string ConnectedReplyBodyStock =
        "0E3FFFFDF5C9E20000FFFFFFFF0000FFFFFFFF0000FFFFFFFF0000FFFFFFFF0000FF" +
        "FFFFFF0000FFFFFFFF0000FFFFFFFF0000FFFFFFFF0000FFFFFFFF0000FFFFFFFF00" +
        "00";

    /// <summary>
    /// A hand-built offline <c>RID_PING</c> (25 bytes) from the layout in
    /// <c>[0x01][client time : u64 LE][magic : 16]</c>, magic at
    /// offset 9, 15 bytes compared, total length 0x19.
    ///
    /// The client time is the sentinel <c>0x1122334455667788</c> so the pong echo is unmistakable on
    /// the wire. No capture.log record contains a ping, so this vector's authority is the documented
    /// layout rather than a recording.
    /// </summary>
    internal const string PingRequest =
        "01" + "8877665544332211" + "00FF00FFFEFEFEFEFDFDFDFD78563417";

    /// <summary>The client time carried by <see cref="PingRequest"/>.</summary>
    internal const ulong PingClientTime = 0x1122334455667788UL;

    /// <summary>
    /// A hand-built offline <c>RID_OPEN_CONNECTION_REQUEST</c> whose version byte is <c>0x03</c>
    /// instead of <c>0x04</c> — the config-failed arm that makes the stock server answer with the
    /// 34-byte <c>0x5F</c> refusal. Everything else matches
    /// <see cref="OpenConnectionRequest"/>.
    /// </summary>
    internal const string OpenConnectionRequestBadVersion =
        "0903F0A5D2BAB4E48DEC4A29518967DEDC2100FF00FFFEFEFEFEFDFDFDFD78563417" +
        "EDCA0101A804E8030000C804E8030000DE";

    /// <summary>
    /// capture.log seq 6 — two packets in one datagram from the real client: the connected
    /// <c>RID_NEW_INCOMING_CONNECTION</c> (wire <c>0x11</c>, mtaId 3) followed by a RakNet system
    /// packet (wire <c>0x00</c>) that must produce no <c>PacketReceived</c> at all.
    /// </summary>
    internal const string IncomingConnectionAndSystemPacket =
        "5A100CC000000000004000001800000000018020113F574DA7EAC53F574DE1C9E20000" +
        "0000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF00" +
        "000000FFFF00000000FFFF00000000FFFF020000004920004D40330000000000";

    /// <summary>
    /// capture.log seq 20 — a 47-byte datagram whose first byte is <c>0x5F</c>, so the offline
    /// router's geometry check fires but the length does not match the 34-byte expectation
    /// (<c>magic@2 MISMATCH len=47 expected=34</c>). It must route to the connected path rather than
    /// be refused. Packet: <c>wire=0x83 seq=9 rel=3 ch=0 oi=4 bits=224 bytes=28</c> → mtaId 32.
    /// </summary>
    internal const string RpcPacketWire83 =
        "5FD18CC000000000024000001801000000170083000000000018000000000000000000" +
        "0001006400008807D13B0000";

    /// <summary>
    /// capture.log seq 23 — <c>wire=0x7B seq=10 rel=3 ch=0 oi=5 bits=16 bytes=2</c> → mtaId 24,
    /// one-byte payload.
    /// </summary>
    internal const string RpcPacketWire7B = "63918CC00000000002800000180140000010807B01";

    /// <summary>
    /// capture.log seq 18 — two <c>wire=0x7B</c> packets in one datagram
    /// (<c>seq=7 bits=16 bytes=2</c> and <c>seq=8 bits=24 bytes=3</c>) → two mtaId-24 events.
    /// </summary>
    internal const string TwoWire7BPackets =
        "5C518CC00000000001C00000180080000010807B0008000000600300000043007B0300";

    /// <summary>
    /// capture.log seq 303 — the client's <c>RID_DISCONNECTION_NOTIFICATION</c>
    /// (<c>wire=0x13 seq=109 rel=3 ch=0 oi=97 bits=8 bytes=1</c>) → mtaId 5 after the
    /// <c>0x13 ⇄ 4</c> swap and the <c>-99</c> offset. The last recorded record.
    /// </summary>
    internal const string DisconnectionNotification = "671C0CC0000000001B40000018184000001C0013";

    /// <summary>
    /// capture.log seq 14 — the stock server's 112-byte <c>SERVER_JOIN_COMPLETE</c> datagram: the
    /// packet-2 leading block exactly as the real server wrote it. Classification
    /// <c>datagram acks=0 pkts=1 second=False [p0 wire=0x65 seq=2 rel=3 ch=0 oi=1 bits=800
    /// bytes=100]</c>.
    ///
    /// This is the golden vector for the packet-2 write order.
    /// Datagram header (12 bytes) = <c>00 800000 1800 4000 0002 0030</c>: no acks, no second field,
    /// sequence 2, reliability 3, channel 0, ordering index 1, split flag clear, compressed bit
    /// count 800.
    /// </summary>
    internal const string ServerJoinCompleteStock =
        "008000001800400000020030" + ServerJoinCompletePayloadStock;

    /// <summary>
    /// The 100-byte payload of <see cref="ServerJoinCompleteStock"/>, decoded field by field:
    /// <c>65</c> wire id; <c>00D0D190</c> = 0x90D1D000 AC-disable bitmap low; <c>1F000000</c> = 0x1F
    /// bitmap high; <c>00000000</c> image-mod flags; <c>2000</c> serial length 32; 32 serial bytes
    /// already shifted by one; <c>0000</c> adapter length 0; <c>FFFFFFFF</c>
    /// <c>iEnableClientChecks</c>; then the two length-prefixed strings. No <c>"HB95"</c> trailer,
    /// matching the row-9 gate.
    ///
    /// The bitmap values are byte-identical to <c>MtaGamePackets.ServerJoinCompleteOptions</c>'s
    /// defaults, so those defaults are the stock server's values rather than a guess.
    /// </summary>
    internal const string ServerJoinCompletePayloadStock =
        "6500D0D1901F000000000000002000" +
        "0101010101010101010101010101010101010101" +
        "10010101C9C2492B3F4F626F" +
        "0000FFFFFFFF2000" +
        "536C6970652053657276657220302E312E30205B57696E646F7773205836345D" +
        "0B00312E362E302D392E302E30";

    /// <summary>The connection text carried by <see cref="ServerJoinCompletePayloadStock"/>.</summary>
    internal const string StockConnectionText = "Slipe Server 0.1.0 [Windows X64]";

    /// <summary>The sortable server version carried by <see cref="ServerJoinCompletePayloadStock"/>.</summary>
    internal const string StockServerVersion = "1.6.0-9.0.0";

    /// <summary>
    /// The mod-level body of <see cref="ServerJoinCompletePayloadStock"/> — the two length-prefixed
    /// strings a Slipe <c>JoinCompletePacket</c> supplies, and the part that survives the leading block
    /// untouched. Sliced from the recording rather than
    /// re-encoded, so it stays an independent literal.
    /// </summary>
    internal const string ServerJoinCompleteBody =
        "2000" +
        "536C6970652053657276657220302E312E30205B57696E646F7773205836345D" +
        "0B00312E362E302D392E302E30";

    /// <summary>
    /// capture.log seq 10 — a stock-server 26-byte datagram carrying
    /// <c>wire=0x6A seq=1 rel=3 ch=0 oi=0 bits=120 bytes=15</c>: mtaId 7, the "deathamatch" text
    /// packet. A real datagram with neither an ack block nor a second field.
    /// </summary>
    internal const string TextPacketStock =
        "00400000180000000013C0" + TextPacketPayloadStock;

    /// <summary>
    /// The 15-byte payload of <see cref="TextPacketStock"/>:
    /// <c>[0x6A][9000][0A00]"deathamatch"</c>.
    /// </summary>
    internal const string TextPacketPayloadStock = "6A90000A0064656174686D61746368";

    // ---------------------------------------------------------------- accessors

    internal static byte[] Bytes(string hex) => WireTestKit.Hex(hex);

    internal static byte[] OpenRequestBytes => Bytes(OpenConnectionRequest);

    internal static byte[] OpenReplyStockBytes => Bytes(OpenConnectionReplyStock);

    internal static byte[] ConnectedRequestBytes => Bytes(ConnectedConnectionRequest);

    internal static byte[] ConnectedReplyStockBytes => Bytes(ConnectedReplyStock);

    internal static byte[] ConnectedReplyBodyStockBytes => Bytes(ConnectedReplyBodyStock);

    internal static byte[] PingRequestBytes => Bytes(PingRequest);

    internal static byte[] RpcPacketWire83Bytes => Bytes(RpcPacketWire83);

    internal static byte[] DisconnectionNotificationBytes => Bytes(DisconnectionNotification);

    internal static byte[] ServerJoinCompleteStockBytes => Bytes(ServerJoinCompleteStock);

    internal static byte[] ServerJoinCompletePayloadStockBytes => Bytes(ServerJoinCompletePayloadStock);

    internal static byte[] ServerJoinCompleteBodyBytes => Bytes(ServerJoinCompleteBody);

    internal static byte[] TextPacketStockBytes => Bytes(TextPacketStock);

    internal static byte[] TextPacketPayloadStockBytes => Bytes(TextPacketPayloadStock);

    /// <summary>
    /// The server GUID that produced <see cref="OpenConnectionReplyStock"/>, used to prove the port
    /// substitutes its own GUID into the same 33-byte shape rather than echoing the recording.
    /// </summary>
    internal static byte[] StockServerGuid => Bytes("3051DD07BB214737861AF1746894BCEF");

    /// <summary>The client GUID carried by <see cref="OpenConnectionRequest"/> and the connected request.</summary>
    internal static byte[] ClientGuid => Bytes("F0A5D2BAB4E48DEC4A29518967DEDC21");

    /// <summary>The 16-byte modified magic, for tests that need to build a datagram around it.</summary>
    internal static byte[] Magic => Bytes("00FF00FFFEFEFEFEFDFDFDFD78563417");
}
