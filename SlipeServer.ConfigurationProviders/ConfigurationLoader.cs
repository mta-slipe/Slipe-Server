using SlipeServer.ConfigurationProviders.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.ConfigurationProviders
{
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
                ".toml" => new TomlConfigurationProvider(configPath),
                _ => throw new NotSupportedException($"Unsupported configuration extension {extension}"),
            };
        }
    }
}
