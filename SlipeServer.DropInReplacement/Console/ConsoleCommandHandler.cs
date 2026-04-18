using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.DropInReplacement.Console;

public class ConsoleCommandHandler(IDropInReplacementResourceService resourceService, IResourceProvider resourceProvider)
{
    public void Handle(string input)
    {
        var splits = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = splits[0];

        switch (command)
        {
            case "start":
                if (splits.Length < 2)
                {
                    System.Console.WriteLine("Usage: start <resourceName>");
                    break;
                }

                resourceService.StartResource(splits[1]);
                break;

            case "stop":
                if (splits.Length < 2)
                {
                    System.Console.WriteLine("Usage: stop <resourceName>");
                    break;
                }

                resourceService.StopResource(splits[1]);
                break;

            case "restart":
                if (splits.Length < 2)
                {
                    System.Console.WriteLine("Usage: restart <resourceName>");
                    break;
                }

                resourceService.RestartResource(splits[1]);
                break;

            case "refresh":
                resourceProvider.Refresh();
                break;
        }
    }
}
