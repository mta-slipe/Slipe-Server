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

        public Clothing(Ped ped)
        {
            this.ped = ped;
        }

        public IEnumerable<PedClothing> GetClothing()
        {
            if(this.Shirt != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Shirt][this.Shirt];
            if(this.Head != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Head][this.Head];
            if(this.Trousers != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Trousers][this.Trousers];
            if(this.Shoes != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Shoes][this.Shoes];
            if(this.TattoosLeftUpperArm != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosLeftUpperArm][this.TattoosLeftUpperArm];
            if(this.TattoosLeftLowerArm != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosLeftLowerArm][this.TattoosLeftLowerArm];
            if(this.TattoosRightUpperArm != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosRightUpperArm][this.TattoosRightUpperArm];
            if(this.TattoosRightLowerArm != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosRightLowerArm][this.TattoosRightLowerArm];
            if(this.TattoosBack != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosBack][this.TattoosBack];
            if(this.TattoosLeftChest != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosLeftChest][this.TattoosLeftChest];
            if(this.TattoosRightChest != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosRightChest][this.TattoosRightChest];
            if(this.TattoosStomach != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosStomach][this.TattoosStomach];
            if(this.TattoosLowerBack != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.TattoosLowerBack][this.TattoosLowerBack];
            if(this.Necklace != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Necklace][this.Necklace];
            if(this.Watch != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Watches][this.Watch];
            if(this.Glasses != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Glasses][this.Glasses];
            if(this.Hat != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Hats][this.Hat];
            if(this.Extra != 255)
                yield return ClothesConstants.ClothesTextureModel[Enums.Clothes.Extra][this.Extra];
        }

        private byte shirt = 255;
        public byte Shirt
        {
            get => this.shirt;
            set
            {
                if (this.ped.Model == 0 && value != this.Shirt)
                {
                    if (value >= 0 && value <= ClothesConstants.ShirtsCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Shirt, this.Shirt, value));
                        this.shirt = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Shirt}.");
                    }
                }
            }
        }

        private byte head = 255;
        public byte Head
        {
            get => this.head;
            set
            {
                if (this.ped.Model == 0 && value != this.Head)
                {
                    if (value >= 0 && value <= ClothesConstants.HeadsCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Head, this.Head, value));
                        this.head = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Head}.");
                    }
                }
            }
        }

        private byte trousers = 255;
        public byte Trousers
        {
            get => this.trousers;
            set
            {
                if (this.ped.Model == 0 && value != this.Trousers)
                {
                    if (value >= 0 && value <= ClothesConstants.TrousersCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Trousers, this.Trousers, value));
                        this.trousers = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Trousers}.");
                    }
                }
            }
        }

        private byte shoes = 255;
        public byte Shoes
        {
            get => this.shoes;
            set
            {
                if (this.ped.Model == 0 && value != this.Shoes)
                {
                    if (value >= 0 && value <= ClothesConstants.ShoesCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Shoes, this.Shoes, value));
                        this.shoes = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Shoes}.");
                    }
                }
            }
        }

        private byte tattoosLeftUpperArm = 255;
        public byte TattoosLeftUpperArm
        {
            get => this.tattoosLeftUpperArm;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosLeftUpperArm)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosLeftUpperArmCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosLeftUpperArm, this.TattoosLeftUpperArm, value));
                        this.tattoosLeftUpperArm = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosLeftUpperArm}.");
                    }
                }
            }
        }

        private byte tattoosLeftLowerArm = 255;
        public byte TattoosLeftLowerArm
        {
            get => this.tattoosLeftLowerArm;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosLeftUpperArm)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosLeftLowerArmCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosLeftLowerArm, this.TattoosLeftLowerArm, value));
                        this.tattoosLeftLowerArm = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosLeftLowerArm}.");
                    }
                }
            }
        }

        private byte tattoosRightUpperArm = 255;
        public byte TattoosRightUpperArm
        {
            get => this.tattoosRightUpperArm;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosRightUpperArm)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosRightUpperArmCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosRightUpperArm, this.TattoosRightUpperArm, value));
                        this.tattoosRightUpperArm = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosRightUpperArm}.");
                    }
                }
            }
        }

        private byte tattoosRightLowerArm = 255;
        public byte TattoosRightLowerArm
        {
            get => this.tattoosRightLowerArm;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosRightLowerArm)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosRightLowerArmCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosRightLowerArm, this.TattoosRightLowerArm, value));
                        this.tattoosRightLowerArm = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosRightLowerArm}.");
                    }
                }
            }
        }

        private byte tattoosBack = 255;
        public byte TattoosBack
        {
            get => this.tattoosBack;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosBack)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosBackCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosBack, this.TattoosBack, value));
                        this.tattoosBack = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosBack}.");
                    }
                }
            }
        }

        private byte tattoosLeftChest = 255;
        public byte TattoosLeftChest
        {
            get => this.tattoosLeftChest;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosLeftChest)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosLeftChestCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosLeftChest, this.TattoosLeftChest, value));
                        this.tattoosLeftChest = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosLeftChest}.");
                    }
                }
            }
        }

        private byte tattoosRightChest = 255;
        public byte TattoosRightChest
        {
            get => this.tattoosRightChest;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosRightChest)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosRightChestCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosRightChest, this.TattoosRightChest, value));
                        this.tattoosRightChest = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosRightChest}.");
                    }
                }
            }
        }

        private byte tattoosStomach = 255;
        public byte TattoosStomach
        {
            get => this.tattoosStomach;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosStomach)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosStomachCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosStomach, this.TattoosStomach, value));
                        this.tattoosStomach = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosStomach}.");
                    }
                }
            }
        }

        private byte tattoosLowerBack = 255;
        public byte TattoosLowerBack
        {
            get => this.tattoosLowerBack;
            set
            {
                if (this.ped.Model == 0 && value != this.TattoosLowerBack)
                {
                    if (value >= 0 && value <= ClothesConstants.TattoosLowerBackCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.TattoosLowerBack, this.TattoosLowerBack, value));
                        this.tattoosLowerBack = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.TattoosLowerBack}.");
                    }
                }
            }
        }

        private byte necklace = 255;
        public byte Necklace
        {
            get => this.necklace;
            set
            {
                if (this.ped.Model == 0 && value != this.Necklace)
                {
                    if (value >= 0 && value <= ClothesConstants.NecklaceCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Necklace, this.Necklace, value));
                        this.necklace = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Necklace}.");
                    }
                }
            }
        }

        private byte watch = 255;
        public byte Watch
        {
            get => this.watch;
            set
            {
                if (this.ped.Model == 0 && value != this.Watch)
                {
                    if (value >= 0 && value <= ClothesConstants.WatchesCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Watches, this.Watch, value));
                        this.watch = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Watches}.");
                    }
                }
            }
        }

        private byte glasses = 255;
        public byte Glasses
        {
            get => this.glasses;
            set
            {
                if (this.ped.Model == 0 && value != this.Glasses)
                {
                    if (value >= 0 && value <= ClothesConstants.GlassesCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Glasses, this.glasses, value));
                        this.glasses = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Glasses}.");
                    }
                }
            }
        }

        private byte hat = 255;
        public byte Hat
        {
            get => this.hat;
            set
            {
                if (this.ped.Model == 0 && value != this.Hat)
                {
                    if (value >= 0 && value <= ClothesConstants.HatsCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Hats, this.hat, value));
                        this.hat = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Hats}.");
                    }
                }
            }
        }

        private byte extra = 255;
        public byte Extra
        {
            get => this.extra;
            set
            {
                if (this.ped.Model == 0 && value != this.Extra)
                {
                    if (value >= 0 && value <= ClothesConstants.ExtraCount)
                    {
                        ClothChanged?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.Clothes.Extra, this.extra, value));
                        this.extra = value;
                    } else
                    {
                        throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.Clothes.Extra}.");
                    }
                }
            }
        }

        public event ElementEventHandler<Ped, ClothingChangedEventArgs>? ClothChanged;
    }
}
