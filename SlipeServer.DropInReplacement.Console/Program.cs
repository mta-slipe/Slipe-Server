using SlipeServer.DropInReplacement;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Server;

var server = MtaServer.Create(x =>
{
    x.UseConfiguration(new Configuration()
    {
        Port = 22003,
        HttpPort = 22005,

#if DEBUG
        ResourceDirectory = Path.Join(Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.Parent!.FullName, "SlipeServer.DropInReplacement.Console", "Resources")
#else
        ResourceDirectory = "./mods/deathmatch/resources"
#endif
    });

    x.AddDropInReplacementServer();
});

server.Start();

var service = server.GetRequiredService<IDropInReplacementResourceService>();
//service.StartResource("freeroam");
//service.StartResource("play");
service.StartResource("fallout");
await Task.Delay(-1);
