namespace SlipeServer.Scripting;

public class ScriptCallbackDelegateWrapper(ScriptCallbackDelegate callbackDelegate, object function)
{
    public ScriptCallbackDelegate CallbackDelegate { get; init; } = callbackDelegate;
    public object BackingValue { get; init; } = function;

    public override bool Equals(object? obj) => (this.BackingValue == (obj as ScriptCallbackDelegateWrapper)?.BackingValue);

    public override int GetHashCode()
    {
        return this.CallbackDelegate.GetHashCode() + this.BackingValue.GetHashCode();
    }
}
