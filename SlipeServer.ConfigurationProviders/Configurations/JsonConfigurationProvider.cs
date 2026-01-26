using SlipeServer.Server;
using System.IO;
using System.Text.Json;

namespace SlipeServer.ConfigurationProviders.Configurations;

public class JsonConfigurationProvider(string fileName) : IConfigurationProvider
{
    private readonly static JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Configuration Configuration { private set; get; } = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(fileName), jsonOptions);
    public Configuration GetConfiguration() => this.Configuration;
}
