using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// <c>PLAYER_LIST</c> reaches the client behind one prefix the mod-level body does not contain: the
/// net layer's anticheat block, a version-gated 16-bit registry value followed by two
/// length-prefixed strings.
/// <para>
/// The client reads that block in two stages. The closed-source <c>netc.dll</c> consumes the 16-bit
/// value, and the open-source <c>CPacketHandler::Packet_PlayerList</c> then reads what is left as its
/// own <c>ReadString(strACInfo)</c> and <c>ReadString(strSDInfo)</c>. The <c>bJustJoined</c> bit it
/// reads next is the mod-level body's first bit: MTA's own <c>CPlayerListPacket::Write</c> emits
/// <c>WriteBit(m_bShowInChat)</c> there rather than a separate flag. The entries follow immediately,
/// with no player count — the client loops while at least 8 bits remain.
/// </para>
/// <para>
/// There is therefore nothing else to add. A second empty-string envelope on top of the block costs
/// 32 bits of misalignment: <c>bJustJoined</c> reads as <c>false</c>, the entry loop starts 32 bits
/// early, and the first <c>ucPlayerNickLength</c> it sees is zero, which the client rejects with
/// <c>RaiseProtocolError(9)</c>. A zero-entry list hides the misalignment because the loop never
/// runs, so that defect showed up most visibly with two players connected.
/// </para>
/// <para>
/// <b>Provenance of the two body literals.</b> They are the <c>PLAYER_LIST</c> payloads at cap-seq
/// 476 and 478 of <c>captures/capture-20261018-075258.log</c> — datagrams of 75 and 69 bytes
/// carrying a single packet each, logged as <c>bits=504 bytes=63</c> and <c>bits=456 bytes=57</c>.
/// The logged <c>bytes=</c> counts the leading wire id, so the bodies are 62 and 56 bytes; those two
/// numbers are asserted below rather than trusted in prose. That capture predates this block, so its
/// payloads are the mod-level body verbatim, which is exactly what
/// <see cref="MtaGamePackets.PrependPlayerListAcBlock"/> takes as input. The capture file is
/// gitignored, so the excerpt is transcribed here instead of cited for reproduction: the two
/// literals are the evidence, not a promise that a reader can regenerate them.
/// </para>
/// <para>
/// <b>The gate must be open; the port's closed gate was the defect.</b> The gate is
/// <c>0x39 &lt; view-&gt;Version() || 0x39 &lt; *(ushort*)(player + 0x938)</c>, so a 1.6.0 client at
/// <c>0x91</c> opens it and the block carries the registry <c>u16</c> ahead of the two strings. An
/// earlier revision of this file read three capture pairs —
/// <c>capture-20261018-082457.log</c> seq 26 (<c>bits=48 bytes=6</c>) against
/// <c>capture-20261018-075258.log</c> seq 26 (<c>bits=16 bytes=2</c>), seq 341 against seq 480, and
/// seq 339 against seq 478 — as proof that a reference server writes only 32 bits and therefore that
/// the gate stays closed in practice, and this port was changed to match. That reading was wrong
/// twice over. Those records are a server whose <c>m_strACInfo</c> was empty, and an empty pair of
/// strings is 32 bits whether or not the registry value precedes it, so the records cannot
/// discriminate the gate at all; and a port that closed the gate made every client quit 126–160 ms
/// after its roster, alone or not, while a port that opened it let the client join and print
/// <c>Server AC info: [Allowed client files: All] [Disabled AC: None] [Enabled SD: None]</c>. The
/// version the gate needs is already in hand at <c>SendPacket</c> time — the trace records
/// <c>bitstream version 0x91</c> for packet 2 on the very sessions whose rosters were broken — so the
/// only fault was that the roster path hardcoded <c>0</c> for both terms.
/// </para>
/// <para>
/// <b>The block for a 1.6.0 client is seven bytes: a registry <c>u16</c>, then
/// <c>0100 30 0000</c>.</b> <c>CNetServer::SetChecks</c> (<c>FUN_10035890</c> at
/// <c>0x10035890</c> in <c>binaries/net.dll</c>) seeds <c>m_strACInfo</c> at <c>+0x183c</c> from
/// <c>sprintf("%d", iEnableClientChecks)</c> — format literal <c>DAT_101bbdf8</c> = <c>"%d"</c>, move
/// at <c>0x10035d41</c>–<c>0x10035d61</c> — so that string can never be empty once <c>SetChecks</c>
/// has run. The reference shim calls <c>SetChecks(..., 0, 0, "none")</c> at
/// <c>NetWrapper.cpp:229</c>, giving the seed <c>"0"</c> and leaving <c>bHideAC</c> (<c>+0x1884</c>)
/// clear, so the real-data branch runs and the strings are <c>0100 30</c> (a one-byte <c>"0"</c>)
/// and <c>0000</c> (an empty SD string). The disabled-check entries that would follow the seed come
/// from <c>szDisableACMap</c>, which the shim passes as <c>""</c>, so nothing follows it; the
/// <c>"12=&amp;14=…"</c> combo map is a separate parameter that never reaches either string.
/// </para>
/// <para>
/// <b>The registry <c>u16</c> is the player count.</b> <c>FUN_1000d3c0</c> — the accessor the
/// <c>0x19</c> arm calls — is <c>MOV EAX, [ECX + 8]; RET</c> with <c>ECX = *(CNetServer + 0x24)</c>,
/// and that sub-object is <c>{ bool; std::_Tree }</c>: <c>_Myhead</c> at <c>+0x4</c>, <c>_Mysize</c>
/// at <c>+0x8</c>. So the field is <c>std::map::size()</c> over the address-keyed player registry,
/// which <c>FUN_10009680</c> (<c>std::_Tree::_Insert</c>, <c>INC [EBX + 4]</c>) increments once per
/// insertion. <c>FUN_10005330</c> reads the same accessor as a dword into the <c>ados_pkts_b</c>
/// snapshot. The port therefore writes <c>sessions.Count</c> here, not a placeholder — a server with
/// one connected player sends <c>0100</c>. No erase path was found for that map, so native could in
/// principle exceed the live count after an address slot is recycled. This is the shape pinned
/// below, and <see cref="MtaNetWrapper.AcInfoText"/> exists so the string half can be retried
/// without a rebuild.
/// </para>
/// </summary>
public sealed class PlayerListFramingTests
{
    /// <summary>
    /// cap-seq 476, for 192.0.2.80:57387 — 62 bytes. One record, nick
    /// <c>SAES&gt;NanoLaptop</c>, spawned, model 7, alpha 100. Bit 0 is <c>showInChat</c>, bits
    /// 1–17 the entry's <c>ElementID</c>, bits 18–25 its <c>timeContext</c>, and the nick's 8-bit
    /// length sits at bit 26.
    /// </summary>
    private const string NanoLaptopBodyHex =
        "850000C3D4D05154CF93985B9BD3185C1D1BDC15C0400000001203D4D05154CF93985B9BD3185C1D1BDC00" +
        "37738080308B80004023A0000060089B000000";

