using SlipeServer.Server;
using System.IO;
using System.Text.Json;

namespace SlipeServer.ConfigurationProviders.Configurations;

public class JsonConfigurationProvider : IConfigurationProvider
{
    private readonly static JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Configuration Configuration { private set; get; }
    public Configuration GetConfiguration() => this.Configuration;

    public JsonConfigurationProvider(string fileName)
    {
        this.Configuration = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(fileName), jsonOptions);
    }
}
