using SlipeServer.Server.Elements;
using System;
using System.Collections.Concurrent;

namespace SlipeServer.Server.PacketHandling;

/// <summary>
/// A pool of elements
/// Element pools can be used to reduce the amount of garbage collection needed by re-using existing elements instead of creating new ones.
/// </summary>
/// <typeparam name="TElement">The type of elements to be stored within this pool</typeparam>
public class ElementPool<TElement>
    where TElement : Element
{
    private readonly ConcurrentQueue<TElement> elements;
    private readonly bool returnsWhenDestroyed;
    private readonly int maxElementCount;

    /// <summary>
    /// Creates an element pool
    /// </summary>
    /// <param name="maxElementCount">The maximum amount of elements to store within this element pool that are not in use</param>
    /// <param name="returnsWhenDestroyed">Whether or not an element should be returned to the element pool (marked as not in use) when it is destroyed</param>
    public ElementPool(int maxElementCount = 64, bool returnsWhenDestroyed = true)
    {
        this.elements = new();
        this.maxElementCount = maxElementCount;
        this.returnsWhenDestroyed = returnsWhenDestroyed;
    }

    /// <summary>
    /// Gets an element from the pool, or a new element if there are none available within the pool.
    /// </summary>
    /// <param name="createCall">Action that will be called to create a new element for the pool</param>
    /// <param name="cleanupCall">Action that will be called to clean up an element in the pool prior to releases it to be reused</param>
    /// <returns></returns>
    public TElement GetOrCreateElement(Func<TElement> createCall, Action<TElement>? cleanupCall = null)
    {
        if (this.elements.TryDequeue(out var element))
        {
            cleanupCall?.Invoke(element);
            element.IsDestroyed = false;
            return element;
        }

        if (!this.returnsWhenDestroyed)
            return createCall();

        var createdElement = createCall();
        createdElement.Destroyed += (_) => ReturnElement(createdElement);
        return createdElement;
    }

    /// <summary>
    /// Returns an element to the element pool, marking it as ready to be re-used
    /// Only returns the element if there is enough space in the pool based on the maximum element count.
    /// </summary>
    public void ReturnElement(TElement element)
    {
        if (this.elements.Count < this.maxElementCount)
           this.elements.Enqueue(element);
    }
}
