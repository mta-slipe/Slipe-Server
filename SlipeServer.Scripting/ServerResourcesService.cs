using Microsoft.Extensions.Logging;
using SlipeServer.Server.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting;

public class ServerResource
{
    private readonly string name;
    private readonly List<IScript> scripts = [];

    public string Name => this.name;
    internal ServerResource(string name)
    {
        this.name = name;
    }

    public void AddScripts(IEnumerable<IScript> scripts)
    {
        this.scripts.AddRange(scripts);
    }

    public void Start()
    {

    }

    public void Stop()
    {

    }
}

public class ServerResourcesService
{
    private readonly Dictionary<string, IScriptingService> scriptingServicesByLanguage;
    private readonly ILogger<ServerResourcesService> logger;

    private readonly List<ServerResource> serverResources = [];

    public ServerResourcesService(IEnumerable<IScriptingService> scriptingServices, ILogger<ServerResourcesService> logger)
    {
        this.scriptingServicesByLanguage = scriptingServices
            .ToDictionary(x => x.Language, x => x);
        this.logger = logger;
    }

    public ServerResource Create(ServerResourceFiles serverResourceFiles)
    {
        var chunksByLanguage = serverResourceFiles.Chunks.GroupBy(x => x.Language);

        var resource = new ServerResource(serverResourceFiles.Name);

        ServerResourceContext.Current = resource;
        try
        {
            var scripts = new List<IScript>();
            foreach (var chunks in chunksByLanguage)
            {
                var scriptingService = this.scriptingServicesByLanguage[chunks.Key];

                var script = scriptingService.CreateScript();
                foreach (var chunk in chunks)
                {
                    script.LoadCode(chunk.Content, chunk.Name);
                }
            }

            resource.AddScripts(scripts);
            this.serverResources.Add(resource);
        }
        catch(ScriptingException ex)
        {
            this.logger.LogError(ex, "Failed to load resource {resourceName}, {message}", resource.Name, ex.Message);
        }
        finally
        {
            ServerResourceContext.Current = null;
        }

        return resource;
    }
}
