using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// The port allocates outbound ordering indexes from two independent per-channel spaces, one for the
/// ordered family and one for the sequenced family, because native does.
/// <para>
/// <c>ReliabilityLayer::Send</c> (<c>FUN_100539a0</c>) chooses the counter by reliability before it
/// stores the index into the packet:
/// </para>
/// <code>
/// iVar7 = puVar4[5];                                             // reliability
/// if ((iVar7 == 4) || (iVar7 == 1)) {
///   iVar7 = *(int *)(in_ECX + 0x99c + channel * 4);              // sequenced counter
///   *(int *)(in_ECX + channel * 4 + 0x99c) = iVar7 + 1;
/// } else {
///   if (iVar7 != 3) goto LAB_10053b81;
///   iVar7 = *(int *)(in_ECX + 0x91c + channel * 4);              // ordered counter
///   *(int *)(in_ECX + channel * 4 + 0x91c) = iVar7 + 1;
/// }
/// puVar4[7] = iVar7;                                             // ordering index
/// </code>
/// <para>
/// The two arrays are 0x80 bytes apart, which is exactly 32 channels of <c>uint</c>, so each is a
/// per-channel counter and neither is a fallback for the other. The receive side mirrors the split
/// with one expected index per channel per family: one array for reliabilities 1 and 4, a second
/// for reliability 3.
/// </para>
/// <para>
/// Sharing one counter is not a cosmetic difference. Sequenced traffic then spends ordered indexes,
/// so the peer's ordered channel sits on a hole that is never filled and stops delivering reliable
/// packets — observed live as "after the second client joins, reliable packets are no longer
/// processed while unreliable ones still arrive".
/// </para>
/// <para>
/// Expected indexes here are <c>0, 1, 2, …</c> from a fresh session: native initialises both counters
/// to zero and increments after handing the value out, so the first packet of each family on a fresh
/// channel is <c>0</c>. The expectations are transcribed from that rule, not read back out of the
/// port's own writer.
/// </para>
/// </summary>
public class OrderingIndexAllocationTests
{
    /// <summary>Slipe's name for wire reliability 3.</summary>
    private const PacketReliability Ordered = PacketReliability.ReliableSequenced;

    /// <summary>Slipe's name for wire reliability 1.</summary>
    private const PacketReliability Sequenced = PacketReliability.UnreliableSequenced;

    /// <summary>Slipe's name for wire reliability 4, the other sequenced-family member.</summary>
    private const PacketReliability SequencedCanDrop = PacketReliability.ReliableSequencedCanDropPackets;

    /// <summary>
    /// The chat echo payload. mtaId 43 is not one of the three ids the wrapper prefixes, so the
    /// datagram is the plain wire id plus these bytes and nothing else competes for the index.
    /// </summary>
    private static readonly byte[] ChatBody = [0x01, 0x02, 0x03, 0x04];

    /// <summary>Wire reliabilities whose packets carry an ordering channel and index.</summary>
    private static bool Carries(int reliability) => reliability is 1 or 3 or 4;

    /// <summary>
    /// Every ordering-carrying chat-echo token the wrapper put on the wire, in send order. Handshake
    /// replies are not RakNet datagrams and carry no such token, so filtering by wire id and
    /// reliability isolates exactly the packets the test sent.
    /// </summary>
    private static List<(string EndPoint, int Reliability, uint Index)> Sent(WrapperHarness harness)
    {
        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_CHAT_ECHO);

