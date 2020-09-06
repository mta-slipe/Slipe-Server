using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements.Events
{
    public delegate void ElementChangedEventHandler<T>(object sender, ElementChangedEventArgs<T> e);
}
