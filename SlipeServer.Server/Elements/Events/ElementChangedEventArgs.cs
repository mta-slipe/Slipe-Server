using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events;

public class ElementChangedEventArgs<TSource, TValue> : EventArgs
{
    public TSource Source { get; }
    public TValue OldValue { get; }
    public TValue NewValue { get; }
    public bool IsSync { get; }

    public ElementChangedEventArgs(TSource source, TValue oldValue, TValue newValue, bool isSync = false)
    {
        this.Source = source;
        this.OldValue = oldValue;
        this.NewValue = newValue;
        this.IsSync = isSync;
    }
}

public class ElementChangedEventArgs<T> : ElementChangedEventArgs<Element, T>
{
    public ElementChangedEventArgs(Element source, T oldValue, T newValue, bool isSync = false)
        : base(source, oldValue, newValue, isSync)
    {
    }
}
