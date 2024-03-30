using SlipeServer.Console.Elements;
using SlipeServer.Console.Services;
using SlipeServer.Server;

namespace SlipeServer.Console.Logic;

public class ServiceUsageTestLogic
{
    public ServiceUsageTestLogic(MtaServer server)
    {
        server.ForAny<CustomPlayer>(player => 
        {
            player.Disconnected += (_, _) => server.GetRequiredServiceScoped<TestService>().HandlePlayerQuit(player);
        });
    }
}
