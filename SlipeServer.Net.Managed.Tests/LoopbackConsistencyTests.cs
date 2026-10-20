using System.Net;
using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// A synthetic client that talks to <see cref="MtaNetWrapper"/> using the port's <em>own</em> writer:
/// <see cref="BitStream"/>, <see cref="RakNetDatagramWriter"/> and <see cref="MtaProtocol"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>This file is internal-consistency evidence only, and must never be described as compatibility
/// evidence.</b> Both sides of every assertion are the port: the client writes with the port's
/// encoder, the server reads with the port's decoder, so any misreading they share cancels out and
/// the test still passes. It proves the two halves agree with each other, not that a stock MTA:SA
/// client would accept either. A loopback run is not compatibility evidence.
/// </para>
/// <para>
/// The real wire evidence lives in <see cref="EndToEndWireTests"/>, which feeds transcribed literals
/// in and never asks the port what the bytes should be.
/// </para>
/// </remarks>
public class LoopbackConsistencyTests
{
    private static readonly IPEndPoint Peer = WireTestKit.Endpoint("203.0.113.9:41000");

    private static readonly ulong PeerAddress = MtaNetWrapper.BinaryAddressOf(Peer);

    /// <summary>
    /// The full client lifecycle in one run: offline open-connection request, the connected
    /// <c>RID_CONNECTION_REQUEST</c>, a first game packet, then the server's packet 2 read back out of
    /// the captured bytes. Each step asserts the observable outcome of the previous one.
    /// </summary>
    [Fact]
    public void TheClientLifecycleCompletesAndPacketTwoRoundTrips()
    {
        using var harness = new WrapperHarness();
        ulong address = MtaNetWrapper.BinaryAddressOf(Peer);

        // 1. Offline RID_OPEN_CONNECTION_REQUEST: [0x09][0x04][client GUID 16][magic 16].
        byte[] clientGuid = WireTestKit.Hex("00112233445566778899AABBCCDDEEFF");
        byte[] openRequest = OfflineOpenConnectionRequest(clientGuid);
        harness.Inject(openRequest, Peer);

        byte[] openReply = harness.Tx[^1].Bytes;
        Assert.Equal(33, openReply.Length);
        Assert.Equal(MtaProtocol.Rid.OpenConnectionReply, openReply[0]);
        Assert.Equal(MtaProtocol.OfflineMagic, openReply[1..17]);
        Assert.Equal(harness.Wrapper.ServerGuid, openReply[17..33]);
        Assert.Equal(1, harness.Wrapper.SessionCount);

        // 2. Connected RID_CONNECTION_REQUEST (wire id 4) inside a RakNet datagram. The real client
        //    sends it reliable but not ordered (capture.log seq=3: rel=2), so it consumes no ordering
        //    index and the first ordered packet below still starts the channel at 0.
        byte[] connectRequest = ConnectedConnectionRequest(clientGuid);
        harness.Inject(Datagram(0, connectRequest, reliability: WireReliability.Reliable), Peer);

        Assert.Empty(harness.Received);
        byte[] connectReply = harness.Tx[^1].Bytes;
        RakNetDatagram.TryParse(connectReply, connectReply.Length, out RakNetDatagram replyDatagram, out string error);
        Assert.Equal(string.Empty, error);
        Assert.Single(replyDatagram.Packets);
        Assert.Equal(ConnectedHandshake.ReplyLength, replyDatagram.Packets[0].Payload.Length);
        Assert.Equal(MtaProtocol.Rid.ConnectionRequestAccepted, replyDatagram.Packets[0].Payload[0]);

        // 3. A first game packet, using the port's own wire-id mapping.
        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_PURESYNC);
        byte[] gamePayload = { wireId, 0xDE, 0xAD, 0xBE, 0xEF };
        harness.Inject(Datagram(1, gamePayload), Peer);

