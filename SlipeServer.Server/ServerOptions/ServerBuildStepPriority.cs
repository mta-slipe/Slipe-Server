namespace SlipeServer.Server.ServerOptions;

public enum ServerBuildStepPriority : int
{
    Low = 0,
    Medium = 1000,
    High = 2000,

    Default = Medium
}
