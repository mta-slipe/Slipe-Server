using System;

namespace SlipeServer.Server.Elements.Events
{
    public delegate void ElementEventHandler<T>(Element sender, T e) where T: EventArgs;
    public delegate void ElementEventHandler(Element sender);
}
