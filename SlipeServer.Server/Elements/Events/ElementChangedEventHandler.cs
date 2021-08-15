using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public delegate void ElementChangedEventHandler<T>(Element sender, ElementChangedEventArgs<T> args);
    public delegate void ElementChangedEventHandler<TSource, TValue>(Element sender, ElementChangedEventArgs<TSource, TValue> args);
}
