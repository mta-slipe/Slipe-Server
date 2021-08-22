﻿using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
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
            blip.OrderingChanged += HandleOrderingChanged;
            blip.VisibleDistanceChanged += HandleVisibleDistanceChanged;
            blip.IconChanged += HandleIconChanged;
            blip.SizeChanged += HandleSizeChanged;
            blip.ColorChanged += HandleColorChanged;
        }

        private void HandleColorChanged(Element sender, ElementChangedEventArgs<Blip, Color> args)
        {
            this.server.BroadcastPacket(new SetBlipColorRpcPacket(args.Source.Id, args.NewValue));
        }

        private void HandleSizeChanged(Element sender, ElementChangedEventArgs<Blip, byte> args)
        {
            throw new System.NotImplementedException();
        }

        private void HandleIconChanged(Element sender, ElementChangedEventArgs<Blip, BlipIcon> args)
        {
            throw new System.NotImplementedException();
        }

        private void HandleVisibleDistanceChanged(Element sender, ElementChangedEventArgs<Blip, ushort> args)
        {
            throw new System.NotImplementedException();
        }

        private void HandleOrderingChanged(Element sender, ElementChangedEventArgs<Blip, short> args)
        {
            throw new System.NotImplementedException();
        }
    }
}
