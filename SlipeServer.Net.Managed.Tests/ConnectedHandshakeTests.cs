using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the Stage 0 connected <c>RID_CONNECTION_REQUEST</c> (4) / <c>RID_CONNECTION_REQUEST_ACCEPTED</c>
/// (0x0E) exchange against the captured request and the native reply encoding
/// (<c>FUN_10043c70</c> / <c>FUN_100484b0</c>).
/// </summary>
public class ConnectedHandshakeTests
{
    /// <summary>
    /// The 33-byte connected payload from capture.log, verbatim: the wire id (0x04), the modified
    /// offline magic, then the same 16-byte client GUID the offline case-9 request carried.
    /// </summary>
    private static readonly byte[] CapturedRequest =
    {
        0x04,
        0x00, 0xFF, 0x00, 0xFF, 0xFE, 0xFE, 0xFE, 0xFE,
        0xFD, 0xFD, 0xFD, 0xFD, 0x78, 0x56, 0x34, 0x17,
        0x25, 0xA3, 0x41, 0x05, 0x26, 0x61, 0xEF, 0x42,
        0x43, 0xF7, 0x28, 0x2E, 0x96, 0xAE, 0xE6, 0x75,
    };

    [Fact]
    public void CapturedRequest_Is33BytesWithTheMagicAtOffset1()
    {
        Assert.Equal(33, CapturedRequest.Length);
        Assert.Equal(MtaProtocol.Rid.ConnectionRequest, CapturedRequest[0]);

        // The connected path repeats the modified offline magic, but the native handler only skips it.
        Assert.True(MtaProtocol.TryValidateMagic(CapturedRequest, MtaProtocol.Rid.ConnectionRequest, out _));
    }

    [Fact]
    public void CapturedRequest_YieldsTheClientGuid()
    {
        Assert.True(ConnectedHandshake.TryReadConnectionRequest(CapturedRequest, out byte[] guid));

        Assert.Equal(ConnectedHandshake.GuidLength, guid.Length);
        Assert.Equal(
            new byte[] { 0x25, 0xA3, 0x41, 0x05, 0x26, 0x61, 0xEF, 0x42, 0x43, 0xF7, 0x28, 0x2E, 0x96, 0xAE, 0xE6, 0x75 },
            guid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(32)]
    public void ShortRequests_AreRejected(int length)
    {
        Assert.False(ConnectedHandshake.TryReadConnectionRequest(new byte[length], out _));
    }

    [Fact]
    public void Reply_Is69Bytes()
    {
        byte[] body = ConnectedHandshake.BuildConnectionRequestAccepted(0, System.Array.Empty<ulong>(), 0);

        Assert.Equal(69, body.Length);
        Assert.Equal(ConnectedHandshake.ReplyLength, body.Length);
        Assert.Equal(MtaProtocol.Rid.ConnectionRequestAccepted, body[0]);
    }

    [Fact]
    public void Reply_ComplementsTheClientAddressAndCarriesItsPort()
    {
        // Client dword 0x1EB2A8C0, port 0xC579.
        ulong client = ((ulong)0x1EB2A8C0u << 32) | 50553;

        byte[] body = ConnectedHandshake.BuildConnectionRequestAccepted(client, System.Array.Empty<ulong>(), 0);

        // ~0x1EB2A8C0 = 0xE14D573F, little-endian.
        Assert.Equal(0x3F, body[1]);
        Assert.Equal(0x57, body[2]);
        Assert.Equal(0x4D, body[3]);
        Assert.Equal(0xE1, body[4]);
        Assert.Equal(0x79, body[5]);
        Assert.Equal(0xC5, body[6]);
    }

    [Fact]
    public void Reply_AdvertisesTheServerAddressInTheFirstSlot()
    {
        ulong server = ((ulong)0x0100007Fu << 32) | 50666; // 127.0.0.1:50666

        byte[] body = ConnectedHandshake.BuildConnectionRequestAccepted(0, new[] { server }, 0);

        // Slot 0 starts at offset 9: ~0x0100007F = 0xFEFFFF80, port 0xC5EA.
        Assert.Equal(0x80, body[9]);
        Assert.Equal(0xFF, body[10]);
        Assert.Equal(0xFF, body[11]);
        Assert.Equal(0xFE, body[12]);
        Assert.Equal(0xEA, body[13]);
        Assert.Equal(0xC5, body[14]);

        // The nine unpopulated slots are zeroed entries: address 0 complements to 0xFFFFFFFF, port 0.
        for (int slot = 1; slot < ConnectedHandshake.ReplyAddressSlots; slot++)
        {
            int at = 9 + (slot * 6);
            Assert.Equal(0xFF, body[at]);
            Assert.Equal(0xFF, body[at + 1]);
            Assert.Equal(0xFF, body[at + 2]);
            Assert.Equal(0xFF, body[at + 3]);
            Assert.Equal(0x00, body[at + 4]);
            Assert.Equal(0x00, body[at + 5]);
        }
    }

    [Fact]
    public void Reply_WritesTheSystemIndexAsA16BitField()
    {
        byte[] body = ConnectedHandshake.BuildConnectionRequestAccepted(0, System.Array.Empty<ulong>(), 0x1234);

        Assert.Equal(0x34, body[7]);
        Assert.Equal(0x12, body[8]);
    }
}
