using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.ElementConcepts;

public class Clothing
{
    private readonly Ped ped;

    public Clothing(Ped ped)
    {
        this.ped = ped;
    }

    public IEnumerable<PedClothing> GetClothing()
    {
        if (this.Shirt != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Shirt][this.Shirt];
        if (this.Head != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Head][this.Head];
        if (this.Trousers != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Trousers][this.Trousers];
        if (this.Shoes != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Shoes][this.Shoes];
        if (this.TattoosLeftUpperArm != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosLeftUpperArm][this.TattoosLeftUpperArm];
        if (this.TattoosLeftLowerArm != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosLeftLowerArm][this.TattoosLeftLowerArm];
        if (this.TattoosRightUpperArm != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosRightUpperArm][this.TattoosRightUpperArm];
        if (this.TattoosRightLowerArm != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosRightLowerArm][this.TattoosRightLowerArm];
        if (this.TattoosBack != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosBack][this.TattoosBack];
        if (this.TattoosLeftChest != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosLeftChest][this.TattoosLeftChest];
        if (this.TattoosRightChest != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosRightChest][this.TattoosRightChest];
        if (this.TattoosStomach != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosStomach][this.TattoosStomach];
        if (this.TattoosLowerBack != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.TattoosLowerBack][this.TattoosLowerBack];
        if (this.Necklace != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Necklace][this.Necklace];
        if (this.Watch != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Watches][this.Watch];
        if (this.Glasses != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Glasses][this.Glasses];
        if (this.Hat != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Hats][this.Hat];
        if (this.Extra != 255)
            yield return ClothesConstants.ClothesTextureModel[Enums.ClothingType.Extra][this.Extra];
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Shirt, this.Shirt, value));
                    this.shirt = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Shirt}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Head, this.Head, value));
                    this.head = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Head}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Trousers, this.Trousers, value));
                    this.trousers = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Trousers}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Shoes, this.Shoes, value));
                    this.shoes = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Shoes}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosLeftUpperArm, this.TattoosLeftUpperArm, value));
                    this.tattoosLeftUpperArm = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosLeftUpperArm}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosLeftLowerArm, this.TattoosLeftLowerArm, value));
                    this.tattoosLeftLowerArm = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosLeftLowerArm}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosRightUpperArm, this.TattoosRightUpperArm, value));
                    this.tattoosRightUpperArm = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosRightUpperArm}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosRightLowerArm, this.TattoosRightLowerArm, value));
                    this.tattoosRightLowerArm = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosRightLowerArm}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosBack, this.TattoosBack, value));
                    this.tattoosBack = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosBack}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosLeftChest, this.TattoosLeftChest, value));
                    this.tattoosLeftChest = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosLeftChest}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosRightChest, this.TattoosRightChest, value));
                    this.tattoosRightChest = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosRightChest}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosStomach, this.TattoosStomach, value));
                    this.tattoosStomach = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosStomach}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.TattoosLowerBack, this.TattoosLowerBack, value));
                    this.tattoosLowerBack = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.TattoosLowerBack}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Necklace, this.Necklace, value));
                    this.necklace = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Necklace}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Watches, this.Watch, value));
                    this.watch = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Watches}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Glasses, this.glasses, value));
                    this.glasses = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Glasses}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Hats, this.hat, value));
                    this.hat = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Hats}.");
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
                    Changed?.Invoke(this.ped, new ClothingChangedEventArgs(this.ped, Enums.ClothingType.Extra, this.extra, value));
                    this.extra = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid cloth id for {Enums.ClothingType.Extra}.");
                }
            }
        }
    }

    public event ElementEventHandler<Ped, ClothingChangedEventArgs>? Changed;
}
