using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the offline-handshake geometry and the reply byte counts.
/// </summary>
public class OfflineHandshakeTests
{
    private static readonly byte[] ServerGuid = Enumerable.Range(0, 16).Select(i => (byte)i).ToArray();

    [Fact]
    public void Magic_IsTheModifiedOne()
    {
        Assert.Equal(
            new byte[] { 0x00, 0xFF, 0x00, 0xFF, 0xFE, 0xFE, 0xFE, 0xFE, 0xFD, 0xFD, 0xFD, 0xFD, 0x78, 0x56, 0x34, 0x17 },
            MtaProtocol.OfflineMagic);
    }

    [Theory]
    // offset, expected total length, bytes compared
    [InlineData(1, 9, 25, 15)]
    [InlineData(2, 9, 25, 15)]
    [InlineData(4, 1, 33, 16)]
    [InlineData(0x0F, 1, 33, 16)]
    [InlineData(0x10, 1, 33, 16)]
    [InlineData(0x12, 1, 33, 16)]
    [InlineData(0x16, 1, 33, 16)]
    [InlineData(9, 0x12, 0, 16)]
    [InlineData(0x0D, 0x12, 0, 16)]
    [InlineData(0x0A, 1, 0, 16)]
    [InlineData(0x26, 1, 0, 16)]
    [InlineData(0x5F, 2, 0x22, 16)]
    public void MagicGeometry_MatchesTheFiveRowTable(byte messageId, int offset, int expectedLength, int bytesCompared)
    {
        Assert.True(MtaProtocol.HasMagicGeometry(messageId));
        var geometry = MtaProtocol.GetMagicGeometry(messageId);
        Assert.Equal(offset, geometry.Offset);
        Assert.Equal(expectedLength, geometry.ExpectedLength);
        Assert.Equal(bytesCompared, geometry.BytesCompared);
    }

    [Theory]
    [InlineData(0x17)]
    [InlineData(0x00)]
    [InlineData(0x03)]
    [InlineData(0xFF)]
    public void MagicGeometry_RejectsIdsOutsideTheTable(byte messageId)
    {
        Assert.False(MtaProtocol.HasMagicGeometry(messageId));
    }

    [Fact]
    public void WireIdOffset_Is99()
    {
        Assert.Equal(99, MtaProtocol.RakNetUserPacketEnum);
        Assert.Equal(99, MtaProtocol.MtaIdToWireId);
        Assert.Equal((byte)0x65, (byte)(2 + MtaProtocol.MtaIdToWireId));   // SERVER_JOIN_COMPLETE
        Assert.Equal((byte)0x61, (byte)(4 - 2 + 0x5F));                    // sanity on the arithmetic
    }

    [Theory]
    [InlineData(0x13, 4)]
    [InlineData(4, 0x13)]
    [InlineData(5, 5)]
    [InlineData(0x63, 0x63)]
    public void IdSwap_IsBidirectionalFor4And0x13(byte input, byte expected)
    {
        Assert.Equal(expected, MtaProtocol.SwapIds(input));
    }

