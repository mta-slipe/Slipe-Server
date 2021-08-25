namespace SlipeServer.Scripting
{
    public class ScriptCallbackDelegateWrapper<TCallbackDelegate>
    {
        public TCallbackDelegate CallbackDelegate { get; init; }
        public object BackingValue { get; init; }

        public ScriptCallbackDelegateWrapper(TCallbackDelegate callbackDelegate, object function)
        {
            this.CallbackDelegate = callbackDelegate;
            this.BackingValue = function;
        }

        public override bool Equals(object? obj) => (this.BackingValue == (obj as ScriptCallbackDelegateWrapper<TCallbackDelegate>)?.BackingValue);

        public override int GetHashCode()
        {
            return this.CallbackDelegate.GetHashCode() + this.BackingValue.GetHashCode();
        }
    }
}
