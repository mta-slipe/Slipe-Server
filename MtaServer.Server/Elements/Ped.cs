using System;
using MtaServer.Packets;
using System.Net;
using MtaServer.Packets.Definitions.Entities.Structs;
using System.Numerics;

namespace MtaServer.Server.Elements
{
    public class Ped: Element
    {
        public override ElementType ElementType => ElementType.Ped;

        public ushort Model { get; set; }
        public float Health { get; set; } = 100;
        public float Armor { get; set; } = 0;
        public PlayerWeapon? CurrentWeapon { get; set; }
        public float PedRotation { get; set; } = 0;
        public Element? Vehicle { get; set; }
        public byte? Seat { get; set; }
        public bool HasJetpack { get; set; } = false;
        public bool IsSyncable { get; set; } = true;
        public bool IsHeadless { get; set; } = false;
        public bool IsFrozen { get; set; } = false;
        public byte Alpha { get; set; } = 255;
        public PedMoveAnimation MoveAnimation { get; set; } = 0;
        public PedClothing[] Clothes { get; set; }
        public PedWeapon[] Weapons { get; set; }


        public Ped(ushort model, Vector3 position): base()
        {
            this.Model = model;
            this.Position = position;

            this.Clothes = new PedClothing[0];
            this.Weapons = new PedWeapon[0];
        }

        public new Ped AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
