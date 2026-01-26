using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.ElementConcepts;

/// <summary>
/// Represents a player's clothing, only used when the player has the CJ model.
/// </summary>
public class Clothing(Ped ped)
{

    /// <summary>
    /// Returns an enumerable of the player's clothing. This will not include any slots that do not have a value.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<PedClothing> GetClothing()
    {
        yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Shirt][this.Shirt];
        yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Head][this.Head];
        yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Trousers][this.Trousers];
        yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Shoes][this.Shoes];

        if (this.TattoosLeftUpperArm.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosLeftUpperArm][this.TattoosLeftUpperArm.Value];
        if (this.TattoosLeftLowerArm.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosLeftLowerArm][this.TattoosLeftLowerArm.Value];
        if (this.TattoosRightUpperArm.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosRightUpperArm][this.TattoosRightUpperArm.Value];
        if (this.TattoosRightLowerArm.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosRightLowerArm][this.TattoosRightLowerArm.Value];
        if (this.TattoosBack.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosBack][this.TattoosBack.Value];
        if (this.TattoosLeftChest.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosLeftChest][this.TattoosLeftChest.Value];
        if (this.TattoosRightChest.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosRightChest][this.TattoosRightChest.Value];
        if (this.TattoosStomach.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosStomach][this.TattoosStomach.Value];
        if (this.TattoosLowerBack.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.TattoosLowerBack][this.TattoosLowerBack.Value];
        if (this.Necklace.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Necklace][this.Necklace.Value];
        if (this.Watch.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Watches][this.Watch.Value];
        if (this.Glasses.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Glasses][this.Glasses.Value];
        if (this.Hat.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Hats][this.Hat.Value];
        if (this.Extra.HasValue)
            yield return ClothingConstants.ClothesTextureModel[Enums.ClothingType.Extra][this.Extra.Value];
    }

    /// <summary>
    /// Adds an article of clothing to the player
    /// </summary>
    public void AddClothing(PedClothing clothing)
    {
        var type = (ClothingType)clothing.Type;
        var value = (byte)Array.FindIndex(ClothingConstants.ClothesTextureModel[type], x => x.Model == clothing.Model && x.Texture == clothing.Texture);
        switch (type)
        {
            case ClothingType.Shirt:
                this.Shirt = value;
                break;
            case ClothingType.Head:
                this.Head = value;
                break;
            case ClothingType.Trousers:
                this.Trousers = value;
                break;
            case ClothingType.Shoes:
                this.Shoes = value;
                break;
            case ClothingType.TattoosLeftUpperArm:
                this.TattoosLeftUpperArm = value;
                break;
            case ClothingType.TattoosLeftLowerArm:
                this.TattoosLeftLowerArm = value;
                break;
            case ClothingType.TattoosRightUpperArm:
                this.TattoosRightUpperArm = value;
                break;
            case ClothingType.TattoosRightLowerArm:
                this.TattoosRightLowerArm = value;
                break;
            case ClothingType.TattoosBack:
                this.TattoosBack = value;
                break;
            case ClothingType.TattoosLeftChest:
                this.TattoosLeftChest = value;
                break;
            case ClothingType.TattoosRightChest:
                this.TattoosRightChest = value;
                break;
            case ClothingType.TattoosStomach:
                this.TattoosStomach = value;
                break;
            case ClothingType.TattoosLowerBack:
                this.TattoosLowerBack = value;
                break;
            case ClothingType.Necklace:
                this.Necklace = value;
                break;
            case ClothingType.Watches:
                this.Watch = value;
                break;
            case ClothingType.Glasses:
                this.Glasses = value;
                break;
            case ClothingType.Hats:
                this.Hat = value;
                break;
            case ClothingType.Extra:
                this.Extra = value;
                break;
        }
    }

    /// <summary>
    /// Removes an article of clothing from the player based on the clothing type
    /// </summary>
    /// <param name="type">The clothing type to remove</param>
    public void RemoveClothing(ClothingType type)
    {
        switch (type)
        {
            case ClothingType.Shirt:
                this.Shirt = ClothingConstants.Defaults[type];
                break;
            case ClothingType.Head:
                this.Head = ClothingConstants.Defaults[type];
                break;
            case ClothingType.Trousers:
                this.Trousers = ClothingConstants.Defaults[type];
                break;
            case ClothingType.Shoes:
                this.Shoes = ClothingConstants.Defaults[type];
                break;
            case ClothingType.TattoosLeftUpperArm:
                this.TattoosLeftUpperArm = null;
                break;
            case ClothingType.TattoosLeftLowerArm:
                this.TattoosLeftLowerArm = null;
                break;
            case ClothingType.TattoosRightUpperArm:
                this.TattoosRightUpperArm = null;
                break;
            case ClothingType.TattoosRightLowerArm:
                this.TattoosRightLowerArm = null;
                break;
            case ClothingType.TattoosBack:
                this.TattoosBack = null;
                break;
            case ClothingType.TattoosLeftChest:
                this.TattoosLeftChest = null;
                break;
            case ClothingType.TattoosRightChest:
                this.TattoosRightChest = null;
                break;
            case ClothingType.TattoosStomach:
                this.TattoosStomach = null;
                break;
            case ClothingType.TattoosLowerBack:
                this.TattoosLowerBack = null;
                break;
            case ClothingType.Necklace:
                this.Necklace = null;
                break;
            case ClothingType.Watches:
                this.Watch = null;
                break;
            case ClothingType.Glasses:
                this.Glasses = null;
                break;
            case ClothingType.Hats:
                this.Hat = null;
                break;
            case ClothingType.Extra:
                this.Extra = null;
                break;
        }
    }

    /// <summary>
    /// Resets the player clothing to the default values
    /// </summary>
    public void Reset()
    {
        this.Shirt = ClothingConstants.Defaults[ClothingType.Shirt];
        this.Head = ClothingConstants.Defaults[ClothingType.Head];
        this.Trousers = ClothingConstants.Defaults[ClothingType.Trousers];
        this.Shoes = ClothingConstants.Defaults[ClothingType.Shoes];
        this.TattoosLeftUpperArm = null;
        this.TattoosLeftLowerArm = null;
        this.TattoosRightUpperArm = null;
        this.TattoosRightLowerArm = null;
        this.TattoosBack = null;
        this.TattoosLeftChest = null;
        this.TattoosRightChest = null;
        this.TattoosStomach = null;
        this.TattoosLowerBack = null;
        this.Necklace = null;
        this.Watch = null;
        this.Glasses = null;
        this.Hat = null;
        this.Extra = null;
    }

    private byte shirt = ClothingConstants.Defaults[ClothingType.Shirt];
    public byte Shirt
    {
        get => this.shirt;
        set
        {
            if (ped.Model == 0 && value != this.Shirt)
            {
                if (value >= 0 && value <= ClothingConstants.ShirtsCount)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Shirt, this.Shirt, value));
                    this.shirt = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Shirt}.");
                }
            }
        }
    }

    private byte head = ClothingConstants.Defaults[ClothingType.Head];
    public byte Head
    {
        get => this.head;
        set
        {
            if (ped.Model == 0 && value != this.Head)
            {
                if (value >= 0 && value <= ClothingConstants.HeadsCount)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Head, this.Head, value));
                    this.head = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Head}.");
                }
            }
        }
    }

    private byte trousers = ClothingConstants.Defaults[ClothingType.Trousers];
    public byte Trousers
    {
        get => this.trousers;
        set
        {
            if (ped.Model == 0 && value != this.Trousers)
            {
                if (value >= 0 && value <= ClothingConstants.TrousersCount)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Trousers, this.Trousers, value));
                    this.trousers = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Trousers}.");
                }
            }
        }
    }

    private byte shoes = ClothingConstants.Defaults[ClothingType.Shoes];
    public byte Shoes
    {
        get => this.shoes;
        set
        {
            if (ped.Model == 0 && value != this.Shoes)
            {
                if (value >= 0 && value <= ClothingConstants.ShoesCount)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Shoes, this.Shoes, value));
                    this.shoes = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Shoes}.");
                }
            }
        }
    }

    private byte? tattoosLeftUpperArm;
    public byte? TattoosLeftUpperArm
    {
        get => this.tattoosLeftUpperArm;
        set
        {
            if (ped.Model == 0 && value != this.TattoosLeftUpperArm)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosLeftUpperArmCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosLeftUpperArm, this.TattoosLeftUpperArm, value));
                    this.tattoosLeftUpperArm = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosLeftUpperArm}.");
                }
            }
        }
    }

    private byte? tattoosLeftLowerArm;
    public byte? TattoosLeftLowerArm
    {
        get => this.tattoosLeftLowerArm;
        set
        {
            if (ped.Model == 0 && value != this.TattoosLeftUpperArm)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosLeftLowerArmCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosLeftLowerArm, this.TattoosLeftLowerArm, value));
                    this.tattoosLeftLowerArm = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosLeftLowerArm}.");
                }
            }
        }
    }

    private byte? tattoosRightUpperArm;
    public byte? TattoosRightUpperArm
    {
        get => this.tattoosRightUpperArm;
        set
        {
            if (ped.Model == 0 && value != this.TattoosRightUpperArm)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosRightUpperArmCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosRightUpperArm, this.TattoosRightUpperArm, value));
                    this.tattoosRightUpperArm = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosRightUpperArm}.");
                }
            }
        }
    }

    private byte? tattoosRightLowerArm;
    public byte? TattoosRightLowerArm
    {
        get => this.tattoosRightLowerArm;
        set
        {
            if (ped.Model == 0 && value != this.TattoosRightLowerArm)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosRightLowerArmCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosRightLowerArm, this.TattoosRightLowerArm, value));
                    this.tattoosRightLowerArm = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosRightLowerArm}.");
                }
            }
        }
    }

    private byte? tattoosBack;
    public byte? TattoosBack
    {
        get => this.tattoosBack;
        set
        {
            if (ped.Model == 0 && value != this.TattoosBack)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosBackCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosBack, this.TattoosBack, value));
                    this.tattoosBack = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosBack}.");
                }
            }
        }
    }

    private byte? tattoosLeftChest;
    public byte? TattoosLeftChest
    {
        get => this.tattoosLeftChest;
        set
        {
            if (ped.Model == 0 && value != this.TattoosLeftChest)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosLeftChestCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosLeftChest, this.TattoosLeftChest, value));
                    this.tattoosLeftChest = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosLeftChest}.");
                }
            }
        }
    }

    private byte? tattoosRightChest;
    public byte? TattoosRightChest
    {
        get => this.tattoosRightChest;
        set
        {
            if (ped.Model == 0 && value != this.TattoosRightChest)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosRightChestCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosRightChest, this.TattoosRightChest, value));
                    this.tattoosRightChest = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosRightChest}.");
                }
            }
        }
    }

    private byte? tattoosStomach;
    public byte? TattoosStomach
    {
        get => this.tattoosStomach;
        set
        {
            if (ped.Model == 0 && value != this.TattoosStomach)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosStomachCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosStomach, this.TattoosStomach, value));
                    this.tattoosStomach = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosStomach}.");
                }
            }
        }
    }

    private byte? tattoosLowerBack;
    public byte? TattoosLowerBack
    {
        get => this.tattoosLowerBack;
        set
        {
            if (ped.Model == 0 && value != this.TattoosLowerBack)
            {
                if (value >= 0 && value <= ClothingConstants.TattoosLowerBackCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.TattoosLowerBack, this.TattoosLowerBack, value));
                    this.tattoosLowerBack = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.TattoosLowerBack}.");
                }
            }
        }
    }

    private byte? necklace;
    public byte? Necklace
    {
        get => this.necklace;
        set
        {
            if (ped.Model == 0 && value != this.Necklace)
            {
                if (value >= 0 && value <= ClothingConstants.NecklaceCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Necklace, this.Necklace, value));
                    this.necklace = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Necklace}.");
                }
            }
        }
    }

    private byte? watch;
    public byte? Watch
    {
        get => this.watch;
        set
        {
            if (ped.Model == 0 && value != this.Watch)
            {
                if (value >= 0 && value <= ClothingConstants.WatchesCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Watches, this.Watch, value));
                    this.watch = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Watches}.");
                }
            }
        }
    }

    private byte? glasses;
    public byte? Glasses
    {
        get => this.glasses;
        set
        {
            if (ped.Model == 0 && value != this.Glasses)
            {
                if (value >= 0 && value <= ClothingConstants.GlassesCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Glasses, this.glasses, value));
                    this.glasses = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Glasses}.");
                }
            }
        }
    }

    private byte? hat;
    public byte? Hat
    {
        get => this.hat;
        set
        {
            if (ped.Model == 0 && value != this.Hat)
            {
                if (value >= 0 && value <= ClothingConstants.HatsCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Hats, this.hat, value));
                    this.hat = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Hats}.");
                }
            }
        }
    }

    private byte? extra;
    public byte? Extra
    {
        get => this.extra;
        set
        {
            if (ped.Model == 0 && value != this.Extra)
            {
                if (value >= 0 && value <= ClothingConstants.ExtraCount || value == null)
                {
                    Changed?.Invoke(ped, new ClothingChangedEventArgs(ped, Enums.ClothingType.Extra, this.extra, value));
                    this.extra = value;
                } else
                {
                    throw new ArgumentOutOfRangeException($"{value} is invalid clothing id for {Enums.ClothingType.Extra}.");
                }
            }
        }
    }

    public event ElementEventHandler<Ped, ClothingChangedEventArgs>? Changed;
}
