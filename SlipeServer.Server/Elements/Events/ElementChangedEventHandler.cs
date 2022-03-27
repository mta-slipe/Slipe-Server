namespace SlipeServer.Server.Elements.Events;

public delegate void ElementChangedEventHandler<T>(Element sender, ElementChangedEventArgs<T> args);
public delegate void ElementChangedEventHandler<TSource, TValue>(TSource sender, ElementChangedEventArgs<TSource, TValue> args);
