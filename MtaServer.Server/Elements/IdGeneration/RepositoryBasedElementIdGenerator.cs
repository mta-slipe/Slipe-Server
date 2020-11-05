using MtaServer.Server.Constants;
using MtaServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements.IdGeneration
{
    public class RepositoryBasedElementIdGenerator: IElementIdGenerator
    {
        private readonly IElementRepository elementRepository;
        private uint idCounter;

        public RepositoryBasedElementIdGenerator(IElementRepository elementRepository)
        {
            this.idCounter = 0;
            this.elementRepository = elementRepository;
        }

        public uint GetId()
        {
            var start = this.idCounter;
            while (this.elementRepository.Get(this.idCounter) != null)
            {
                this.idCounter = (this.idCounter + 1) % ElementConstants.MaxElementId;
                if (idCounter == start)
                    throw new ElementIdsExhaustedException();
            }

            return this.idCounter;
        }
    }
}