    /// <summary>
    /// cap-seq 478, for 192.0.2.10:64822 — 56 bytes. One record, nick <c>SAES&gt;NanoBob</c>,
    /// spawned, model 0, default alpha 255.
    /// </summary>
    private const string NanoBobBodyHex =
        "0580004314D05154CF93985B9BD09BD895C040000000120314D05154CF93985B9BD09BD8803000000000" +
        "0000000000000E40600900000000";

    /// <summary>
    /// The two length-prefixed strings on their own, without the version-gated registry value: the
    /// reference shim's <c>SetChecks(..., 0, 0, "none")</c> call seeds <c>m_strACInfo</c> with
    /// <c>sprintf("%d", 0)</c> = <c>"0"</c> and leaves the SD string empty. This is the whole block
    /// below the gate. See the class remarks for the writer.
    /// </summary>
    private const string AnticheatStringsHex = "0100300000";

    /// <summary>
    /// The seven-byte gate-open block with the registry value left at the helper's default of zero:
    /// the shape for an empty player registry. The wrapper never uses that default — it writes the
    /// live count — so this literal only pins the pure function.
    /// </summary>
    private const string AnticheatBlockHex = "00000100300000";

    /// <summary>
    /// The registry value a server with exactly one connected player writes: <c>1</c>, little-endian.
    /// The value is the player registry's <c>std::map::size()</c> (see the class remarks), so it is a
    /// count rather than a constant, and every wrapper test below runs with one session.
    /// </summary>
    private const string OneSessionRegistryHex = "0100";

