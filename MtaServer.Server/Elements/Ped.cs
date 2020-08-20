using System;
using MtaServer.Packets;
using MTAServerWrapper.Server;
using System.Net;

namespace MtaServer.Server.Elements
{
    public class Ped: Element
    {
        public override ElementType ElementType => ElementType.Ped;

        public float PedRotation { get; set; }


        public Ped(): base()
        {

        }
    }
}
