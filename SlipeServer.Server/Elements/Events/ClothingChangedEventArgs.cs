using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class ClothingChangedEventArgs : EventArgs
{
    public Ped Ped { get; }
    public ClothingType ClothingType { get; }
    public byte? Previous { get; }
    public byte? Current { get; }

    public ClothingChangedEventArgs(Ped ped, ClothingType clothes, byte? previous, byte? current)
    {
        this.Ped = ped;
        this.ClothingType = clothes;
        this.Previous = previous;
        this.Current = current;
    }
}
