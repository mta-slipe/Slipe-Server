using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementCollections;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.IdGeneration;

public class RepositoryBasedElementIdGenerator : IElementIdGenerator
{
    private readonly IElementCollection elementRepository;
    private uint idCounter;

    public RepositoryBasedElementIdGenerator(IElementCollection elementRepository)
    {
        this.idCounter = 1;
        this.elementRepository = elementRepository;
    }

    public uint GetId()
    {
        var start = this.idCounter;
        while (this.elementRepository.Get(this.idCounter) != null)
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
