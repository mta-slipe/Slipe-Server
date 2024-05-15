using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;
using SlipeServer.Server.ServerBuilders;
using System.Reflection;

namespace SlipeServer.WebHostBuilderExample;

public static class ServerBuilderExtensions
{
    public static void AddSampleResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new SampleResource(server);
            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSampleResources();
        });
    }

    public static IServiceCollection AddSampleResources(this IServiceCollection services)
    {
        return services;
    }
}

internal class SampleResource : Resource
{
    public static class ResourceFiles
    {
        public static byte[] Sample { get; } = GetLuaFile("SlipeServer.WebHostBuilderExample.Resources.Sample.Sample.lua");
        private static byte[] GetLuaFile(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(name);

            if (stream == null)
                throw new FileNotFoundException($"File \"{name}\" not found in embedded resources.");

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }

    public Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["Sample.lua"] = ResourceFiles.Sample,
    };

    internal SampleResource(MtaServer server) : base(server, server.RootElement, "Sample")
    {
        this.NoClientScripts["foo.lua"] = System.Text.UTF8Encoding.UTF8.GetBytes("outputChatBox('sample')");

        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
