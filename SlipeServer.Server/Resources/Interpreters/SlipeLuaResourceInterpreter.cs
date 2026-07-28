using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources.Providers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SlipeServer.Server.Resources.Interpreters;

internal struct Manifest
{
    public string[] Modules { get; set; }
    public string[] Types { get; set; }
    public string[] RequiredAssemblies { get; set; }
}

public class SlipeLuaResourceInterpreter : IResourceInterpreter
{
    private readonly static JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public bool IsFallback => false;

    public bool TryInterpretResource(
        IMtaServer mtaServer,
        IRootElement rootElement,
        string name,
        string path,
        IResourceProvider resourceProvider,
        out Resource? resource
    )
    {
        var files = resourceProvider.GetFilesForResource(name);
        if (!files.Contains("entrypoint.slipe"))
        {
            resource = null;
            return false;
        }

        List<ResourceFile> resourceFiles = GetFilesForSlipeLuaResource(resourceProvider, name, path, files);

        resource = new Resource(mtaServer, rootElement, name, path)
        {
            Files = resourceFiles
        };

        return true;
    }

    private List<ResourceFile> GetFilesForSlipeLuaResource(IResourceProvider resourceProvider, string name, string path, IEnumerable<string> fileNames)
    {
        var files = fileNames.ToDictionary(x => x, file => resourceProvider.GetFileContent(name, file));

        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        var distLuaDirectory = Path.Join("Lua", "Dist");

        resourceFiles.Add(GetResourceFileForPath(files, Path.Join("Lua", "patches.lua")));

        var entrypoint = Encoding.Default.GetString(files["entrypoint.slipe"]);

        var coreSystemManifestText = Encoding.Default.GetString(files[Path.Combine("CoreSystem.Lua", "manifest.json")]);
        var coreSystemManifest = JsonSerializer.Deserialize<Manifest>(coreSystemManifestText, jsonOptions);
        foreach (var module in coreSystemManifest.Modules)
        {
            resourceFiles.Add(GetResourceFileForPath(files, Path.Combine(
                "CoreSystem.Lua",
                "CoreSystem",
                module.Replace('.', Path.DirectorySeparatorChar)) + ".lua"
            ));
        }

        var directory = Path.Combine(distLuaDirectory, entrypoint);
        var entryManifestText = Encoding.Default.GetString(files[Path.Join(directory, "manifest.json")]);
        var entryManifest = JsonSerializer.Deserialize<Manifest>(entryManifestText, jsonOptions);
        foreach (var file in GetResourceFilesForManifest(files, entryManifest, distLuaDirectory, directory))
            resourceFiles.Add(file);

        resourceFiles.Add(GetResourceFileForPath(files, Path.Join("Lua", "main.lua")));

        return resourceFiles;
    }

    private List<ResourceFile> GetResourceFilesForManifest(
        Dictionary<string, byte[]> files,
        Manifest manifest,
        string distLuaDirectory,
        string baseDirectory)
    {
        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        foreach (var assembly in manifest.RequiredAssemblies)
        {
            var directory = Path.Combine(distLuaDirectory, assembly);
            var manifestText = Encoding.Default.GetString(files[Path.Combine(directory, "manifest.json")]);
            var subManifest = JsonSerializer.Deserialize<Manifest>(manifestText, jsonOptions);

            foreach (var file in GetResourceFilesForManifest(files, subManifest, distLuaDirectory, directory))
                resourceFiles.Add(file);
        }

        foreach (var type in manifest.Modules)
            resourceFiles.Add(GetResourceFileForPath(files, Path.Join(baseDirectory, type.Replace('.', Path.DirectorySeparatorChar)) + ".lua"));

        resourceFiles.Add(GetResourceFileForPath(files, Path.Combine(baseDirectory, "manifest.lua")));

        return resourceFiles;
    }

    private ResourceFile GetResourceFileForPath(Dictionary<string, byte[]> files, string path)
    {
        return ResourceFileFactory.FromBytes(files[path], path);
    }
}
