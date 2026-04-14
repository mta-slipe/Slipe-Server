using SlipeServer.Server.Resources;
using System;
using System.Threading;

namespace SlipeServer.Scripting;

public class ScriptExecutionContext(Resource? Owner)
{
    public Resource? Owner { get; } = Owner;

    public Action<string, object>? SetGlobal { get; set; }
    public Action<string>? RemoveGlobal { get; set; }

    private readonly static AsyncLocal<ScriptExecutionContext?> current = new();
    public static ScriptExecutionContext? Current
    {
        get => current.Value;
        set => current.Value = value;
    }
}