        return harness.Tx
            .SelectMany(line => line.Packets.Select(token => (line.EndPoint, token)))
            .Where(entry => entry.token.WireId == wireId && Carries(entry.token.Reliability))
            .Select(entry => (entry.EndPoint, entry.token.Reliability, entry.token.OrderingIndex))
            .ToList();
    }

    private static void Send(WrapperHarness harness, PacketReliability reliability) =>
        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_CHAT_ECHO,
            0x91,
            ChatBody,
            PacketPriority.High,
            reliability);

    /// <summary>
    /// A fresh session numbers the ordered stream from zero, one per packet.
    /// </summary>
    [Fact]
    public void OrderedSendsAreNumberedFromZeroWithoutGaps()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        Send(harness, Ordered);
        Send(harness, Ordered);
        Send(harness, Ordered);

        List<(string EndPoint, int Reliability, uint Index)> sent = Sent(harness);

        Assert.Equal([3, 3, 3], sent.Select(entry => entry.Reliability));
        Assert.Equal([0u, 1u, 2u], sent.Select(entry => entry.Index));
    }

    /// <summary>
    /// Sequenced sends must not touch the ordered counter. This is the regression: with one shared
    /// counter the ordered indexes come out <c>0, 3, 6</c> and the client stalls on the hole at 1.
    /// </summary>
    [Fact]
    public void SequencedSendsDoNotConsumeOrderedIndexes()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        Send(harness, Ordered);      // ordered 0
        Send(harness, Sequenced);    // sequenced 0
        Send(harness, Ordered);      // ordered 1
        Send(harness, Sequenced);    // sequenced 1
        Send(harness, Ordered);      // ordered 2
        Send(harness, Sequenced);    // sequenced 2

        List<(string EndPoint, int Reliability, uint Index)> sent = Sent(harness);

        Assert.Equal([3, 1, 3, 1, 3, 1], sent.Select(entry => entry.Reliability));

        Assert.Equal(
            [0u, 1u, 2u],
            sent.Where(entry => entry.Reliability == 3).Select(entry => entry.Index));

        Assert.Equal(
            [0u, 1u, 2u],
            sent.Where(entry => entry.Reliability == 1).Select(entry => entry.Index));
    }

    /// <summary>
    /// Reliabilities 1 and 4 are one family, so they share a counter with each other and not with
    /// reliability 3. Native tests them together in a single arm.
    /// </summary>
    [Fact]
    public void TheTwoSequencedReliabilitiesShareOneCounter()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        Send(harness, Sequenced);            // sequenced 0
        Send(harness, SequencedCanDrop);     // sequenced 1
        Send(harness, Ordered);              // ordered 0
        Send(harness, Sequenced);            // sequenced 2
        Send(harness, Ordered);              // ordered 1

        List<(string EndPoint, int Reliability, uint Index)> sent = Sent(harness);

        Assert.Equal([1, 4, 3, 1, 3], sent.Select(entry => entry.Reliability));

        Assert.Equal(
            [0u, 1u, 2u],
            sent.Where(entry => entry.Reliability is 1 or 4).Select(entry => entry.Index));

        Assert.Equal(
            [0u, 1u],
            sent.Where(entry => entry.Reliability == 3).Select(entry => entry.Index));
    }

    /// <summary>
    /// A reliability that carries no ordering index advances neither counter — native leaves both
    /// arrays alone and jumps straight past the ordering block.
    /// </summary>
    [Fact]
    public void ReliabilitiesWithoutAnOrderingIndexAdvanceNeitherCounter()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        Send(harness, PacketReliability.Unreliable);
        Send(harness, PacketReliability.Reliable);
        Send(harness, Ordered);
        Send(harness, Sequenced);

        List<(string EndPoint, int Reliability, uint Index)> sent = Sent(harness);

        Assert.Equal([3, 1], sent.Select(entry => entry.Reliability));
        Assert.Equal([0u, 0u], sent.Select(entry => entry.Index));
    }

    /// <summary>
    /// The two counters are per session, so a second peer starts its own streams at zero rather than
    /// inheriting the first peer's position.
    /// </summary>
    [Fact]
    public void EachSessionHasItsOwnCounters()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        Send(harness, Ordered);
        Send(harness, Ordered);

        var second = WireTestKit.Endpoint("192.0.2.80:49189");
        harness.ConnectSession(second);

        harness.Wrapper.SendPacket(
            MtaNetWrapper.BinaryAddressOf(second),
            PacketId.PACKET_ID_CHAT_ECHO,
            0x91,
            ChatBody,
            PacketPriority.High,
            Ordered);

        string otherPeer = WireTestKit.EndpointOf(second);

        List<(string EndPoint, int Reliability, uint Index)> all = Sent(harness);
        List<uint> first = all
            .Where(entry => entry.EndPoint != otherPeer)
            .Select(entry => entry.Index)
            .ToList();
        List<uint> other = all
            .Where(entry => entry.EndPoint == otherPeer)
            .Select(entry => entry.Index)
            .ToList();

        Assert.Equal([0u, 1u], first);
        Assert.Equal([0u], other);
    }
}
