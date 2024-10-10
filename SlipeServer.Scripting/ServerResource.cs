using SlipeServer.Server.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting;

public class ServerResource
{
    private readonly string name;
    private readonly ScriptChunk[] chunks;
    private readonly IReadOnlyDictionary<string, IScriptingService> scriptingServiceByLanguage;
    private readonly List<IScript> scripts = [];
    private readonly Dictionary<string, object> globalsCache = [];

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

    public IDisposable PushVariable(string variableName, object value)
    {
        return new TemporarilyGlobalVariable(variableName, value);
    }

    public void Stop()
    {

    }

    public object? GetGlobal(string name, string? language = null)
    {
        if(language == null)
            globalsCache.GetValueOrDefault(name);
        foreach (var script in this.scripts)
        {
            if(script.Language == language)
                return script;
        }
        return null;
    }

    public void SetGlobal(string name, object? value)
    {
        if(value == null)
            this.globalsCache.Remove(name);
        else
            this.globalsCache[name] = value;
        foreach (var script in scripts)
        {
            script.SetGlobal(name, value);
        }
    }

    private struct TemporarilyGlobalVariable : IDisposable
    {
        private readonly string variableName;
        private readonly object? value;

        public TemporarilyGlobalVariable(string variableName, object newValue)
        {
            var resource = ServerResourceContext.Current;
            if (resource == null)
                throw new InvalidOperationException("Can not push variable outside script.");
            this.value = resource.GetGlobal(variableName);
            this.variableName = variableName;

            resource.SetGlobal(variableName, newValue);
        }

        public void Dispose()
        {
            var resource = ServerResourceContext.Current;
            if (resource == null)
                throw new InvalidOperationException("Can not push variable outside script.");

            resource.SetGlobal(this.variableName, this.value);
        }
    }
}
