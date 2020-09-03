using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements.Events
{
    public class ElementChangeEventArgs<TSource, TValue> : EventArgs
    {
        public TSource Source { get; }
        public TValue NewValue { get; }
        public bool IsSync { get; }

        public ElementChangeEventArgs(TSource source, TValue newValue, bool isSync = false)
        {
            Source = source;
            NewValue = newValue;
            IsSync = isSync;
        }
    }

    public class ElementChangeEventArgs<T>: ElementChangeEventArgs<Element, T>
    {
        public ElementChangeEventArgs(Element source, T newValue, bool isSync = false)
            :base(source, newValue, isSync)
        {
        }
    }
}