    /// <summary>
    /// The block is written ahead of the body unchanged, in the shape a 1.6.0 client's roster
    /// carries: the registry value, the seed string, an empty SD string.
    /// </summary>
    [Fact]
    public void TheAnticheatBlockIsWrittenAheadOfTheBody()
    {
        byte[] body = [0xAA, 0xBB];
        byte[] framed = MtaGamePackets.PrependPlayerListAcBlock(body, 0x91, 0x91);

        Assert.Equal(AnticheatBlockHex, WireTestKit.HexOf(framed[..^body.Length]));
        Assert.Equal(body, framed[^body.Length..]);
    }

    /// <summary>
    /// <c>bJustJoined</c> is the body's first bit, so overriding it must move that bit and nothing
    /// else. The client only calls <c>CAntiCheat::GetInfo</c> — and only prints
    /// <c>"Server AC info: %s"</c> — when the bit reads <i>false</i> (CPacketHandler.cpp:754). That is
    /// what the <c>justJoined</c> hook exists for: a client that prints the info has demonstrably
    /// found the block at the offset the writer intended. Production sends it unset.
    /// </summary>
    [Theory]
    [InlineData("80", null, "80")]
    [InlineData("80", false, "00")]
    [InlineData("80", true, "80")]
    [InlineData("05", null, "05")]
    [InlineData("05", true, "85")]
    [InlineData("05", false, "05")]
    public void OverridingJustJoinedMovesOnlyTheBodysFirstBit(
        string bodyHex, bool? justJoined, string expectedBodyHex)
    {
        byte[] body = Convert.FromHexString(bodyHex);
        byte[] framed = MtaGamePackets.PrependPlayerListAcBlock(body, 0x91, 0x91, justJoined: justJoined);

        Assert.Equal(AnticheatBlockHex, WireTestKit.HexOf(framed[..^body.Length]));
        Assert.Equal(expectedBodyHex, WireTestKit.HexOf(framed[^body.Length..]));
    }

    /// <summary>
    /// The 16-bit registry value is version-gated; the two strings are not. Below the gate the block
    /// is the two length-prefixed strings alone, not seven bytes.
    /// </summary>
    [Fact]
    public void TheAnticheatBlockOmitsTheRegistryValueBelowTheGate()
    {
        byte[] framed = MtaGamePackets.PrependPlayerListAcBlock([0xAA], 0x39, 0x39);

        Assert.Equal(AnticheatStringsHex, WireTestKit.HexOf(framed[..^1]));
        Assert.Equal(6, framed.Length);
    }

    /// <summary>
    /// The native test is <c>0x39 &lt; view-&gt;Version() || 0x39 &lt; *(ushort*)(player + 0x938)</c>
    /// — an OR of the packet view's version and the player record's own. Either one clearing the gate
    /// is enough, so collapsing them to a single value diverges whenever they disagree.
    /// </summary>
    [Theory]
    [InlineData(0x90, 0x90, 7)]
    [InlineData(0x90, 0x00, 7)]
    [InlineData(0x00, 0x90, 7)]
    [InlineData(0x3A, 0x00, 7)]
    [InlineData(0x00, 0x3A, 7)]
    [InlineData(0x39, 0x39, 5)]
    [InlineData(0x00, 0x00, 5)]
    public void EitherVersionClearingTheGateWritesTheRegistryValue(
        ushort viewVersion,
        ushort playerVersion,
        int expectedPrefixLength)
    {
        byte[] framed = MtaGamePackets.PrependPlayerListAcBlock(
            Array.Empty<byte>(), viewVersion, playerVersion);

        Assert.Equal(expectedPrefixLength, framed.Length);
    }

    /// <summary>
    /// <c>bHideAC</c> does not omit the block. The registry value is written before
    /// <c>+0x1884</c> is tested, so hiding anticheat reporting keeps the value and both length
    /// fields and substitutes a bare <c>0x0000</c> for each string — the block never shortens below
    /// six bytes.
    /// </summary>
    [Fact]
    public void HidingAnticheatEmptiesBothStringsButKeepsTheirLengthsAndTheRegistryValue()
    {
        byte[] framed = MtaGamePackets.PrependPlayerListAcBlock(
            [0xAA],
            bitStreamVersion: 0x90,
            playerBitStreamVersion: 0x90,
            hideAnticheat: true,
            registryValue: 0x1234);

        Assert.Equal("3412" + "0000" + "0000", WireTestKit.HexOf(framed[..6]));
        Assert.Equal(7, framed.Length);
    }

