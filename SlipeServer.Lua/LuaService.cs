using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Scripting;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Lua;

public class LuaService(IMtaServer server, ILogger logger, IRootElement root, ScriptTransformationPipeline scriptTransformationPipeline, IScriptEventRuntime scriptEventRuntime)
{
    private readonly Dictionary<string, Script> scripts = [];
    private readonly Dictionary<string, LuaMethod> methods = [];
    private readonly Dictionary<string, object> globalValues = [];
    private readonly LuaTranslator translator = new();

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

            var nullContext = new NullabilityInfoContext();
            var nullabilityInfos = methodParameters.Select(p => nullContext.Create(p)).ToArray();

            this.methods[attribute.NiceName] = (values) =>
            {
                var valueQueue = new Queue<DynValue>(values.AsEnumerable());

                object?[] parameters = new object[methodParameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        if (methodParameters[i].IsDefined(typeof(ParamArrayAttribute), false))
                        {
                            var paramType = methodParameters[i].ParameterType.GetElementType();
                            if (paramType != null)
                            {
                                var extraParams = new List<object?>();
                                while (valueQueue.Any())
                                    extraParams.Add(this.translator.FromDynValue(paramType, valueQueue));

                                var typedArray = Array.CreateInstance(paramType, extraParams.Count);
                                Array.Copy(extraParams.ToArray(), typedArray, extraParams.Count);
                                parameters[i] = typedArray;
                            }
                        } else if (valueQueue.Any())
                        {
                            var isNullable = nullabilityInfos[i].ReadState == NullabilityState.Nullable;
                            parameters[i] = this.translator.FromDynValue(methodParameters[i].ParameterType, valueQueue, isNullable);
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
                    catch (NotImplementedException e)
                    {
                        valueQueue.TryDequeue(out DynValue? valueType);
                        throw new LuaException($"Unsupported Lua value translation for {methodParameters[i].ParameterType}. {e.Message}\n{e.StackTrace}");
                    }
                    catch (Exception e)
                    {
                        throw new LuaException($"Error when converting Lua value to {methodParameters[i].ParameterType}. {e.Message}\n{e.StackTrace}", e);
                    }
                }
                try
                {
                    var result = method.Invoke(methodSet, parameters);

                    return this.translator.ToDynValues(result).ToArray();
                } catch (Exception e)
                {
                    throw new Exception($"Failed to load definitions for {attribute?.NiceName} {e.Message}\n {e.StackTrace}", e);
                }
            };
        }
    }

    public void LoadDefinitions<T>()
    {
        LoadDefinitions(server.Instantiate<T>()!);
    }

    public void LoadDefaultDefinitions()
    {
        foreach (var type in typeof(ScriptFunctionDefinitionAttribute).Assembly.DefinedTypes
            .Where(type => type.GetMethods()
                .Any(method => method.CustomAttributes
                    .Any(attribute => attribute.AttributeType == typeof(ScriptFunctionDefinitionAttribute)))))
        {
            LoadDefinitions(server.Instantiate(type));
        }
    }

    public void LoadScript(string identifier, string code, Resource? runningResource = null)
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        script.Options.DebugPrint = (value) => logger.LogInformation(value);
        this.scripts[identifier] = script;

        LoadGlobals(script);
        LoadDefinitions(script);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(code));
        var stream = scriptTransformationPipeline.Transform(ms, "lua");

        Scripting.ScriptExecutionContext.Current = new(runningResource);
        script.DoStream(stream, codeFriendlyName: identifier);
        Scripting.ScriptExecutionContext.Current = null;
    }

    public void AddGlobal(string key, object value)
    {
        this.globalValues[key] = value;
    }

    public void LoadScript(string identifier, string[] codes, Resource? runningResource = null)
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        script.Options.DebugPrint = (value) =>
        {
            using var scope = logger.BeginScope(script);
            logger.LogDebug("{value}", value);
        };
        this.scripts[identifier] = script;

        LoadGlobals(script);
        LoadDefinitions(script);

        Scripting.ScriptExecutionContext.Current = new(runningResource);

        foreach (var code in codes)
            script.DoString(code, codeFriendlyName: identifier);

        Scripting.ScriptExecutionContext.Current = null;
    }

    public async Task LoadScriptFromPath(string path, Resource? runningResource = null) => LoadScript(path, await File.ReadAllTextAsync(path), runningResource);
    public async Task LoadScriptFromPaths(string identifier, string[] paths, Resource? runningResource = null)
    {
        var codeTasks = paths.Select(path => File.ReadAllTextAsync(path));
        await Task.WhenAll(codeTasks);
        LoadScript(identifier, codeTasks.Select(task => task.Result).ToArray(), runningResource);
    }

    public void UnloadScript(string identifier)
    {
        this.scripts.Remove(identifier);
    }

    public void UnloadScriptsFor(Resource runningResource)
    {
        scriptEventRuntime.RemoveEventHandlersWithContext(runningResource);

        static void DestroyChildren(IElement parent)
        {
            foreach (var child in parent.Children.ToArray())
            {
                DestroyChildren(child);
            }

            parent.Destroy();
        }

        foreach (var element in runningResource.DynamicRoot.Children.ToArray() ?? [])
            DestroyChildren(element);
    }

    private void LoadDefinitions(Script script)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var definition in this.methods)
        {
            script.Globals["slipe_" + definition.Key] = definition.Value;
            stringBuilder.AppendLine($"function {definition.Key}(...) return table.unpack(slipe_{definition.Key}({{...}})) end");
        }
        script.DoString(stringBuilder.ToString(), codeFriendlyName: "SlipeDefinitions");
    }

    private void LoadGlobals(Script script)
    {
        script.Globals["root"] = this.translator.ToDynValues(root).First();
        script.Globals["isSlipeServer"] = this.translator.ToDynValues(true).First();
        foreach (var (key, value) in this.globalValues)
            script.Globals[key] = this.translator.ToDynValues(value).First();
    }

    public delegate DynValue[] LuaMethod(params DynValue[] values);
}
