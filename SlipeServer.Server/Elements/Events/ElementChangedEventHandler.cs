using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public delegate void ElementChangedEventHandler<T>(object sender, ElementChangedEventArgs<T> args);
    public delegate void ElementChangedEventHandler<TSource, TValue>(object sender, ElementChangedEventArgs<TSource, TValue> args);
}
