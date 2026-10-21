using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SlipeServer.Net.Wrappers;
using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Enums;
using SlipeServer.Server.ServerBuilders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Ase;

/// <summary>
/// A client aborts its connection unless the ASE <c>ver</c> string it reads equals the one its own
/// build was compiled with ("1.6" for release, "1.6n" for debug), so a listener that answers a
/// single fixed family can never admit both builds. These tests cover the per-peer mechanism that
/// lets one listener answer each peer in the family that peer was built for.
/// </summary>
public class AsePerPeerVersionTests
{
    private static readonly IPAddress peerA = IPAddress.Parse("203.0.113.7");
    private static readonly IPAddress peerB = IPAddress.Parse("198.51.100.9");

    private sealed class FakeAseQueryService : IAseQueryService
    {
        public List<VersionType> LightQueries { get; } = [];

        public bool ShowPlayers { get; set; }

        public string? GetRule(string key) => null;

        public bool RemoveRule(string key) => false;

        public void SetRule(string key, string value) { }

        public string GetVersion(AseVersion version = AseVersion.v1_6) =>
            version == AseVersion.v1_6n ? "1.6n" : "1.6";

        public byte[] QueryFull(ushort port) => [(byte)'E', (byte)'Y', (byte)'E', (byte)'3', (byte)'f'];

        public byte[] QueryLight(ushort port, VersionType version = VersionType.Release)
        {
            this.LightQueries.Add(version);
            byte[] ver = System.Text.Encoding.ASCII.GetBytes(this.GetVersion(
                version == VersionType.Release ? AseVersion.v1_6 : AseVersion.v1_6n));

            return [(byte)'E', (byte)'Y', (byte)'E', (byte)'2', .. ver];
        }

        public byte[] QueryXFireLight() => [(byte)'E', (byte)'Y', (byte)'E', (byte)'1'];
    }

    private static string Describe(byte[] data) => System.Text.Encoding.ASCII.GetString(data);

    private static AseUdpListener CreateListener(
        FakeAseQueryService queryService,
        IPeerVersionHints? hints,
        bool isDebug,
        Func<ulong>? clock)
    {
        // Port 0 binds an ephemeral port: the listener needs a socket to exist, but these tests
        // drive the reply path through the send callback and never put anything on the wire.
        return new AseUdpListener(queryService, NullLogger.Instance, 0, isDebug, [], hints, clock);
    }

    /// <summary>
    /// Finds an unused UDP port to use as the ASE port. <see cref="AseBehaviour"/> listens on
    /// <see cref="Configuration.Port"/> + 123, so the game port is derived from it.
    /// </summary>
    private static ushort ReserveAsePort()
    {
        using Socket probe = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return (ushort)((IPEndPoint)probe.LocalEndPoint!).Port;
    }

    // ---- the version-type digit ------------------------------------------------------------

