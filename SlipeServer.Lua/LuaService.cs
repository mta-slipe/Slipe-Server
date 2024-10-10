﻿using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Scripting;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Lua;

public class LuaService
{
    private readonly MtaServer server;
    private readonly ILogger logger;
    private readonly RootElement root;
    private readonly Dictionary<string, Script> scripts;
    private readonly Dictionary<string, LuaMethod> methods;
    private readonly LuaTranslator translator;

    public LuaService(MtaServer server, ILogger logger, RootElement root)
    {
        this.server = server;
        this.logger = logger;
        this.root = root;
        this.scripts = new Dictionary<string, Script>();
        this.methods = new Dictionary<string, LuaMethod>();
        this.translator = new LuaTranslator();
    }

    public void LoadDefinitions(object methodSet)
    {
        foreach (var method in methodSet.GetType().GetMethods()
                .Where(method => method.CustomAttributes
                    .Any(attribute => attribute.AttributeType == typeof(ScriptFunctionDefinitionAttribute))))
        {
            var attribute = method.GetCustomAttribute<ScriptFunctionDefinitionAttribute>();

            if (this.methods.ContainsKey(attribute!.NiceName))
                throw new Exception($"Lua name conflict for '{attribute.NiceName}'");

            var methodParameters = method.GetParameters();
            this.methods[attribute.NiceName] = (values) =>
            {
                var valueQueue = new Queue<DynValue>(values.AsEnumerable());

                object?[] parameters = new object[methodParameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        if (valueQueue.Any())
                        {
                            parameters[i] = this.translator.FromDynValue(methodParameters[i].ParameterType, valueQueue);
                        } else
                        {
                            if (methodParameters[i].IsOptional)
                            {
                                parameters[i] = methodParameters[i].DefaultValue;
                            } else
                            {
                                throw new LuaArgumentException(methodParameters[i].Name!, methodParameters[i].ParameterType, i, DataType.Nil);
                            }
                        }
                    }
                    catch (NotImplementedException)
                    {
                        valueQueue.TryDequeue(out DynValue? valueType);
                        throw new LuaException($"Unsupported Lua value translation for {methodParameters[i].ParameterType}");
                    }
                    catch (Exception e)
                    {
                        throw new LuaException($"Error when converting Lua value to {methodParameters[i].ParameterType}", e);
                    }
                }
                var result = method.Invoke(methodSet, parameters);

                return this.translator.ToDynValues(result).ToArray();
            };
        }
    }

    public void LoadDefinitions<T>()
    {
        LoadDefinitions(this.server.Instantiate<T>()!);
    }

    public void LoadDefaultDefinitions()
    {
        foreach (var type in typeof(ScriptFunctionDefinitionAttribute).Assembly.DefinedTypes
            .Where(type => type.GetMethods()
                .Any(method => method.CustomAttributes
                    .Any(attribute => attribute.AttributeType == typeof(ScriptFunctionDefinitionAttribute)))))
        {
            LoadDefinitions(this.server.Instantiate(type));
        }
    }

    public void LoadScript(string identifier, string code)
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        script.Options.DebugPrint = (value) => this.logger.LogInformation(value);
        this.scripts[identifier] = script;

        LoadGlobals(script);
        LoadDefinitions(script);

        script.DoString(code, codeFriendlyName: identifier);
    }

    public void LoadScript(string identifier, string[] codes)
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        script.Options.DebugPrint = (value) =>
        {
            using var scope = this.logger.BeginScope(script);
            this.logger.LogDebug(value);
        };
        this.scripts[identifier] = script;

        LoadGlobals(script);
        LoadDefinitions(script);

        foreach (var code in codes)
            script.DoString(code, codeFriendlyName: identifier);
    }

    public async Task LoadScriptFromPath(string path) => LoadScript(path, await File.ReadAllTextAsync(path));
    public async Task LoadScriptFromPaths(string identifier, string[] paths)
    {
        var codeTasks = paths.Select(path => File.ReadAllTextAsync(path));
        await Task.WhenAll(codeTasks);
        LoadScript(identifier, codeTasks.Select(task => task.Result).ToArray());
    }

    public void UnloadScript(string identifier)
    {
        this.scripts.Remove(identifier);
    }

    internal void LoadDefinitions(Script script)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var definition in this.methods)
        {
            script.Globals["real" + definition.Key] = definition.Value;
            stringBuilder.AppendLine($"function {definition.Key}(...) return table.unpack(real{definition.Key}({{...}})) end");
        }
        script.DoString(stringBuilder.ToString(), codeFriendlyName: "SlipeDefinitions");
    }

    internal void LoadGlobals(Script script)
    {
        script.Globals["root"] = this.translator.ToDynValues(this.root).First();
        script.Globals["isSlipeServer"] = this.translator.ToDynValues(true).First();
    }

    public delegate DynValue[] LuaMethod(params DynValue[] values);
}
