using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events
{
    public class ClothingChangedEventArgs : EventArgs
    {
        public Ped Ped { get; set; }
        public Clothes Clothes { get; set; }
        public byte Previous { get; set; }
        public byte Current { get; set; }

        public ClothingChangedEventArgs(Ped ped, Clothes clothes, byte previous, byte current)
        {
            this.Ped = ped;
            this.Clothes = clothes;
            this.Previous = previous;
            this.Current = current;
        }
    }
}
