using Microsoft.Extensions.Logging;
using SlipeServer.Net.Wrappers;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// Behaviour responsible for replying to ASE queries, these are the queries used to populate the master server list, this listen on the server port + 123
/// If the server configuration's debug port is specified ASE queries will also be listened to on the debug port + 123
/// </summary>
public class AseBehaviour : IDisposable
{
    private readonly List<AseUdpListener> aseListeners = [];

    public AseBehaviour(
        IAseQueryService aseQueryService,
        Configuration configuration,
        ILogger<AseBehaviour> logger,
        IPeerVersionHints? peerVersionHints = null)
    {
        if (!configuration.AsePerPeerVersion)
            peerVersionHints = null;

        this.aseListeners.Add(new AseUdpListener(aseQueryService, logger, ToAsePort(configuration.Port, nameof(configuration.Port)), false, configuration.BlockedAseIpAddresses, peerVersionHints));
        if (configuration.DebugPort.HasValue)
            this.aseListeners.Add(new AseUdpListener(aseQueryService, logger, ToAsePort(configuration.DebugPort.Value, nameof(configuration.DebugPort)), true, configuration.BlockedAseIpAddresses, peerVersionHints));
    }

    /// <summary>Maps a game port to its ASE port, failing rather than wrapping past the UDP port range.</summary>
    internal static ushort ToAsePort(ushort gamePort, string parameterName)
    {
        int asePort = gamePort + AseUdpListener.AsePortOffset;

        if (asePort > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                gamePort,
                $"The game port must be at most {ushort.MaxValue - AseUdpListener.AsePortOffset} so its ASE port ({AseUdpListener.AsePortOffset} higher) fits in a UDP port.");
        }

        return (ushort)asePort;
    }

    public void Dispose()
    {
        foreach (AseUdpListener listener in this.aseListeners)
            listener.Dispose();

        this.aseListeners.Clear();
        GC.SuppressFinalize(this);
    }
}
