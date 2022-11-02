using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

public struct VehicleSirenSet
{
    public VehicleSirenType SirenType { get; set; }

    // We use 8 different fields instead of a collection to make sure
    // When a copy of this struct is made it also copies the sirens
    // And does not keep a reference to the original collection
    private VehicleSiren? siren1 = null;
    private VehicleSiren? siren2 = null;
    private VehicleSiren? siren3 = null;
    private VehicleSiren? siren4 = null;
    private VehicleSiren? siren5 = null;
    private VehicleSiren? siren6 = null;
    private VehicleSiren? siren7 = null;
    private VehicleSiren? siren8 = null;

    public IEnumerable<VehicleSiren> Sirens
    {
        get
        {
            if (this.siren1.HasValue)
                yield return this.siren1.Value;
            if (this.siren2.HasValue)
                yield return this.siren2.Value;
            if (this.siren3.HasValue)
                yield return this.siren3.Value;
            if (this.siren4.HasValue)
                yield return this.siren4.Value;
            if (this.siren5.HasValue)
                yield return this.siren5.Value;
            if (this.siren6.HasValue)
                yield return this.siren6.Value;
            if (this.siren7.HasValue)
                yield return this.siren7.Value;
            if (this.siren8.HasValue)
                yield return this.siren8.Value;
        }
    }

    public byte Count => (byte)this.Sirens.Count();

    public VehicleSirenSet()
    {
        this.SirenType = VehicleSirenType.Dual;
    }

    public void AddSiren(VehicleSiren siren)
    {
        var before = this.Count;

        switch (siren.Id)
        {
            case 0:
                this.siren1 = siren;
                break;
            case 1:
                this.siren2 = siren;
                break;
            case 2:
                this.siren3 = siren;
                break;
            case 3:
                this.siren4 = siren;
                break;
            case 4:
                this.siren5 = siren;
                break;
            case 5:
                this.siren6 = siren;
                break;
            case 6:
                this.siren7 = siren;
                break;
            case 7:
                this.siren8 = siren;
                break;
        }

        if (this.Count > before)
            this.SirenAdded?.Invoke(this, siren);
        else
            this.SirenModified?.Invoke(this, siren);
    }

    public void RemoveSiren(byte id)
    {
        switch (id)
        {
            case 0:
                if (this.siren1.HasValue)
                    this.SirenRemoved?.Invoke(this, this.siren1.Value);
                this.siren1 = null;
                break;
            case 1:
                if (this.siren2.HasValue)
                    this.SirenRemoved?.Invoke(this, this.siren2.Value);
                this.siren2 = null;
                break;
            case 2:
                if (this.siren3.HasValue)
                    this.SirenRemoved?.Invoke(this, this.siren3.Value);
                this.siren3 = null;
                break;
            case 3:
                if (this.siren4.HasValue)
                    this.SirenRemoved?.Invoke(this, this.siren4.Value);
                this.siren4 = null;
                break;
            case 4:
                if (this.siren5.HasValue)
                    this.SirenRemoved?.Invoke(this, this.siren5.Value);
                this.siren5 = null;
                break;
            case 5:
                if (this.siren6.HasValue)
                    this.SirenRemoved?.Invoke(this, this.siren6.Value);
                this.siren6 = null;
                break;
            case 6:
                if (this.siren7.HasValue)
                    this.SirenRemoved?.Invoke(this, this.siren7.Value);
                this.siren7 = null;
                break;
            case 7:
                if (this.siren8.HasValue)
                    this.SirenRemoved?.Invoke(this, this.siren8.Value);
                this.siren8 = null;
                break;
        }
    }

    public void RemoveSiren(VehicleSiren siren)
    {
        RemoveSiren(siren.Id);
    }

    public void ModifySiren(VehicleSiren siren)
    {
        AddSiren(siren);
    }

    public event Action<VehicleSirenSet, VehicleSiren>? SirenModified = null;
    public event Action<VehicleSirenSet, VehicleSiren>? SirenAdded = null;
    public event Action<VehicleSirenSet, VehicleSiren>? SirenRemoved = null;
}
