using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.Elements.IdGeneration;

public class CollectionBasedElementIdGenerator : IElementIdGenerator
{
    private readonly IElementCollection elementCollection;
    private uint idCounter;

    public CollectionBasedElementIdGenerator(IElementCollection elementCollection)
    {
        this.idCounter = 1;
        this.elementCollection = elementCollection;
    }

    public uint GetId()
    {
        var start = this.idCounter;
        while (this.elementCollection.Get(this.idCounter) != null)
        {
            this.idCounter = (this.idCounter + 1) % ElementConstants.MaxElementId;
            if (this.idCounter == 0)
                this.idCounter++;
            if (this.idCounter == start)
                throw new ElementIdsExhaustedException();
        }

        return this.idCounter;
    }
}