        Received delivered = Assert.Single(harness.Received);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_PURESYNC, delivered.Id);
        Assert.Equal(WireTestKit.HexOf(gamePayload[1..]), WireTestKit.HexOf(delivered.Data));
        Assert.Equal(address, delivered.Address);

        // 4. The server's own packet 2, read back out of the capture.
        harness.Wrapper.SendPacket(
            address,
            PacketId.PACKET_ID_SERVER_JOIN_COMPLETE,
            0x90,
            CaptureVectors.ServerJoinCompleteBodyBytes);

        RakNetDatagram.TryParse(harness.Tx[^1].Bytes, harness.Tx[^1].Bytes.Length, out RakNetDatagram sent, out error);
        Assert.Equal(string.Empty, error);
        Assert.Single(sent.Packets);

        byte[] packet2 = sent.Packets[0].Payload;
        Assert.Equal(MtaProtocol.ToWireId(MtaGamePackets.ServerJoinCompleteMtaId), packet2[0]);

        // The version-gated leading block sits between the wire id and the mod-level body, so the body
        // is the length-prefixed tail: u16 length + ASCII, twice, in the order the body was written.
        byte[] text = System.Text.Encoding.ASCII.GetBytes(CaptureVectors.StockConnectionText);
        byte[] version = System.Text.Encoding.ASCII.GetBytes(CaptureVectors.StockServerVersion);
        var expectedTail = new List<byte>();
        expectedTail.AddRange(BitConverter.GetBytes((ushort)text.Length));
        expectedTail.AddRange(text);
        expectedTail.AddRange(BitConverter.GetBytes((ushort)version.Length));
        expectedTail.AddRange(version);

        Assert.True(
            packet2.AsSpan().EndsWith(expectedTail.ToArray()),
            $"packet 2 must end with the join-complete body; got {WireTestKit.HexOf(packet2)}");
    }

    /// <summary>
    /// Ack generation, observed through the capture rather than through session internals: every
    /// admitted game packet is acknowledged, the ack names the sequence number that was sent, and the
    /// reply carries no game payload of its own.
    /// </summary>
    [Fact]
    public void EveryAdmittedGamePacketIsAcknowledgedOnTheNextDatagram()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession(Peer);

        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_PURESYNC);

        for (uint sequence = 100; sequence < 103; sequence++)
        {
            int before = harness.Tx.Count;

            // Three distinct reliable-ordered messages on one channel are ordering indexes 0, 1, 2;
            // repeating 0 would make the second and third older than the hole the layer expects.
            harness.Inject(Datagram(sequence, new byte[] { wireId, 0x01 }, orderingIndex: sequence - 100), Peer);

            CaptureLine ack = harness.Tx[before];
            RakNetDatagram.TryParse(ack.Bytes, ack.Bytes.Length, out RakNetDatagram parsed, out string error);
            Assert.Equal(string.Empty, error);
            Assert.True(parsed.HasAcks);
            Assert.Contains(parsed.Acks, range => range.Start <= sequence && sequence <= range.End);
            Assert.Empty(parsed.Packets);
        }

        Assert.Equal(3, harness.Received.Count);
    }

    /// <summary>
    /// A duplicate sequence number is acknowledged again — the peer is stuck in retransmission until
    /// it hears an ack — but delivered only once. Exactly one event, and at least one ack.
    /// </summary>
    [Fact]
    public void ADuplicateSequenceIsAcknowledgedAgainButDeliveredOnce()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession(Peer);

        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_PURESYNC);
        byte[] datagram = Datagram(7, new byte[] { wireId, 0x42 });

        harness.Inject(datagram, Peer);
        int txAfterFirst = harness.Tx.Count;
        harness.Inject(datagram, Peer);

        Assert.Single(harness.Received);
        Assert.True(harness.Tx.Count > txAfterFirst, "a retransmission must still be acked");
    }

    /// <summary>
    /// Outbound ordering: successive sends on the same channel get strictly increasing RakNet sequence
    /// numbers and ordering indexes, and each datagram carries exactly one packet.
    /// </summary>
    [Fact]
    public void OutboundSequenceNumbersAndOrderingIndexesAdvanceMonotonically()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession(Peer);

        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_PURESYNC);

        var sequences = new List<uint>();
        var indexes = new List<uint>();

        for (int i = 0; i < 5; i++)
        {
            harness.Wrapper.SendPacket(PeerAddress, PacketId.PACKET_ID_PLAYER_PURESYNC, 0x90, new byte[] { (byte)i });

            CaptureLine line = harness.Tx[^1];
            RakNetDatagram.TryParse(line.Bytes, line.Bytes.Length, out RakNetDatagram parsed, out string error);
            Assert.Equal(string.Empty, error);

            RakNetPacketRecord packet = Assert.Single(parsed.Packets);
            Assert.Equal(wireId, packet.Payload[0]);
            sequences.Add(packet.SequenceNumber);
            indexes.Add(packet.OrderingIndex);
        }

        for (int i = 1; i < sequences.Count; i++)
        {
            Assert.True(sequences[i] > sequences[i - 1], "sequence numbers must advance");
            Assert.True(indexes[i] > indexes[i - 1], "ordering indexes must advance");
        }
    }

    /// <summary>
    /// A payload larger than one datagram is split, every part carries the same split id and the same
    /// part count, and the parts cover the payload in order. Reassembly itself is not exercised:
    /// this port has no inbound reassembler, so only the outbound half can be checked.
    /// </summary>
    [Fact]
    public void AnOversizePayloadIsSplitAcrossDatagrams()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession(Peer);

        var big = new byte[4000];
        for (int i = 0; i < big.Length; i++)
            big[i] = (byte)i;

        int before = harness.Tx.Count;
        harness.Wrapper.SendPacket(PeerAddress, PacketId.PACKET_ID_PLAYER_PURESYNC, 0x90, big);

        var parts = new List<RakNetPacketRecord>();
        foreach (CaptureLine line in harness.Tx.Skip(before))
        {
            RakNetDatagram.TryParse(line.Bytes, line.Bytes.Length, out RakNetDatagram parsed, out string error);
            Assert.Equal(string.Empty, error);
            parts.AddRange(parsed.Packets);
        }

        Assert.True(parts.Count > 1, "a 4000-byte payload must not fit in one datagram");
        Assert.All(parts, part => Assert.True(part.HasSplitData));
        Assert.Single(parts.Select(part => part.SplitPacketId).Distinct());
        Assert.Single(parts.Select(part => part.SplitPacketCount).Distinct());
        Assert.Equal((uint)parts.Count, parts[0].SplitPacketCount);
        Assert.Equal(Enumerable.Range(0, parts.Count).Select(i => (uint)i), parts.Select(part => part.SplitPacketIndex));

        // The reassembled payload is the wire id followed by the original bytes.
        byte[] reassembled = parts.OrderBy(part => part.SplitPacketIndex).SelectMany(part => part.Payload).ToArray();
        Assert.Equal(MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_PURESYNC), reassembled[0]);
        Assert.Equal(big, reassembled[1..]);
    }

    /// <summary>
    /// The offline <c>RID_OPEN_CONNECTION_REQUEST</c>: <c>[0x09][0x04][client GUID 16][magic 16]</c>,
    /// matching the geometry of the 0x09 row (magic at offset
    /// 18, sixteen bytes).
    /// </summary>
    private static byte[] OfflineOpenConnectionRequest(byte[] clientGuid)
    {
        var message = new byte[2 + 16 + 16];
        message[0] = MtaProtocol.Rid.OpenConnectionRequest;
        message[1] = 0x04;
        Array.Copy(clientGuid, 0, message, 2, 16);
        Array.Copy(MtaProtocol.OfflineMagic, 0, message, 18, 16);
        return message;
    }

    /// <summary>
    /// The connected <c>RID_CONNECTION_REQUEST</c>: wire id 4, the 16-byte magic and the 16-byte
    /// client GUID, which is the 33-byte shape <see cref="ConnectedHandshake.TryReadConnectionRequest"/>
    /// accepts. The id stays at its raw RakNet value — 4 is below <c>RID_USER_PACKET_ENUM</c>, so it is
    /// a system message and never goes through the <c>+99</c> MTA mapping.
    /// </summary>
    private static byte[] ConnectedConnectionRequest(byte[] clientGuid)
    {
        var payload = new byte[1 + 16 + 16];
        payload[0] = MtaProtocol.Rid.ConnectionRequest;
        Array.Copy(MtaProtocol.OfflineMagic, 0, payload, 1, 16);
        Array.Copy(clientGuid, 0, payload, 17, 16);
        return payload;
    }

    /// <summary>Wraps one payload in a reliable-ordered RakNet datagram with no acks.</summary>
    private static byte[] Datagram(
        uint sequence,
        byte[] payload,
        uint orderingIndex = 0,
        WireReliability reliability = WireReliability.ReliableOrdered) =>
        RakNetDatagramWriter.Build(
            0,
            Array.Empty<AckRange>(),
            new[]
            {
                new OutgoingPacket
                {
                    SequenceNumber = sequence,
                    Reliability = reliability,
                    OrderingIndex = orderingIndex,
                    Payload = payload,
                },
            });
}
