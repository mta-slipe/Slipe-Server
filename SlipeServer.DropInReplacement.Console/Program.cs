using SlipeServer.DropInReplacement;
using SlipeServer.Server;

var server = MtaServer.Create(x =>
{
    x.UseConfiguration(new Configuration()
    {
        Port = 22003,
        HttpPort = 22005,

#if DEBUG
        ResourceDirectory = Path.Join(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName, "SlipeServer.DropInReplacement.Console", "Resources"),
#endif
    });

    x.AddDropInReplacementServer();
});

server.Start();
await Task.Delay(-1);
