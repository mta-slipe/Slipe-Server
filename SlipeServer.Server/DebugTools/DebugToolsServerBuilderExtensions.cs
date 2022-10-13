using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Server.DebugTools;
public static class DebugToolsServerBuilderExtensions
{
    public static void AddDebugFileSystemResourceProvider(this ServerBuilder builder, string projectName)
    {
        builder.ConfigureServices(x =>
        {
            x.AddSingleton<IResourceProvider, FileSystemResourceProvider>();
        });
    }
}
