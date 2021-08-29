using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.ElementConcepts
{
    public class Clothing
    {
        private readonly Ped ped;
        public List<PedClothing> PedClothings { get; set; }

        public Clothing(Ped ped)
        {
            this.ped = ped;
            this.PedClothings = new();

            ClothChanged += HandleClothChanged;
        }

        private void HandleClothChanged(Ped sender, ClothChangedEventArgs e)
        {
            this.PedClothings.RemoveAll(p => p.Type == (byte)e.Cloth);
            this.PedClothings.Add(ClothesConstants.ClothesTextureModel[e.Cloth][e.Current]);
        }

        private byte shirt;
        public byte Shirt
        {
            get => this.shirt;
            set
            {
                if(this.ped.Model == 0 && value != this.Shirt)
                {
                    if(value >= 0 && value <= ClothesConstants.ValidClothes[Enums.Clothes.Shirt])
                    {
                        ClothChanged?.Invoke(this.ped, new ClothChangedEventArgs(this.ped, Enums.Clothes.Shirt, this.Shirt, value));
                        this.shirt = value;
                    }
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Shirt}.");
                }
            }
        }

        public event ElementEventHandler<Ped, ClothChangedEventArgs>? ClothChanged;
    }
}
