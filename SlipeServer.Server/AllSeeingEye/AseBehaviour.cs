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

        this.aseListeners.Add(new AseUdpListener(aseQueryService, logger, (ushort)(configuration.Port + 123), false, configuration.BlockedAseIpAddresses, peerVersionHints));
        if (configuration.DebugPort.HasValue)
            this.aseListeners.Add(new AseUdpListener(aseQueryService, logger, (ushort)(configuration.DebugPort + 123), true, configuration.BlockedAseIpAddresses, peerVersionHints));
    }

    public void Dispose()
    {
        foreach (AseUdpListener listener in this.aseListeners)
            listener.Dispose();

        this.aseListeners.Clear();
        GC.SuppressFinalize(this);
    }
}
