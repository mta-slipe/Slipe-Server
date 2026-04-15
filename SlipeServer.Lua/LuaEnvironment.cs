using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Scripting;
using SlipeServer.Server.Resources;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SlipeServer.Lua;

public class LuaEnvironment
{
    private readonly Script script;
    private readonly LuaTranslator translator;
    private readonly object scriptLock = new();
    private readonly ILogger logger;
    private readonly ScriptTransformationPipeline scriptTransformationPipeline;
    private readonly IScriptEventRuntime scriptEventRuntime;
    private readonly ScriptTimerService scriptTimerService;
    private readonly IScriptInputRuntime scriptInputRuntime;

    public string Identifier { get; }
    public Scripting.ScriptExecutionContext ExecutionContext { get; }

    internal LuaEnvironment(
        string identifier,
        Script script,
        Scripting.ScriptExecutionContext executionContext,
        LuaTranslator translator,
        ILogger logger,
        ScriptTransformationPipeline scriptTransformationPipeline,
        IScriptEventRuntime scriptEventRuntime,
        ScriptTimerService scriptTimerService,
        IScriptInputRuntime scriptInputRuntime
    )
    {
        this.Identifier = identifier;
        this.ExecutionContext = executionContext;
        this.script = script;
        this.translator = translator;
        this.logger = logger;
        translator.RegisterEnvironment(this.script, this);
        this.scriptTransformationPipeline = scriptTransformationPipeline;
        this.scriptEventRuntime = scriptEventRuntime;
        this.scriptTimerService = scriptTimerService;
        this.scriptInputRuntime = scriptInputRuntime;

        if (executionContext.Owner != null)
        {
            SetGlobal("resource", executionContext.Owner);
            SetGlobal("resourceRoot", executionContext.Owner.Root);
        }

        executionContext.SetGlobal = (key, value) => SetGlobal(key, value);
        executionContext.RemoveGlobal = (key) => RemoveGlobal(key);

        this.script.DoString(MtaOopPrelude.Full, null, "mta-oop-prelude");
    }

    internal void EnterScriptLock() => Monitor.Enter(this.scriptLock);
    internal void ExitScriptLock() => Monitor.Exit(this.scriptLock);

    public void LoadString(string code, string? codeFriendlyName = null)
    {
        var previous = Scripting.ScriptExecutionContext.Current;
        Scripting.ScriptExecutionContext.Current = ExecutionContext;
        try
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(code));
            var stream = this.scriptTransformationPipeline.Transform(ms, "lua");
            this.script.DoStream(stream, codeFriendlyName: codeFriendlyName ?? Identifier);
        }
        catch (ScriptRuntimeException exception)
        {
            this.logger.LogError("{Lua error}", FormatLuaError(exception));
            throw;
        }
        catch (LuaException exception)
        {
            this.logger.LogError("{Lua error}", exception.Message);
            throw;
        }
        finally
        {
            Scripting.ScriptExecutionContext.Current = previous;
        }
    }

    public void SetGlobal(string key, object value)
    {
        this.script.Globals[key] = this.translator.ToDynValues(value).First();
    }

    public void RemoveGlobal(string key)
    {
        this.script.Globals.Remove(key);
    }

    public DynValue[] CallFunction(string functionName, params object[] args)
    {
        var luaFunction = this.script.Globals.Get(functionName);
        if (luaFunction.Type != DataType.Function)
            throw new ArgumentException($"Function '{functionName}' not found in Lua environment '{Identifier}'");

        var previous = Scripting.ScriptExecutionContext.Current;
        Scripting.ScriptExecutionContext.Current = ExecutionContext;
        try
        {
            var dynArgs = args.SelectMany(this.translator.ToDynValues).ToArray();
            var result = luaFunction.Function.Call(dynArgs);
            return result.Type == DataType.Tuple ? result.Tuple : [result];
        }
        catch (ScriptRuntimeException e)
        {
            this.logger.LogError(e, "Error calling '{Function}' in '{Identifier}': {DecoratedMessage}", functionName, Identifier, e.DecoratedMessage);
            return [];
        }
        finally
        {
            Scripting.ScriptExecutionContext.Current = previous;
        }
    }

    internal DynValue[] CallWithSource(string functionName, Resource? sourceResource, DynValue[] args)
    {
        var luaFunction = this.script.Globals.Get(functionName);
        if (luaFunction.Type != DataType.Function)
            throw new ArgumentException($"Function '{functionName}' not found in Lua environment '{Identifier}'");

        var previous = Scripting.ScriptExecutionContext.Current;
        Scripting.ScriptExecutionContext.Current = ExecutionContext;

        var prevSourceResource = this.script.Globals.Get("sourceResource");
        var prevSourceResourceRoot = this.script.Globals.Get("sourceResourceRoot");

        if (sourceResource != null)
        {
            this.script.Globals["sourceResource"] = UserData.Create(sourceResource);
            this.script.Globals["sourceResourceRoot"] = this.translator.ToDynValues(sourceResource.DynamicRoot).First();
        }

        try
        {
            var result = luaFunction.Function.Call(args);
            return result.Type == DataType.Tuple ? result.Tuple : [result];
        }
        finally
        {
            Scripting.ScriptExecutionContext.Current = previous;
            this.script.Globals["sourceResource"] = prevSourceResource;
            this.script.Globals["sourceResourceRoot"] = prevSourceResourceRoot;
        }
    }

    public void Unload()
    {
        this.scriptEventRuntime.RemoveEventHandlersWithContext(ExecutionContext.Owner);
        this.scriptTimerService.KillTimersWithContext(ExecutionContext.Owner);
        this.scriptInputRuntime.RemoveCommandHandlersWithContext(ExecutionContext.Owner);
        this.scriptInputRuntime.RemoveKeyBindingsWithContext(ExecutionContext.Owner);
    }

    private string FormatLuaError(ScriptRuntimeException exception)
    {
        var sb = new StringBuilder();
        sb.AppendLine(exception.DecoratedMessage);

        if (exception.CallStack is { Count: > 0 })
        {
            foreach (var frame in exception.CallStack.Where(x => x.Name?.Contains("<LoadDefinitions>") != true))
            {
                if (frame.Location is { IsClrLocation: true })
                    continue;

                var location = frame.Location?.FormatLocation(this.script) ?? "?";
                sb.AppendLine($"\t{location}: in {frame.Name ?? "main chunk"}");
            }
        }

        return sb.ToString().TrimEnd();
    }
}
