using SlipeServer.Server.Resources;
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
    public Resource Resource { get; }

    internal ServerResource(Resource resource, string name, ScriptChunk[] chunks, IReadOnlyDictionary<string, IScriptingService> scriptingServiceByLanguage)
    {
        this.Resource = resource;
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
