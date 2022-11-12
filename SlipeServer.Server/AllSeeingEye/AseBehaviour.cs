using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// Behaviour responsible for replying to ASE queries
/// </summary>
public class AseBehaviour
{
    private readonly List<AseUdpListener> aseListeners = new();

    public AseBehaviour(IAseQueryService aseQueryService, Configuration configuration, ILogger logger)
    {
        this.aseListeners.Add(new AseUdpListener(aseQueryService, logger, (ushort)(configuration.Port + 123), false));
        if (configuration.DebugPort.HasValue)
            this.aseListeners.Add(new AseUdpListener(aseQueryService, logger, (ushort)(configuration.DebugPort + 123), true));
    }
}
