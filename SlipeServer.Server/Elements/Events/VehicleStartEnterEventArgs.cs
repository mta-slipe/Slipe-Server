using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleStartEnterEventArgs(Ped enteringPed, byte seat, Ped? jacked, byte door) : EventArgs
{
    public Ped EnteringPed { get; } = enteringPed;
    public byte Seat { get; } = seat;
    public Ped? Jacked { get; } = jacked;
    public byte Door { get; } = door;
}