    [Theory]
    [InlineData("1.6.0-9.24149.0", MtaPeerBuildType.Release)]
    [InlineData("1.6.0-7.24149.0", MtaPeerBuildType.Release)]
    [InlineData("1.6.0-5.24155.0", MtaPeerBuildType.Debug)]
    [InlineData("1.6.0-3.24155.0", MtaPeerBuildType.Debug)]
    [InlineData("1.6.0-1.24155.0", MtaPeerBuildType.Debug)]
    [InlineData("1.6.0-5", MtaPeerBuildType.Debug)]
    public void ParsesTheVersionTypeDigit(string version, MtaPeerBuildType expected)
    {
        MtaPeerBuildTypeParser.TryParsePlayerVersion(version, out MtaPeerBuildType build).Should().BeTrue();
        build.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1.6.0")]
    [InlineData("1.6.0-")]
    [InlineData("1.6.0-4.24149.0")]
    [InlineData("1.6.0-x.24149.0")]
    [InlineData("not a version")]
    public void RejectsVersionsWithoutAKnownVersionType(string? version)
    {
        MtaPeerBuildTypeParser.TryParsePlayerVersion(version, out _).Should().BeFalse();
    }

    // ---- the hint store --------------------------------------------------------------------

    [Fact]
    public void HintIsReadableAfterBeingRecorded()
    {
        PeerVersionHints hints = new();

        hints.Record(peerA, MtaPeerBuildType.Debug);

        hints.TryGet(peerA, out MtaPeerBuildType build).Should().BeTrue();
        build.Should().Be(MtaPeerBuildType.Debug);
    }

    [Fact]
    public void UnknownPeerHasNoHint()
    {
        PeerVersionHints hints = new();
        hints.Record(peerA, MtaPeerBuildType.Release);

        hints.TryGet(peerB, out _).Should().BeFalse();
    }

    [Fact]
    public void HintExpiresAfterItsTimeToLive()
    {
        ulong now = 1000;
        PeerVersionHints hints = new(TimeSpan.FromSeconds(5), () => now);

        hints.Record(peerA, MtaPeerBuildType.Debug);

        now += 4999;
        hints.TryGet(peerA, out _).Should().BeTrue();

        now += 1;
        hints.TryGet(peerA, out _).Should().BeFalse();
    }

    [Fact]
    public void RecordingAgainReplacesTheHint()
    {
        PeerVersionHints hints = new();

        hints.Record(peerA, MtaPeerBuildType.Release);
        hints.Record(peerA, MtaPeerBuildType.Debug);

        hints.TryGet(peerA, out MtaPeerBuildType build).Should().BeTrue();
        build.Should().Be(MtaPeerBuildType.Debug);
    }

    [Fact]
    public void ClearDiscardsEveryHint()
    {
        PeerVersionHints hints = new();
        hints.Record(peerA, MtaPeerBuildType.Release);
        hints.Record(peerB, MtaPeerBuildType.Debug);

        hints.Clear();

        hints.TryGet(peerA, out _).Should().BeFalse();
        hints.TryGet(peerB, out _).Should().BeFalse();
    }

    // ---- the hold queue --------------------------------------------------------------------

    [Fact]
    public void HeldQueryIsNotReleasedBeforeItsDeadline()
    {
        ulong now = 0;
        AsePendingReplies pending = new(TimeSpan.FromMilliseconds(500), () => now);

        pending.Enqueue(new IPEndPoint(peerA, 1234), AseQueryType.LightRelease);

        now = 499;
        pending.TakeExpired().Should().BeEmpty();
        pending.Count.Should().Be(1);
    }

    [Fact]
    public void HeldQueryIsReleasedOnceAtItsDeadline()
    {
        ulong now = 0;
        AsePendingReplies pending = new(TimeSpan.FromMilliseconds(500), () => now);
        pending.Enqueue(new IPEndPoint(peerA, 1234), AseQueryType.LightRelease);

        now = 500;

        IReadOnlyList<PendingAseQuery> expired = pending.TakeExpired();
        expired.Should().HaveCount(1);
        expired[0].Source.Should().Be(new IPEndPoint(peerA, 1234));
        expired[0].QueryType.Should().Be(AseQueryType.LightRelease);

        pending.TakeExpired().Should().BeEmpty();
        pending.Count.Should().Be(0);
    }

    [Fact]
    public void HeldQueriesAreReleasedOldestFirst()
    {
        ulong now = 0;
        AsePendingReplies pending = new(TimeSpan.FromMilliseconds(100), () => now);

        pending.Enqueue(new IPEndPoint(peerA, 1), AseQueryType.LightRelease);
        now = 50;
        pending.Enqueue(new IPEndPoint(peerB, 2), AseQueryType.LightRelease);

        now = 200;

        pending.TakeExpired().Should().SatisfyRespectively(
            first => first.Source.Port.Should().Be(1),
            second => second.Source.Port.Should().Be(2));
    }

    [Fact]
    public void HoldQueueIsBoundedAndDropsTheOldestEntry()
    {
        AsePendingReplies pending = new(TimeSpan.FromMinutes(1));

        for (int i = 0; i < AsePendingReplies.MaxPending + 1; i++)
            pending.Enqueue(new IPEndPoint(peerA, i), AseQueryType.LightRelease);

        pending.Count.Should().Be(AsePendingReplies.MaxPending);
    }

    // ---- the listener ----------------------------------------------------------------------

    [Fact]
    public void ProbeIsAnsweredImmediatelyWhenThePeerBuildIsKnown()
    {
        FakeAseQueryService queryService = new();
        PeerVersionHints hints = new();
        hints.Record(peerA, MtaPeerBuildType.Debug);
        using AseUdpListener listener = CreateListener(queryService, hints, isDebug: false, clock: null);

        List<byte[]> sent = [];
        listener.HandleQuery(AseQueryType.LightRelease, new IPEndPoint(peerA, 1234), sent.Add);

        sent.Should().HaveCount(1);
        Describe(sent[0]).Should().EndWith("1.6n");
        listener.PendingReplyCount.Should().Be(0);
    }

    [Fact]
    public void ProbeIsAnsweredImmediatelyInTheListenersOwnFamilyWithoutHints()
    {
        FakeAseQueryService queryService = new();
        using AseUdpListener listener = CreateListener(queryService, hints: null, isDebug: false, clock: null);

        List<byte[]> sent = [];
        listener.HandleQuery(AseQueryType.LightRelease, new IPEndPoint(peerA, 1234), sent.Add);

        sent.Should().HaveCount(1);
        Describe(sent[0]).Should().EndWith("1.6");
        listener.PendingReplyCount.Should().Be(0);
    }

    [Fact]
    public void ProbeIsHeldUntilThePeerBuildIsKnown()
    {
        ulong now = 0;
        FakeAseQueryService queryService = new();
        PeerVersionHints hints = new();
        using AseUdpListener listener = CreateListener(queryService, hints, isDebug: false, clock: () => now);

        List<byte[]> sent = [];
        listener.HandleQuery(AseQueryType.LightRelease, new IPEndPoint(peerA, 1234), sent.Add);

        sent.Should().BeEmpty("the peer's build is not known yet");
        listener.PendingReplyCount.Should().Be(1);

        // The join data arrives after the probe and states the build.
        hints.Record(peerA, MtaPeerBuildType.Debug);
        now += (ulong)AseUdpListener.HintHoldDuration.TotalMilliseconds;

        List<byte[]> flushed = [];
        listener.FlushExpiredReplies((data, _) => flushed.Add(data));

        flushed.Should().HaveCount(1);
        Describe(flushed[0]).Should().EndWith("1.6n");
        listener.PendingReplyCount.Should().Be(0);
    }

    [Fact]
    public void HeldProbeFallsBackToTheListenersFamilyWhenNoBuildArrives()
    {
        ulong now = 0;
        FakeAseQueryService queryService = new();
        PeerVersionHints hints = new();
        using AseUdpListener listener = CreateListener(queryService, hints, isDebug: false, clock: () => now);

        listener.HandleQuery(AseQueryType.LightRelease, new IPEndPoint(peerA, 1234), _ => { });
        now += (ulong)AseUdpListener.HintHoldDuration.TotalMilliseconds;

        List<byte[]> flushed = [];
        listener.FlushExpiredReplies((data, _) => flushed.Add(data));

        flushed.Should().HaveCount(1);
        Describe(flushed[0]).Should().EndWith("1.6");
    }

    [Fact]
    public void HeldRepliesAreTailoredPerPeer()
    {
        ulong now = 0;
        FakeAseQueryService queryService = new();
        PeerVersionHints hints = new();
        using AseUdpListener listener = CreateListener(queryService, hints, isDebug: false, clock: () => now);

        listener.HandleQuery(AseQueryType.LightRelease, new IPEndPoint(peerA, 1234), _ => { });
        listener.HandleQuery(AseQueryType.LightRelease, new IPEndPoint(peerB, 5678), _ => { });

        hints.Record(peerA, MtaPeerBuildType.Debug);
        hints.Record(peerB, MtaPeerBuildType.Release);
        now += (ulong)AseUdpListener.HintHoldDuration.TotalMilliseconds;

        List<string> flushed = [];
        listener.FlushExpiredReplies((data, _) => flushed.Add(Describe(data)));

        flushed.Should().HaveCount(2);
        flushed[0].Should().EndWith("1.6n");
        flushed[1].Should().EndWith("1.6");
    }

    [Fact]
    public void QueriesWithoutAVersionFieldAreAnsweredImmediately()
    {
        FakeAseQueryService queryService = new();
        PeerVersionHints hints = new();
        using AseUdpListener listener = CreateListener(queryService, hints, isDebug: false, clock: null);

        List<byte[]> sent = [];
        listener.HandleQuery(AseQueryType.Full, new IPEndPoint(peerA, 1234), sent.Add);

        sent.Should().HaveCount(1);
        listener.PendingReplyCount.Should().Be(0);
    }

    // ---- wiring ----------------------------------------------------------------------------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BehaviourResolvesWhetherOrNotAHintStoreIsRegistered(bool registerHints)
    {
        ushort asePort = ReserveAsePort();
        Configuration configuration = new() { Port = (ushort)(asePort - 123), AsePerPeerVersion = registerHints };

        ServiceCollection services = new();
        services.AddSingleton<IAseQueryService>(new FakeAseQueryService());
        services.AddSingleton(configuration);
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<AseBehaviour>>(NullLogger<AseBehaviour>.Instance);
        if (registerHints)
            services.AddSingleton<IPeerVersionHints, PeerVersionHints>();

        using ServiceProvider provider = services.BuildServiceProvider();
        using AseBehaviour behaviour = ActivatorUtilities.CreateInstance<AseBehaviour>(provider);

        behaviour.Should().NotBeNull();
    }

    [Fact]
    public void DisabledByDefaultSoTheListenerKeepsOneFixedFamily()
    {
        new Configuration().AsePerPeerVersion.Should().BeFalse();
    }

    [Fact]
    public void BuilderParameterTurnsTheFeatureOn()
    {
        ServerBuilder builder = new(withDependencyLoaders: false);

        builder.AddManagedNetworking("127.0.0.1", 22003, asePerPeerVersion: true);

        builder.Configuration.AsePerPeerVersion.Should().BeTrue();
    }

    [Fact]
    public void BuilderParameterOverridesTheConfigurationEitherWay()
    {
        ServerBuilder on = new(withDependencyLoaders: false);
        on.UseConfiguration(new Configuration { AsePerPeerVersion = true });
        on.AddManagedNetworking("127.0.0.1", 22003, asePerPeerVersion: false);
        on.Configuration.AsePerPeerVersion.Should().BeFalse();

        ServerBuilder off = new(withDependencyLoaders: false);
        off.AddManagedNetworking("127.0.0.1", 22003, asePerPeerVersion: true);
        off.Configuration.AsePerPeerVersion.Should().BeTrue();
    }

    [Fact]
    public void OmittingTheBuilderParameterLeavesTheConfigurationInCharge()
    {
        ServerBuilder builder = new(withDependencyLoaders: false);
        builder.UseConfiguration(new Configuration { AsePerPeerVersion = true });

        builder.AddManagedNetworking("127.0.0.1", 22003);

        builder.Configuration.AsePerPeerVersion.Should().BeTrue();
    }

    [Fact]
    public void ManagedNetWrapperExtensionForwardsTheFlag()
    {
        ServerBuilder builder = new(withDependencyLoaders: false);

        builder.AddManagedNetWrapper(asePerPeerVersion: true);

        builder.Configuration.AsePerPeerVersion.Should().BeTrue();
    }
}