    [Theory]
    [InlineData(0, 0x63)]    // SERVER_JOIN
    [InlineData(2, 0x65)]    // SERVER_JOIN_COMPLETE
    [InlineData(3, 0x66)]    // PLAYER_JOIN
    [InlineData(4, 0x76)]    // PLAYER_JOINDATA -- swapped to 0x13 before the offset
    [InlineData(0x13, 0x67)] // the 0x13 side of the same swap
    [InlineData(23, 0x7A)]   // SERVER_DISCONNECTED
    [InlineData(103, 0xCA)]  // PACKET_ID_PLAYER_NETWORK_STATUS
    public void OutboundWireId_AppliesTheOffsetAndTheSwap(byte mtaId, byte expected)
    {
        // The release build's remap is bidirectional: a release client's join data is on the wire as
        // 0x76, which requires 4 -> 0x13 before the offset. This arm is inert for a server, since no
        // server-to-client packet uses mtaId 4 or 0x13.
        Assert.Equal(expected, MtaProtocol.ToWireId(mtaId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(0x13)]
    [InlineData(23)]
    [InlineData(0x63)]
    public void OutboundWireId_RoundTripsThroughTheReleaseInboundPath(byte mtaId)
    {
        // Under the release dialect the egress remap and the ingress remap are the same involution, so
        // every id survives the round trip. The debug and mixed dialects are covered separately.
        byte wireId = MtaProtocol.ToWireId(mtaId);
        byte recovered = MtaProtocol.ResolveInboundId((byte)(wireId - MtaProtocol.MtaIdToWireId), MtaIdDialect.Release);
        Assert.Equal(mtaId, recovered);
    }

    [Theory]
    // A release client puts join data on the wire as 0x76 (capture.log seq 12) and a debug client as
    // 0x67 (cap4.log); both must land on mtaId 4 in the modes that accept them.
    [InlineData(MtaIdDialect.Release, 0x13, 4)]   // release client join data
    [InlineData(MtaIdDialect.Release, 4, 0x13)]
    [InlineData(MtaIdDialect.Debug, 4, 4)]        // debug client join data
    [InlineData(MtaIdDialect.Debug, 0x13, 0x13)]
    [InlineData(MtaIdDialect.Both, 0x13, 4)]      // either build
    [InlineData(MtaIdDialect.Both, 4, 4)]
    [InlineData(MtaIdDialect.Auto, 0x13, 4)]      // no latch yet: behaves like Both
    [InlineData(MtaIdDialect.Auto, 4, 4)]
    [InlineData(MtaIdDialect.Release, 5, 5)]      // unaffected ids
    [InlineData(MtaIdDialect.Debug, 5, 5)]
    [InlineData(MtaIdDialect.Both, 5, 5)]
    public void InboundIdResolution(MtaIdDialect dialect, byte resolved, byte expected)
    {
        Assert.Equal(expected, MtaProtocol.ResolveInboundId(resolved, dialect));
    }

    [Fact]
    public void Whitelist_MatchesTheDocumentedPredicate()
    {
        Assert.True(MtaProtocol.IsWhitelisted(0x22, Array.Empty<byte>(), 0));
        Assert.True(MtaProtocol.IsWhitelisted(0x6C, Array.Empty<byte>(), 0));

        Assert.True(MtaProtocol.IsWhitelisted(0x18, new byte[] { 0x00, 0x00 }, 2));
        Assert.True(MtaProtocol.IsWhitelisted(0x18, new byte[] { 0x00, 0x01 }, 2));
        Assert.False(MtaProtocol.IsWhitelisted(0x18, new byte[] { 0x00, 0x02 }, 2));
        Assert.False(MtaProtocol.IsWhitelisted(0x18, new byte[] { 0x00, 0x00 }, 1));

        Assert.False(MtaProtocol.IsWhitelisted(0x01, new byte[] { 0x00, 0x00 }, 2));
    }

    [Fact]
    public void OpenConnectionReply_Is33Bytes()
    {
        var reply = OfflineHandshake.BuildOpenConnectionReply(ServerGuid);

        // [0x0A][magic 16 B][our GUID 16 B] — no config block, because the serializer's
        // guard reads the cipher's permanently-zero encryption flag.
        Assert.Equal(33, reply.Length);
        Assert.Equal(MtaProtocol.Rid.OpenConnectionReply, reply[0]);
        Assert.Equal(MtaProtocol.OfflineMagic, reply.Skip(1).Take(16).ToArray());
        Assert.Equal(ServerGuid, reply.Skip(17).Take(16).ToArray());
    }

    [Fact]
    public void Refusal95_Is34Bytes_AndCarriesTheVersionByte()
    {
        var reply = OfflineHandshake.BuildRefusal(MtaProtocol.Rid.Refusal95, ServerGuid);

        // Assembly-confirmed: [0x5F][0x04][magic 16 B][GUID 16 B] = 34 bytes.
        Assert.Equal(34, reply.Length);
        Assert.Equal(MtaProtocol.Rid.Refusal95, reply[0]);
        Assert.Equal(OfflineHandshake.OpenConnectionRequestVersion, reply[1]);
        Assert.Equal(MtaProtocol.OfflineMagic, reply.Skip(2).Take(16).ToArray());
        Assert.Equal(ServerGuid, reply.Skip(18).Take(16).ToArray());
    }

    [Theory]
    [InlineData(0x10)]
    [InlineData(0x12)]
    public void RefusalNoSlots_Is33Bytes(byte messageId)
    {
        var reply = OfflineHandshake.BuildRefusal(messageId, ServerGuid);
        Assert.Equal(33, reply.Length);
        Assert.Equal(messageId, reply[0]);
        Assert.Equal(ServerGuid, reply.Skip(17).Take(16).ToArray());
    }

    [Fact]
    public void Pong_Is41Bytes()
    {
        var pong = OfflineHandshake.BuildPong(0x1122334455667788ul, ServerGuid);

        // [0x1A][64-bit echo][our GUID 16 B][magic 16 B] = 41 bytes.
        Assert.Equal(41, pong.Length);
        Assert.Equal(MtaProtocol.Rid.Pong, pong[0]);
        Assert.Equal(0x88, pong[1]);
        Assert.Equal(0x11, pong[8]);
        Assert.Equal(ServerGuid, pong.Skip(9).Take(16).ToArray());
        Assert.Equal(MtaProtocol.OfflineMagic, pong.Skip(25).Take(16).ToArray());
    }

    [Fact]
    public void MtaHandshake38Reply_Is26Bytes()
    {
        var reply = OfflineHandshake.BuildMtaHandshake38Reply(ServerGuid, 0xAABBCCDD, 0x01020304);

        Assert.Equal(26, reply.Length);
        Assert.Equal(MtaProtocol.Rid.MtaHandshake38, reply[0]);
        // The reply writes a single 0x02 byte after the GUID.
        Assert.Equal(0x02, reply[17]);
        Assert.Equal(MtaProtocol.OfflineMagic, reply.Skip(1).Take(16).ToArray());
        Assert.Equal(0xDD, reply[18]);
        Assert.Equal(0xAA, reply[21]);
        Assert.Equal(0x04, reply[22]);
        Assert.Equal(0x01, reply[25]);
    }

    [Fact]
    public void PingRequest_IsParsedFromTheDocumentedOffset()
    {
        var request = new byte[25];
        request[0] = MtaProtocol.Rid.Ping;
        // The echo occupies bytes 9..24 (magic lives at offset 9 for ids 1 and 2).
        BitConverter.GetBytes(0x0102030405060708ul).CopyTo(request, 1);

        Assert.True(OfflineHandshake.TryReadPing(request, request.Length, out ulong clientTime));
        Assert.Equal(0x0102030405060708ul, clientTime);
    }

    [Fact]
    public void OpenConnectionRequest_Requires34Bytes()
    {
        var guid = Enumerable.Range(0, 16).Select(i => (byte)(0xA0 + i)).ToArray();
        var request = new byte[34];
        request[0] = MtaProtocol.Rid.OpenConnectionRequest;
        request[1] = OfflineHandshake.OpenConnectionRequestVersion;
        guid.CopyTo(request, 2);
        MtaProtocol.OfflineMagic.CopyTo(request, 18);

        Assert.True(OfflineHandshake.TryReadOpenConnectionRequest(request, request.Length, out var readGuid, out var version));
        Assert.Equal(OfflineHandshake.OpenConnectionRequestVersion, version);
        Assert.Equal(guid, readGuid);

        Assert.False(OfflineHandshake.TryReadOpenConnectionRequest(request, 33, out _, out _));
    }

    [Fact]
    public void MtaHandshake38_EchoIsReadForSubtype1Only()
    {
        var request = new byte[22];
        request[0] = MtaProtocol.Rid.MtaHandshake38;
        request[17] = 1;
        BitConverter.GetBytes(0xCAFEBABEu).CopyTo(request, 18);

        Assert.True(OfflineHandshake.TryReadMtaHandshake38(request, request.Length, out var subtype, out var echo));
        Assert.Equal(1, subtype);
        Assert.Equal(0xCAFEBABEu, echo);

        request[17] = 0;
        Assert.True(OfflineHandshake.TryReadMtaHandshake38(request, request.Length, out subtype, out _));
        Assert.Equal(0, subtype);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(9, true)]
    [InlineData(0x26, true)]
    [InlineData(10, false)]
    [InlineData(0x0F, false)]
    [InlineData(4, false)]
    public void ServerRoleCases_AreExactly1_9_0x26(byte messageId, bool serverRole)
    {
        Assert.Equal(serverRole, OfflineHandshake.IsServerRoleCase(messageId));
    }

    /// <summary>
    /// The client emits <c>RID_CONNECTION_REQUEST</c> as
    /// <c>[0x04][magic 16 B][GUID 16 B]</c> = 33 bytes, which is exactly the offline case-4
    /// geometry. The router must classify it as offline even when the caller
    /// hands it a reused, over-sized receive buffer — the socket path always does, and measuring with
    /// <c>Array.Length</c> there made every fixed-length id fail its geometry guard.
    /// </summary>
    [Fact]
    public void ConnectionRequest_RoutesOfflineFromAReusedReceiveBuffer()
    {
        var buffer = new byte[1492];
        buffer[0] = MtaProtocol.Rid.ConnectionRequest;
        MtaProtocol.OfflineMagic.CopyTo(buffer, 1);
        for (int i = 17; i < 33; i++)
            buffer[i] = (byte)i;

        Assert.Equal(OfflineRoute.Offline, OfflineHandshake.Route(buffer, byteLength: 33, hasSessionRecord: true));

        // Without the length the same bytes are indistinguishable from a 1492-byte datagram, which is
        // why the router must not fall back to Array.Length.
        Assert.NotEqual(OfflineRoute.Offline, OfflineHandshake.Route(buffer, buffer.Length, hasSessionRecord: true));
    }
}
