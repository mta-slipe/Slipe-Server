using SlipeServer.Server.Resources;
using System;
using System.Collections.Generic;
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

    private readonly static AsyncLocal<Dictionary<string, object>?> pendingGlobals = new();
    public static Dictionary<string, object>? PendingGlobals
    {
        get => pendingGlobals.Value;
        set => pendingGlobals.Value = value;
    }
}
