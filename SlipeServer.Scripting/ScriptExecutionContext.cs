using SlipeServer.Server.Resources;
using System.Threading;

namespace SlipeServer.Scripting;

public class ScriptExecutionContext(Resource? Owner)
{
    public Resource? Owner { get; } = Owner;

    private readonly static AsyncLocal<ScriptExecutionContext?> current = new();
    public static ScriptExecutionContext? Current
    {
        get => current.Value;
        set => current.Value = value;
    }
}
