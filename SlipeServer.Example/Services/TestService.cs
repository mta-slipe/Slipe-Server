using Microsoft.Extensions.Logging;

namespace SlipeServer.Example.Services;

public class TestService
{
    private readonly ILogger logger;

    public TestService(ILogger logger)
    {
        this.logger = logger;
        logger.LogInformation("TestService instantiated");
    }

    public void HandleBlurLevel(CustomPlayer player, int blurLevel)
    {
        this.logger.LogInformation("{player} has blur level {level}", player, blurLevel);
    }

    public void HandlePlayerQuit(CustomPlayer player)
    {
        this.logger.LogInformation("{player} has quit :(", player);
    }
}
