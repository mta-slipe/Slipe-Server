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

public class LuaService(
    IMtaServer server,
    ILogger logger,
    IRootElement root,
    ScriptTransformationPipeline scriptTransformationPipeline,
    IScriptEventRuntime scriptEventRuntime,
    ScriptTimerService scriptTimerService,
    IScriptInputRuntime scriptInputRuntime,
    LuaEnvironmentService environmentService,
    LuaCallDefinitions callDefinitions)
{
    private readonly Dictionary<string, LuaMethod> methods = [];
    private readonly Dictionary<string, object> globalValues = [];
    private readonly LuaTranslator translator = new(logger);
    private bool callDefinitionsLoaded;
    private bool defaultDefinitionsLoaded;

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
                        throw new ScriptRuntimeException($"Unsupported Lua value translation for {methodParameters[i].ParameterType}. {e.Message}\n{e.StackTrace}");
                    }
                    catch (Exception e)
                    {
                        throw new ScriptRuntimeException($"Error when converting Lua value to {methodParameters[i].ParameterType}. {e.Message}\n{e.StackTrace}");
                    }
                }
                try
                {
                    var result = method.Invoke(methodSet, parameters);

                    return this.translator.ToDynValues(result).ToArray();
                } catch (Exception e)
                {
                    if (e is ScriptRuntimeException)
                        throw;

                    throw new ScriptRuntimeException($"Failed to invoke {attribute?.NiceName} {(e.InnerException ?? e).Message}\n {(e.InnerException ?? e).StackTrace}", e);
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
        if (this.defaultDefinitionsLoaded)
            return;
        this.defaultDefinitionsLoaded = true;

        foreach (var type in typeof(ScriptFunctionDefinitionAttribute).Assembly.DefinedTypes
            .Where(type => type.GetMethods()
                .Any(method => method.CustomAttributes
                    .Any(attribute => attribute.AttributeType == typeof(ScriptFunctionDefinitionAttribute)))))
        {
            LoadDefinitions(server.Instantiate(type));
        }
    }

    public LuaEnvironment CreateEnvironment(string identifier, Resource? resource = null)
    {
        if (!callDefinitionsLoaded)
        {
            LoadDefinitions(callDefinitions);
            callDefinitionsLoaded = true;
        }

        var script = new Script(CoreModules.Preset_SoftSandbox);
        script.Options.DebugPrint = (value) => logger.LogInformation(value);

        LoadGlobals(script);
        LoadDefinitions(script);
        callDefinitions.LoadExports(script);

        var context = new Scripting.ScriptExecutionContext(resource);
        var environment = new LuaEnvironment(identifier, script, context, this.translator, logger, scriptTransformationPipeline, scriptEventRuntime, scriptTimerService, scriptInputRuntime);

        environmentService.RegisterEnvironment(environment);
        return environment;
    }

    public void LoadScript(string identifier, string code, Resource? runningResource = null)
    {
        var environment = CreateEnvironment(identifier, runningResource);
        environment.LoadString(code, identifier);
    }

    public void AddGlobal(string key, object value)
    {
        this.globalValues[key] = value;
    }

    public void RemoveGlobal(string key)
    {
        this.globalValues.Remove(key);
    }

    public void LoadScript(string identifier, string[] codes, Resource? runningResource = null)
    {
        var environment = CreateEnvironment(identifier, runningResource);
        foreach (var code in codes)
            environment.LoadString(code, identifier);
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
        var environment = environmentService.GetEnvironment(identifier);
        environment?.Unload();
        environmentService.UnregisterEnvironment(identifier);
    }

    public void UnloadScriptsFor(Resource runningResource)
    {
        var environment = environmentService.GetEnvironment(runningResource);
        if (environment != null)
        {
            environment.Unload();
            environmentService.UnregisterEnvironment(environment.Identifier);
        }
        else
        {
            scriptEventRuntime.RemoveEventHandlersWithContext(runningResource);
            scriptTimerService.KillTimersWithContext(runningResource);
            scriptInputRuntime.RemoveCommandHandlersWithContext(runningResource);
            scriptInputRuntime.RemoveKeyBindingsWithContext(runningResource);
        }

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
        foreach (var definition in this.methods)
        {
            var method = definition.Value;
            script.Globals[definition.Key] = DynValue.NewCallback((ctx, args) =>
            {
                var results = method(args.GetArray());
                return results.Length switch
                {
                    0 => DynValue.Void,
                    1 => results[0],
                    _ => DynValue.NewTuple(results)
                };
            });
        }
    }

    private void LoadGlobals(Script script)
    {
        script.Globals["root"] = this.translator.ToDynValues(root).First();
        script.Globals["isSlipeServer"] = this.translator.ToDynValues(true).First();
        foreach (var (key, value) in this.globalValues)
            script.Globals[key] = this.translator.ToDynValues(value).First();
    }

    private static string FormatLuaError(ScriptRuntimeException exception, Script? script)
    {
        var sb = new StringBuilder();
        sb.AppendLine(exception.DecoratedMessage);

        if (exception.CallStack is { Count: > 0 })
        {
            foreach (var frame in exception.CallStack.Where(x => x.Name?.Contains("<LoadDefinitions>") != true))
            {
                if (frame.Location is { IsClrLocation: true })
                    continue;

                var location = script != null
                    ? frame.Location?.FormatLocation(script) ?? "?"
                    : frame.Location != null ? $":{frame.Location.FromLine}" : "?";

                sb.AppendLine($"\t{location}: in {frame.Name ?? "main chunk"}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    public delegate DynValue[] LuaMethod(params DynValue[] values);

    public event Action<string>? ScriptErrored
    {
        add => this.translator.ScriptErrored += value;
        remove => this.translator.ScriptErrored -= value;
    }
}
