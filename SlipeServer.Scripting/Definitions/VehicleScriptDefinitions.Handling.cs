using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("getVehicleHandling")]
    public LuaValue GetVehicleHandling(Vehicle vehicle)
        => HandlingToLuaValue(vehicle.AppliedHandling);

    [ScriptFunctionDefinition("setVehicleHandling")]
    public bool SetVehicleHandling(Vehicle vehicle, string property, LuaValue? value = null)
    {
        if (value == null || value.IsNil)
        {
            vehicle.Handling = null;
            return true;
        }

        var handling = vehicle.AppliedHandling;
        handling = ApplyHandlingProperty(handling, property, value);
        vehicle.Handling = handling;
        return true;
    }

    [ScriptFunctionDefinition("getModelHandling")]
    public LuaValue GetModelHandling(int model)
    {
        if (!VehicleHandlingConstants.DefaultVehicleHandling.TryGetValue((ushort)model, out var handling))
            return LuaValue.Nil;
        return HandlingToLuaValue(handling);
    }

    [ScriptFunctionDefinition("setModelHandling")]
    public bool SetModelHandling(int model, string property, LuaValue? value = null)
    {
        if (!VehicleHandlingConstants.DefaultVehicleHandling.TryGetValue((ushort)model, out var handling))
            return false;

        if (value == null || value.IsNil)
        {
            if (this.originalHandling.TryGetValue((ushort)model, out var orig))
                VehicleHandlingConstants.DefaultVehicleHandling[(ushort)model] = orig;
            return true;
        }

        var updated = ApplyHandlingProperty(handling, property, value);
        VehicleHandlingConstants.DefaultVehicleHandling[(ushort)model] = updated;
        return true;
    }

    [ScriptFunctionDefinition("getOriginalHandling")]
    public LuaValue GetOriginalHandling(int model)
    {
        if (!this.originalHandling.TryGetValue((ushort)model, out var handling))
            return LuaValue.Nil;
        return HandlingToLuaValue(handling);
    }

    private static LuaValue HandlingToLuaValue(VehicleHandling h)
    {
        var com = h.CenterOfMass;
        var comTable = new Dictionary<LuaValue, LuaValue>
        {
            [new LuaValue(1)] = new LuaValue(com.X),
            [new LuaValue(2)] = new LuaValue(com.Y),
            [new LuaValue(3)] = new LuaValue(com.Z),
        };

        return new LuaValue(new Dictionary<LuaValue, LuaValue>
        {
            [new LuaValue("mass")] = new LuaValue(h.Mass),
            [new LuaValue("turnMass")] = new LuaValue(h.TurnMass),
            [new LuaValue("dragCoefficient")] = new LuaValue(h.DragCoefficient),
            [new LuaValue("centerOfMass")] = new LuaValue(comTable),
            [new LuaValue("percentSubmerged")] = new LuaValue((int)h.PercentSubmerged),
            [new LuaValue("tractionMultiplier")] = new LuaValue(h.TractionMultiplier),
            [new LuaValue("driveType")] = new LuaValue(h.DriveType switch
            {
                VehicleDriveType.FourWheelDrive => "4wd",
                VehicleDriveType.FrontWheelDrive => "fwd",
                _ => "rwd"
            }),
            [new LuaValue("engineType")] = new LuaValue(h.EngineType switch
            {
                VehicleEngineType.Diesel => "diesel",
                VehicleEngineType.Electric => "electric",
                _ => "petrol"
            }),
            [new LuaValue("numberOfGears")] = new LuaValue((int)h.NumberOfGears),
            [new LuaValue("engineAcceleration")] = new LuaValue(h.EngineAcceleration),
            [new LuaValue("engineInertia")] = new LuaValue(h.EngineInertia),
            [new LuaValue("maxVelocity")] = new LuaValue(h.MaxVelocity),
            [new LuaValue("brakeDeceleration")] = new LuaValue(h.BrakeDeceleration),
            [new LuaValue("brakeBias")] = new LuaValue(h.BrakeBias),
            [new LuaValue("abs")] = new LuaValue(h.Abs),
            [new LuaValue("steeringLock")] = new LuaValue(h.SteeringLock),
            [new LuaValue("tractionLoss")] = new LuaValue(h.TractionLoss),
            [new LuaValue("tractionBias")] = new LuaValue(h.TractionBias),
            [new LuaValue("suspensionForceLevel")] = new LuaValue(h.SuspensionForceLevel),
            [new LuaValue("suspensionDamping")] = new LuaValue(h.SuspensionDampening),
            [new LuaValue("suspensionHighSpeedDamping")] = new LuaValue(h.SuspensionHighSpeedDampening),
            [new LuaValue("suspensionUpperLimit")] = new LuaValue(h.SuspensionUpperLimit),
            [new LuaValue("suspensionLowerLimit")] = new LuaValue(h.SuspensionLowerLimit),
            [new LuaValue("suspensionFrontRearBias")] = new LuaValue(h.SuspensionFrontRearBias),
            [new LuaValue("suspensionAntiDiveMultiplier")] = new LuaValue(h.SuspensionAntiDiveMultiplier),
            [new LuaValue("collisionDamageMultiplier")] = new LuaValue(h.CollisionDamageMultiplier),
            [new LuaValue("seatOffsetDistance")] = new LuaValue(h.SeatOffsetDistance),
            [new LuaValue("monetary")] = new LuaValue(0),
            [new LuaValue("handlingFlags")] = new LuaValue((int)h.HandlingFlags),
            [new LuaValue("modelFlags")] = new LuaValue((int)h.ModelFlags),
            [new LuaValue("headLight")] = new LuaValue(0),
            [new LuaValue("tailLight")] = new LuaValue(0),
            [new LuaValue("animGroup")] = new LuaValue((int)h.AnimGroup),
        });
    }

    private static VehicleHandling ApplyHandlingProperty(VehicleHandling h, string property, LuaValue value)
    {
        static float F(LuaValue v) =>
            v.FloatValue ?? (v.DoubleValue.HasValue ? (float)v.DoubleValue.Value : (float)(v.IntegerValue ?? 0));

        switch (property)
        {
            case "mass": h.Mass = F(value); break;
            case "turnMass": h.TurnMass = F(value); break;
            case "dragCoefficient": h.DragCoefficient = F(value); break;
            case "centerOfMass":
                if (value.TableValue != null)
                {
                    h.CenterOfMass = new Vector3(
                        F(value.TableValue[new LuaValue(1)]),
                        F(value.TableValue[new LuaValue(2)]),
                        F(value.TableValue[new LuaValue(3)]));
                }
                break;
            case "percentSubmerged": h.PercentSubmerged = (byte)F(value); break;
            case "tractionMultiplier": h.TractionMultiplier = F(value); break;
            case "driveType":
                h.DriveType = (value.StringValue ?? "rwd").ToLowerInvariant() switch
                {
                    "4wd" => VehicleDriveType.FourWheelDrive,
                    "fwd" => VehicleDriveType.FrontWheelDrive,
                    _ => VehicleDriveType.RearWheelDrive
                };
                break;
            case "engineType":
                h.EngineType = (value.StringValue ?? "petrol").ToLowerInvariant() switch
                {
                    "diesel" => VehicleEngineType.Diesel,
                    "electric" => VehicleEngineType.Electric,
                    _ => VehicleEngineType.Petrol
                };
                break;
            case "numberOfGears": h.NumberOfGears = (byte)F(value); break;
            case "engineAcceleration": h.EngineAcceleration = F(value); break;
            case "engineInertia": h.EngineInertia = F(value); break;
            case "maxVelocity": h.MaxVelocity = F(value); break;
            case "brakeDeceleration": h.BrakeDeceleration = F(value); break;
            case "brakeBias": h.BrakeBias = F(value); break;
            case "abs": h.Abs = value.BoolValue ?? (F(value) > 0); break;
            case "steeringLock": h.SteeringLock = F(value); break;
            case "tractionLoss": h.TractionLoss = F(value); break;
            case "tractionBias": h.TractionBias = F(value); break;
            case "suspensionForceLevel": h.SuspensionForceLevel = F(value); break;
            case "suspensionDamping": h.SuspensionDampening = F(value); break;
            case "suspensionHighSpeedDamping": h.SuspensionHighSpeedDampening = F(value); break;
            case "suspensionUpperLimit": h.SuspensionUpperLimit = F(value); break;
            case "suspensionLowerLimit": h.SuspensionLowerLimit = F(value); break;
            case "suspensionFrontRearBias": h.SuspensionFrontRearBias = F(value); break;
            case "suspensionAntiDiveMultiplier": h.SuspensionAntiDiveMultiplier = F(value); break;
            case "collisionDamageMultiplier": h.CollisionDamageMultiplier = F(value); break;
            case "seatOffsetDistance": h.SeatOffsetDistance = F(value); break;
            case "handlingFlags": h.HandlingFlags = (uint)F(value); break;
            case "modelFlags": h.ModelFlags = (uint)F(value); break;
            case "animGroup": h.AnimGroup = (byte)F(value); break;
        }
        return h;
    }
}
