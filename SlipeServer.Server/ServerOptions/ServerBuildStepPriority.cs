namespace SlipeServer.Server.ServerBuilders;

public enum ServerBuildStepPriority : int
{
    Low = 0,
    Medium = 1000,
    High = 2000,

    Default = Medium
}
