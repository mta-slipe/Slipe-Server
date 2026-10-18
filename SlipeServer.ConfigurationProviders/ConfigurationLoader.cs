using SlipeServer.ConfigurationProviders.Configurations;
using System;
using System.IO;

namespace SlipeServer.ConfigurationProviders;

public static class ConfigurationLoader
{
    public static IConfigurationProvider GetConfigurationProvider(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file {configPath} does not exist.");
        }

        string extension = Path.GetExtension(configPath);
        return extension switch
        {
            ".json" => new JsonConfigurationProvider(configPath),
            ".xml" => new XmlConfigurationProvider(configPath),
            // MTA's own configuration file is XML with a .conf extension, so it is parsed as such.
            ".conf" => new XmlConfigurationProvider(configPath),
            ".toml" => new TomlConfigurationProvider(configPath),
            _ => throw new NotSupportedException($"Unsupported configuration extension {extension}"),
        };
    }
}
