using SlipeServer.Server.ElementCollections;
using System.Threading;

namespace SlipeServer.Server.Elements.IdGeneration;

public class RangedCollectionBasedElementIdGenerator(IElementCollection elementCollection, uint start, uint stop) : IElementIdGenerator
{
    private readonly uint start = start;
    private uint idCounter = start;

    private readonly Lock idLock = new();

    public uint GetId()
    {
        lock (this.idLock)
        {
            this.idCounter++;
            var start = this.idCounter;
            while (elementCollection.Get(this.idCounter) != null)
            {
                this.idCounter++;
                if (this.idCounter > stop)
                    this.idCounter = this.start;
                if (this.idCounter == start)
                    throw new ElementIdsExhaustedException();
            }

            return this.idCounter;
        }
    }
}
