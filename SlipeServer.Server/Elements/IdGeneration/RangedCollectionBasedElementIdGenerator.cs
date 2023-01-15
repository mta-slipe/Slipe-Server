using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.Elements.IdGeneration;

public class RangedCollectionBasedElementIdGenerator : IElementIdGenerator
{
    private readonly IElementCollection elementCollection;
    private readonly uint start;
    private readonly uint stop;
    private uint idCounter;

    private readonly object idLock = new();

    public RangedCollectionBasedElementIdGenerator(IElementCollection elementCollection, uint start, uint stop)
    {
        this.idCounter = start;
        this.elementCollection = elementCollection;
        this.start = start;
        this.stop = stop;
    }

    public uint GetId()
    {
        lock (this.idLock)
        {
            this.idCounter++;
            var start = this.idCounter;
            while (this.elementCollection.Get(this.idCounter) != null)
            {
                this.idCounter++;
                if (this.idCounter > this.stop)
                    this.idCounter = this.start;
                if (this.idCounter == start)
                    throw new ElementIdsExhaustedException();
            }

            return this.idCounter;
        }
    }
}
