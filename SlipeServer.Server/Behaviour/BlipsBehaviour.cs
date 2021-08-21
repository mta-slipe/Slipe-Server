using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Repositories;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Behaviour
{
    public class BlipsBehaviour
    {
        private readonly MtaServer server;
        private readonly HashSet<Blip> blips;

        public BlipsBehaviour(MtaServer server, IElementRepository elementRepository)
        {
            this.server = server;
            this.blips = new HashSet<Blip>();
            foreach (var blip in elementRepository.GetByType<Blip>(ElementType.Blip))
            {
                AddBlip(blip);
            }

            server.ElementCreated += OnElementCreate;
        }

        private void OnElementCreate(Element element)
        {
            if (element is Blip blip)
            {
                AddBlip(blip);
            }
        }

        private void AddBlip(Blip blip)
        {
            this.blips.Add(blip);
            blip.Destroyed += (source) => this.blips.Remove(blip);
        }
    }
}
