using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements.Events
{
    public class ElementChangedEventArgs<TSource, TValue> : EventArgs
    {
        public TSource Source { get; }
        public TValue NewValue { get; }
        public bool IsSync { get; }

        public ElementChangedEventArgs(TSource source, TValue newValue, bool isSync = false)
        {
            Source = source;
            NewValue = newValue;
            IsSync = isSync;
        }
    }

    public class ElementChangedEventArgs<T>: ElementChangedEventArgs<Element, T>
    {
        public ElementChangedEventArgs(Element source, T newValue, bool isSync = false)
            :base(source, newValue, isSync)
        {
        }
    }
}
