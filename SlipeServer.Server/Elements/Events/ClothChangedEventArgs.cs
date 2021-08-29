using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events
{
    public class ClothChangedEventArgs : EventArgs
    {
        public Ped Ped { get; set; }
        public Clothes Cloth { get; set; }
        public byte Previous { get; set; }
        public byte Current { get; set; }

        public ClothChangedEventArgs(Ped ped, Clothes cloth, byte previous, byte current)
        {
            this.Ped = ped;
            this.Cloth = cloth;
            this.Previous = previous;
            this.Current = current;
        }
    }
}
