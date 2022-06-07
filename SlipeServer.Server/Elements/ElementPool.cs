using SlipeServer.Server.Elements;
using System;
using System.Collections.Concurrent;

namespace SlipeServer.Server.PacketHandling;

public class ElementPool<TElement>
    where TElement : Element
{
    private readonly ConcurrentQueue<TElement> elements;
    private readonly bool returnsWhenDestroyed;
    private readonly int maxElementCount;

    public ElementPool(int maxElementCount = 64, bool returnsWhenDestroyed = true)
    {
        this.elements = new();
        this.maxElementCount = maxElementCount;
        this.returnsWhenDestroyed = returnsWhenDestroyed;
    }

    public TElement GetOrCreateElement(Func<TElement> createCall, Action<TElement>? cleanupCall = null)
    {
        if (this.elements.TryDequeue(out var element))
        {
            cleanupCall?.Invoke(element);
            return element;
        }

        if (!this.returnsWhenDestroyed)
            return createCall();

        var createdElement = createCall();
        createdElement.Destroyed += (_) => ReturnElement(createdElement);
        return createdElement;
    }

    public void ReturnElement(TElement element)
    {
        if (this.elements.Count < this.maxElementCount)
           this.elements.Enqueue(element);
    }
}
