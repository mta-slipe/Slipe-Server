using Microsoft.Extensions.Logging;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting;

public class ServerResource
{
    private readonly string name;
    private readonly ScriptChunk[] chunks;
    private readonly IReadOnlyDictionary<string, IScriptingService> scriptingServiceByLanguage;
    private readonly List<IScript> scripts = [];

    public string Name => this.name;
    public IEnumerable<ScriptChunk> Chunks => this.chunks;

    internal ServerResource(string name, ScriptChunk[] chunks, IReadOnlyDictionary<string, IScriptingService> scriptingServiceByLanguage)
    {
        this.name = name;
        this.chunks = chunks;
        this.scriptingServiceByLanguage = scriptingServiceByLanguage;
    }

    public void AddScripts(IEnumerable<IScript> scripts)
    {
        this.scripts.AddRange(scripts);
    }

    public void Start()
    {
        ServerResourceContext.Current = this;
        try
        {
            this.scripts.Clear();

            var chunksByLanguage = this.chunks.GroupBy(x => x.Language);

            foreach (var chunks in chunksByLanguage)
            {
                var scriptingService = this.scriptingServiceByLanguage[chunks.Key];

                var script = scriptingService.CreateScript();
                foreach (var chunk in chunks)
                {
                    script.LoadCode(chunk.Content, chunk.Name);
                }
                this.scripts.Add(script);
            }
        }
        finally
        {
            ServerResourceContext.Current = null;
        }
    }

    public void Stop()
    {

    }
}

public class ServerResourceService
{
    private readonly IReadOnlyDictionary<string, IScriptingService> scriptingServicesByLanguage;
    private readonly ILogger<ServerResourceService> logger;
    private readonly IResourceProvider resourceProvider;
    private readonly List<ServerResource> serverResources = [];
    private readonly List<ServerResource> startedResources = [];

    public ServerResourceService(IEnumerable<IScriptingService> scriptingServices, ILogger<ServerResourceService> logger, IResourceProvider resourceProvider)
    {
        this.scriptingServicesByLanguage = scriptingServices
            .ToDictionary(x => x.Language, x => x);
        this.logger = logger;
        this.resourceProvider = resourceProvider;
    }

    public ServerResource? GetResource(string name) => serverResources.FirstOrDefault(x => x.Name == name);

    public ServerResource Create(ServerResourceFiles serverResourceFiles)
    {
        var resource = new ServerResource(serverResourceFiles.Name, serverResourceFiles.Chunks, this.scriptingServicesByLanguage);

        this.serverResources.Add(resource);

        return resource;
    }

    public ServerResource? StartResource(string name)
    {
        if (!this.startedResources.Any(r => r.Name == name))
        {
            var resource = this.serverResources.Single(r => r.Name == name);
            resource.Start();
            this.startedResources.Add(resource);

            return resource;
        }
        return null;
    }

    public void StopResource(string name)
    {
        if (this.startedResources.Any(r => r.Name == name))
        {
            var resource = this.startedResources.Single(r => r.Name == name);
            this.startedResources.Remove(resource);
            resource.Stop();
        }
    }

}
