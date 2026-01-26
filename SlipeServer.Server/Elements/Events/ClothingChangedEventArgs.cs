using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class ClothingChangedEventArgs(Ped ped, ClothingType clothes, byte? previous, byte? current) : EventArgs
{
    public Ped Ped { get; } = ped;
    public ClothingType ClothingType { get; } = clothes;
    public byte? Previous { get; } = previous;
    public byte? Current { get; } = current;
}
