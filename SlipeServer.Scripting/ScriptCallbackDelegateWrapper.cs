namespace SlipeServer.Scripting
{
    public class ScriptCallbackDelegateWrapper
    {
        public ScriptCallbackDelegate CallbackDelegate { get; init; }
        public object BackingValue { get; init; }

        public ScriptCallbackDelegateWrapper(ScriptCallbackDelegate callbackDelegate, object function)
        {
            this.CallbackDelegate = callbackDelegate;
            this.BackingValue = function;
        }

        public override bool Equals(object? obj) => (this.BackingValue == (obj as ScriptCallbackDelegateWrapper)?.BackingValue);

        public override int GetHashCode()
        {
            return this.CallbackDelegate.GetHashCode() + this.BackingValue.GetHashCode();
        }
    }
}
