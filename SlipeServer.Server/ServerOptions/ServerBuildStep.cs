using System;

namespace SlipeServer.Server.ServerOptions;

public struct ServerBuildStep
{
    public Action<MtaServer> Step { get; init; }
    public ServerBuildStepPriority Priority { get; init; }

    public ServerBuildStep(Action<MtaServer> step, ServerBuildStepPriority priority)
    {
        this.Step = step;
        this.Priority = priority;
    }
}
