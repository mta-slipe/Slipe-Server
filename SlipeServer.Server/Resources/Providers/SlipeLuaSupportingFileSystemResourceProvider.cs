using Force.Crc32;
using Newtonsoft.Json;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace SlipeServer.Server.Resources.Providers;

struct Manifest
{
    public string[] Modules { get; set; }
    public string[] Types { get; set; }
    public string[] RequiredAssemblies { get; set; }
}

public class SlipeLuaSupportingFileSystemResourceProvider : IResourceProvider
{
    private readonly MtaServer mtaServer;
    private readonly RootElement rootElement;
    private readonly Configuration configuration;
    private readonly Dictionary<string, Resource> resources;
    private ushort netId = 0;

    public SlipeLuaSupportingFileSystemResourceProvider(MtaServer mtaServer, RootElement rootElement, Configuration configuration)
    {
        this.mtaServer = mtaServer;
        this.rootElement = rootElement;
        this.configuration = configuration;
        this.resources = new();
        this.Refresh();
    }

    public Resource GetResource(string name)
    {
        return this.resources[name];
    }

    public IEnumerable<Resource> GetResources()
    {
        return this.resources.Values;
    }

    public void Refresh()
    {
        this.resources.Clear();
        var resources = IndexResourceDirectory(this.configuration.ResourceDirectory);

        foreach (var resource in resources)
            this.resources[resource.Name] = resource;
    }

    private IEnumerable<Resource> IndexResourceDirectory(string directory)
    {
        List<Resource> resources = new();

        if (!Directory.Exists(directory))
            return resources;

        var directories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
        foreach (var subDirectory in directories)
        {
            if (subDirectory.StartsWith("[") && subDirectory.EndsWith("]"))
                foreach (var resource in IndexResourceDirectory(subDirectory))
                    resources.Add(resource);

            var name = Path.GetFileName(subDirectory)!;
            if (this.resources.ContainsKey(name))
            {
                resources.Add(this.resources[name]);
            } else
            {
                var resource = new Resource(this.mtaServer, this.rootElement, name, subDirectory)
                {
                    NetId = this.ReserveNetId(),
                    Files = GetFilesForResource(subDirectory)
                };
                resources.Add(resource);
            }
        }

        return resources;
    }

    public List<ResourceFile> GetFilesForResource(string path)
    {
        if (File.Exists(Path.Combine(path, "entrypoint.slipe")))
            return GetFilesForSlipeLuaResource(path);
        return GetFilesForSimpleResource(path);
    }

    private List<ResourceFile> GetFilesForSimpleResource(string path)
    {
        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        using (var md5 = MD5.Create())
        {
            foreach (var file in Directory.GetFiles(path))
            {
                resourceFiles.Add(GetResourceFileForPath(file, path));
            }
        }

        return resourceFiles;
    }

    private List<ResourceFile> GetFilesForSlipeLuaResource(string path)
    {
        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        var distLuaDirectory = Path.Combine(path, "Lua/Dist");

        resourceFiles.Add(GetResourceFileForPath(Path.Join(path, "Lua", "patches.lua"), path));

        var entrypoint = File.ReadAllText(Path.Combine(path, "entrypoint.slipe"));

        var coreSystemManifestText = File.ReadAllText(Path.Combine(path, "CoreSystem.lua", "manifest.json"));
        var coreSystemManifest = JsonConvert.DeserializeObject<Manifest>(coreSystemManifestText);
        foreach (var module in coreSystemManifest.Modules)
        {
            resourceFiles.Add(GetResourceFileForPath(Path.Combine(
                path, 
                "CoreSystem.lua", 
                "CoreSystem", 
                module.Replace('.', Path.DirectorySeparatorChar)) + ".lua", 
            path));
        }

        var directory = Path.Combine(distLuaDirectory, entrypoint);
        var entryManifestText = File.ReadAllText(Path.Combine(directory, "manifest.json"));
        var entryManifest = JsonConvert.DeserializeObject<Manifest>(entryManifestText);
        foreach (var file in GetResourceFilesForManifest(entryManifest, distLuaDirectory, directory, path))
            resourceFiles.Add(file);

        resourceFiles.Add(GetResourceFileForPath(Path.Join(path, "Lua", "main.lua"), path));

        return resourceFiles;
    }

    private List<ResourceFile> GetResourceFilesForManifest(
        Manifest manifest, 
        string distLuaDirectory, 
        string baseDirectory, 
        string relativeTo)
    {
        List<ResourceFile> resourceFiles = new List<ResourceFile>();

        foreach (var assembly in manifest.RequiredAssemblies)
        {
            var directory = Path.Combine(distLuaDirectory, assembly);
            var manifestText = File.ReadAllText(Path.Combine(directory, "manifest.json"));
            var subManifest = JsonConvert.DeserializeObject<Manifest>(manifestText);

            foreach (var file in GetResourceFilesForManifest(subManifest, distLuaDirectory, directory, relativeTo))
                resourceFiles.Add(file);
        }

        foreach (var type in manifest.Modules)
            resourceFiles.Add(GetResourceFileForPath(Path.Join(baseDirectory, type.Replace('.', Path.DirectorySeparatorChar)) + ".lua", relativeTo));

        resourceFiles.Add(GetResourceFileForPath(Path.Combine(baseDirectory, "manifest.lua"), relativeTo));

        return resourceFiles;
    }

    private ResourceFile GetResourceFileForPath(string path, string relativeTo)
    {
        using var md5 = MD5.Create();
        byte[] content = File.ReadAllBytes(path);
        var hash = md5.ComputeHash(content);
        var checksum = Crc32Algorithm.Compute(content);

        string fileName = Path.GetRelativePath(relativeTo, path);
        var fileType = fileName.EndsWith(".lua") ? ResourceFileType.ClientScript : ResourceFileType.ClientFile;
        return new ResourceFile()
        {
            Name = fileName,
            AproximateSize = content.Length,
            IsAutoDownload = fileType == ResourceFileType.ClientFile ? true : null,
            CheckSum = checksum,
            FileType = (byte)fileType,
            Md5 = hash
        };
    }

    public ushort ReserveNetId() => this.netId++;
}
