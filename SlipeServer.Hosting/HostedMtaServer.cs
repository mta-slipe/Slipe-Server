namespace SlipeServer.Hosting;

public class HostedMtaServer(MtaServer server)
{
    public MtaServer Server { get; } = server;

    public void Start() => this.Server.Start();

    public void Stop() => this.Server.Stop();
}
