using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public delegate void ElementChangedEventHandler<T>(object sender, ElementChangedEventArgs<T> e);
}
