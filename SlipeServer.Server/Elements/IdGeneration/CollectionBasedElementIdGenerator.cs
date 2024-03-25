using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.Elements.IdGeneration;

public class CollectionBasedElementIdGenerator(IElementCollection elementCollection) : IElementIdGenerator
{
    private uint idCounter = 1;
    private readonly object idLock = new();

    public uint GetId()
    {
        lock (this.idLock)
        {
            var start = this.idCounter;
            while (elementCollection.Get(this.idCounter) != null)
            {
                this.idCounter = (this.idCounter + 1) % ElementConstants.MaxElementId;
                if (this.idCounter == 0)
                    this.idCounter++;
                if (this.idCounter == start)
                    throw new ElementIdsExhaustedException();
            }

            var id = this.idCounter;
            this.idCounter = (this.idCounter + 1) % ElementConstants.MaxElementId;
            return id;
        }
    }
}
