using Microsoft.Extensions.Logging;

namespace SlipeServer.Console.Services;
public class TestService
{
    public TestService(ILogger logger)
    {
        logger.LogInformation("TestService instantiated");
    }
}
