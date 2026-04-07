using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("addVehicleSirens")]
    public bool AddVehicleSirens(
        Vehicle vehicle, int sirenCount, int sirenType,
        bool flag360 = false, bool checkLosFlag = true,
        bool useRandomiser = true, bool silentFlag = false)
    {
        var sirenSet = new VehicleSirenSet { SirenType = (VehicleSirenType)sirenType };
        for (byte i = 0; i < sirenCount && i < 8; i++)
        {
            sirenSet.AddSiren(new VehicleSiren
            {
                Id = i,
                Position = Vector3.Zero,
                Color = Color.White,
                SirenMinAlpha = 0,
                Is360 = flag360,
                UsesLineOfSightCheck = checkLosFlag,
                UsesRandomizer = useRandomiser,
                IsSilent = silentFlag,
            });
        }
        vehicle.Sirens = sirenSet;
        return true;
    }

    [ScriptFunctionDefinition("removeVehicleSirens")]
    public bool RemoveVehicleSirens(Vehicle vehicle)
    {
        vehicle.Sirens = null;
        return true;
    }

    [ScriptFunctionDefinition("setVehicleSirens")]
    public bool SetVehicleSirens(
        Vehicle vehicle, int sirenId,
        float x, float y, float z,
        int red, int green, int blue, int alpha,
        uint minAlpha = 0)
    {
        if (vehicle.Sirens == null)
            return false;

        var sirenSet = vehicle.Sirens.Value;
        var id = (byte)(sirenId - 1);
        sirenSet.AddSiren(new VehicleSiren
        {
            Id = id,
            Position = new Vector3(x, y, z),
            Color = Color.FromArgb(alpha, red, green, blue),
            SirenMinAlpha = minAlpha,
            Is360 = sirenSet.Sirens.FirstOrDefault(s => s.Id == id).Is360,
            UsesLineOfSightCheck = sirenSet.Sirens.FirstOrDefault(s => s.Id == id).UsesLineOfSightCheck,
            UsesRandomizer = sirenSet.Sirens.FirstOrDefault(s => s.Id == id).UsesRandomizer,
            IsSilent = sirenSet.Sirens.FirstOrDefault(s => s.Id == id).IsSilent,
        });
        vehicle.Sirens = sirenSet;
        return true;
    }

    [ScriptFunctionDefinition("getVehicleSirens")]
    public LuaValue GetVehicleSirens(Vehicle vehicle)
    {
        if (vehicle.Sirens == null)
            return LuaValue.Nil;

        var table = new Dictionary<LuaValue, LuaValue>();
        int idx = 1;
        foreach (var siren in vehicle.Sirens.Value.Sirens)
        {
            var sirenTable = new Dictionary<LuaValue, LuaValue>
            {
                [new LuaValue("x")] = new LuaValue(siren.Position.X),
                [new LuaValue("y")] = new LuaValue(siren.Position.Y),
                [new LuaValue("z")] = new LuaValue(siren.Position.Z),
                [new LuaValue("red")] = new LuaValue(siren.Color.R),
                [new LuaValue("green")] = new LuaValue(siren.Color.G),
                [new LuaValue("blue")] = new LuaValue(siren.Color.B),
                [new LuaValue("alpha")] = new LuaValue(siren.Color.A),
                [new LuaValue("minAlpha")] = new LuaValue((int)siren.SirenMinAlpha),
            };
            table[new LuaValue(idx++)] = new LuaValue(sirenTable);
        }
        return new LuaValue(table);
    }

    [ScriptFunctionDefinition("getVehicleSirensOn")]
    public bool GetVehicleSirensOn(Vehicle vehicle) => vehicle.IsSirenActive;

    [ScriptFunctionDefinition("setVehicleSirensOn")]
    public bool SetVehicleSirensOn(Vehicle vehicle, bool on)
    {
        vehicle.IsSirenActive = on;
        return true;
    }

    [ScriptFunctionDefinition("getVehicleSirenParams")]
    public LuaValue GetVehicleSirenParams(Vehicle vehicle)
    {
        if (vehicle.Sirens == null)
            return LuaValue.Nil;

        var sirenSet = vehicle.Sirens.Value;
        var firstSiren = sirenSet.Sirens.FirstOrDefault();

        var flagsTable = new Dictionary<LuaValue, LuaValue>
        {
            [new LuaValue("Is360")] = new LuaValue(firstSiren.Is360),
            [new LuaValue("DoLOSCheck")] = new LuaValue(firstSiren.UsesLineOfSightCheck),
            [new LuaValue("UseRandomiser")] = new LuaValue(firstSiren.UsesRandomizer),
            [new LuaValue("Silent")] = new LuaValue(firstSiren.IsSilent),
        };

        var result = new Dictionary<LuaValue, LuaValue>
        {
            [new LuaValue("SirenCount")] = new LuaValue((int)sirenSet.Count),
            [new LuaValue("SirenType")] = new LuaValue((int)sirenSet.SirenType),
            [new LuaValue("Flags")] = new LuaValue(flagsTable),
        };

        return new LuaValue(result);
    }
}
