using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleStartExitEventArgs(Ped exitingPed, byte seat, Ped? jacker, byte door) : EventArgs
{
    public Ped ExitingPed { get; } = exitingPed;
    public byte Seat { get; } = seat;
    public Ped? Jacker { get; } = jacker;
    public byte Door { get; } = door;
}
