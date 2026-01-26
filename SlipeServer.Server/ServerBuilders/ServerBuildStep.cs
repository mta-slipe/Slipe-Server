using System;

namespace SlipeServer.Server.ServerBuilders;

public readonly struct ServerBuildStep(Action<IMtaServer> step, ServerBuildStepPriority priority)
{
    public Action<IMtaServer> Step { get; init; } = step;
    public ServerBuildStepPriority Priority { get; init; } = priority;
}