    /// <summary>
    /// Non-empty check text and map rendering are length-prefixed rather than padded, so the block
    /// grows by exactly their byte counts. The payloads are raw bytes, not characters through a code
    /// page: native copies a <c>std::string</c>'s storage, so a byte above 0x7F must survive
    /// unchanged.
    /// </summary>
    [Fact]
    public void TheAnticheatBlockLengthPrefixesBothStringsAsRawBytes()
    {
        byte[] framed = MtaGamePackets.PrependPlayerListAcBlock(
            [0xAA],
            bitStreamVersion: 0x90,
            playerBitStreamVersion: 0x90,
            registryValue: 0,
            acInfo: [(byte)'1', 0xE9],
            sdInfo: [(byte)'a', (byte)'=', 0xFF]);

        Assert.Equal("0000" + "0200" + "31E9" + "0300" + "613DFF", WireTestKit.HexOf(framed[..11]));
        Assert.Equal(12, framed.Length);
    }

    /// <summary>
    /// The block's length fields are 16 bits, so an over-long payload cannot be truncated into a
    /// shorter field and sent — it must be refused.
    /// </summary>
    [Fact]
    public void AnOverlongStringIsRejectedRatherThanTruncated()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MtaGamePackets.PrependPlayerListAcBlock(
            Array.Empty<byte>(),
            bitStreamVersion: 0x90,
            playerBitStreamVersion: 0x90,
            acInfo: new byte[ushort.MaxValue + 1]));
    }

    /// <summary>
    /// The conformance property the defect violated: read the framed stream exactly as the client
    /// does and the entry loop must start at the body's second bit. <c>netc.dll</c> consumes the
    /// registry value first, then the two <c>ReadString</c> fields are the next thing on the stream,
    /// and <c>bJustJoined</c> must land on the body's own leading <c>showInChat</c> bit.
    /// </summary>
    [Theory]
    [InlineData(NanoLaptopBodyHex)]
    [InlineData(NanoBobBodyHex)]
    public void TheClientLandsOnTheBodyTheFirstEntryBitAfterTheBlock(string bodyHex)
    {
        byte[] body = WireTestKit.Hex(bodyHex);
        byte[] framed = MtaGamePackets.PrependPlayerListAcBlock(body, 0x91, 0x91);

        var reader = new ClientStreamReader(framed);

        reader.ReadUInt16();
        string acInfo = reader.ReadString();
        string sdInfo = reader.ReadString();
        bool justJoined = reader.ReadBit();

        Assert.Equal(MtaGamePackets.DefaultAcInfoText, acInfo);
        Assert.Equal(string.Empty, sdInfo);

        // bJustJoined is not a separate field: it is the body's own showInChat bit.
        Assert.Equal((body[0] & 0x80) != 0, justJoined);

        // registry value (16) + "0" (16-bit length + 1 byte) + "" (16-bit length) + 1 bit = 57, so
        // the entry loop starts at body bit 1 and the reader is one bit into the body.
        Assert.Equal(57, reader.BitPosition);
    }

    /// <summary>
    /// The defect this pins: the wrapper is handed a 1.6.0 client's bitstream version — <c>0x91</c>
    /// in the <c>SendPacket</c> argument, and mirrored into
    /// <c>MtaNetSession.NegotiatedBitStreamVersion</c> — and the roster must therefore carry the
    /// registry value. The roster path used to hardcode <c>0</c> for both gate terms while the
    /// argument already held <c>0x91</c>, so the gate was always closed and every client quit
    /// 126–160 ms after its roster, alone or not. Forcing the gate open by hand made the client join
    /// and print <c>Server AC info: [Allowed client files: All] [Disabled AC: None]
    /// [Enabled SD: None]</c>.
    /// </summary>
    [Fact]
    public void TheRosterForAModernClientCarriesTheRegistryValue()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        MtaNetSession session = Assert.IsType<MtaNetSession>(harness.Wrapper.FindSession(harness.Address));
        session.NegotiatedBitStreamVersion = 0x91;

        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_PLAYER_LIST,
            0x91,
            [0x80]);

        // The gate is open at 0x91, so the block is the registry value — one, for the single
        // connected session — then the SetChecks-seeded "0", the empty SD string, and the body's
        // `80`: nine bytes behind the wire id.
        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_LIST);
        string want = WireTestKit.HexOf(
            [
                wireId,
                .. WireTestKit.Hex(OneSessionRegistryHex),
                .. WireTestKit.Hex(AnticheatStringsHex),
                0x80,
            ]);

        string tx = WireTestKit.HexOf(harness.Tx[^1].Bytes);
        Assert.True(tx.EndsWith(want, StringComparison.Ordinal), $"tx ({tx.Length / 2}B) = {tx}");
        Assert.Equal(9, want.Length / 2);

        // The trace is how the defect was found: it reported 0x0 for both versions on every roster
        // while packet 2 reported 0x91 on the same sessions.
        Assert.Contains("view version 0x91 / player version 0x91", harness.Trace[^1], StringComparison.Ordinal);
    }

    /// <summary>
    /// The wrapper emits the recorded body behind the block and nothing else, at the length the
    /// reference server actually sends. The version is the one a 1.6.0 client negotiates, so the
    /// gate is open: the wire id plus the seven-byte block plus the 62- or 56-byte body, i.e. 70 or
    /// 64 bytes. Both bodies are transcribed from the two-client capture, one per recipient.
    /// </summary>
    [Theory]
    [InlineData(NanoLaptopBodyHex, 62, 70)]
    [InlineData(NanoBobBodyHex, 56, 64)]
    public void TheWrapperEmitsTheRecordedPlayerListBodyBehindTheBlock(
        string bodyHex,
        int bodyLength,
        int packetLength)
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        byte[] body = WireTestKit.Hex(bodyHex);
        Assert.Equal(bodyLength, body.Length);

        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_PLAYER_LIST,
            0x91,
            body);

        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_LIST);
        string want = WireTestKit.HexOf(
            [
                wireId,
                .. WireTestKit.Hex(OneSessionRegistryHex),
                .. WireTestKit.Hex(AnticheatStringsHex),
                .. body,
            ]);
        Assert.Equal(packetLength, want.Length / 2);

        string tx = WireTestKit.HexOf(harness.Tx[^1].Bytes);
        Assert.True(tx.EndsWith(want, StringComparison.Ordinal), $"tx ({tx.Length / 2}B) = {tx}");
    }

    /// <summary>
    /// The block is a <c>PLAYER_LIST</c>-only rewrite, like packet 2's leading block. Every other
    /// packet must reach the wire as exactly the wire id plus the body Slipe built — not merely
    /// <i>ending</i> with the body, which a prepended block would still satisfy.
    /// </summary>
    [Fact]
    public void NoOtherPacketGetsTheBlock()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        byte[] body = WireTestKit.Hex(NanoLaptopBodyHex);

        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_PLAYER_PURESYNC,
            0x90,
            body);

        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_PURESYNC);
        string want = WireTestKit.HexOf([wireId, .. body]);

        string tx = WireTestKit.HexOf(harness.Tx[^1].Bytes);
        Assert.Equal(want, tx[^want.Length..]);
    }

    /// <summary>
    /// Reads a bitstream the way the client does: MSB-first within each byte, a <c>ushort</c> length
    /// written low byte first, and string bodies as raw bytes.
    /// </summary>
    private sealed class ClientStreamReader(byte[] data)
    {
        private int bitPosition;

        public int BitPosition => this.bitPosition;

        public bool ReadBit()
        {
            int bit = (data[this.bitPosition >> 3] >> (7 - (this.bitPosition & 7))) & 1;
            this.bitPosition++;
            return bit != 0;
        }

        public ushort ReadUInt16()
        {
            int low = ReadByte();
            int high = ReadByte();
            return (ushort)(low | (high << 8));
        }

        public string ReadString()
        {
            int length = ReadUInt16();
            var bytes = new byte[length];
            for (int i = 0; i < length; i++)
                bytes[i] = (byte)ReadByte();

            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        private int ReadByte()
        {
            int value = 0;
            for (int i = 0; i < 8; i++)
                value = (value << 1) | (ReadBit() ? 1 : 0);

            return value;
        }
    }
}
