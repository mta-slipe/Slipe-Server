namespace SlipeServer.Scripting;

public enum ResourceState
{
    None,
    Loaded,              // Its been loaded successfully (i.e. meta parsed ok), included resources loaded ok
    Starting,            // The resource is starting
    Running,             // Resource items are running
    Stopping,            // The resource is stopping
}
