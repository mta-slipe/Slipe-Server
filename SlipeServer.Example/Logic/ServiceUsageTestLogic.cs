using SlipeServer.Example.Elements;
using SlipeServer.Example.Services;
using SlipeServer.Server;

namespace SlipeServer.Example.Logic;

public class ServiceUsageTestLogic
{

    public ServiceUsageTestLogic(MtaServer server)
    {
        server.ForAny<CustomPlayer>(player => 
        {
            player.Disconnected += (_, _) => server.InstantiateScoped<TestService>().HandlePlayerQuit(player);
        });
    }
}
