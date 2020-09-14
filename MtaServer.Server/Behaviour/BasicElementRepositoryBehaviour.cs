using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Server.Elements;
using MtaServer.Server.Repositories;

namespace MtaServer.Server.Behaviour
{
    public class BasicElementRepositoryBehaviour
    {
        private readonly IElementRepository elementRepository;

        public BasicElementRepositoryBehaviour(IElementRepository elementRepository, MtaServer server)
        {
            this.elementRepository = elementRepository;

            server.ElementCreated += OnElementCreate;
        }

        private void OnElementCreate(Element element)
        {
            this.elementRepository.Add(element);
            element.Destroyed += OnElementDestroy;
        }

        private void OnElementDestroy(Element element)
        {
            this.elementRepository.Remove(element);
        }
    }
}
