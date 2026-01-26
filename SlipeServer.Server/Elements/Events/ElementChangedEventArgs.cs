using System;

namespace SlipeServer.Server.Elements.Events;

public class ElementChangedEventArgs<TSource, TValue>(TSource source, TValue oldValue, TValue newValue, bool isSync = false) : EventArgs
{
    public TSource Source { get; } = source;
    public TValue OldValue { get; } = oldValue;
    public TValue NewValue { get; } = newValue;
    public bool IsSync { get; } = isSync;
}

public sealed class ElementChangedEventArgs<T>(Element source, T oldValue, T newValue, bool isSync = false) : ElementChangedEventArgs<Element, T>(source, oldValue, newValue, isSync)
{
}
