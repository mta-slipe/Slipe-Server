using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using System.Drawing;

namespace SlipeServer.Scripting.Definitions;

public partial class VehicleScriptDefinitions
{
    [ScriptFunctionDefinition("getVehicleColor")]
    public (int, int, int, int, int, int, int, int, int, int, int, int) GetVehicleColor(Vehicle vehicle, bool bRGB = true)
    {
        var c = vehicle.Colors;
        return (c.Primary.R, c.Primary.G, c.Primary.B,
                c.Secondary.R, c.Secondary.G, c.Secondary.B,
                c.Color3.R, c.Color3.G, c.Color3.B,
                c.Color4.R, c.Color4.G, c.Color4.B);
    }

    [ScriptFunctionDefinition("setVehicleColor")]
    public bool SetVehicleColor(
        Vehicle vehicle,
        int r1, int g1, int b1,
        int r2 = -1, int g2 = -1, int b2 = -1,
        int r3 = -1, int g3 = -1, int b3 = -1,
        int r4 = -1, int g4 = -1, int b4 = -1)
    {
        vehicle.Colors.Primary = Color.FromArgb(r1, g1, b1);
        if (r2 >= 0) vehicle.Colors.Secondary = Color.FromArgb(r2, g2, b2);
        if (r3 >= 0) vehicle.Colors.Color3 = Color.FromArgb(r3, g3, b3);
        if (r4 >= 0) vehicle.Colors.Color4 = Color.FromArgb(r4, g4, b4);
        return true;
    }

    [ScriptFunctionDefinition("getVehiclePlateText")]
    public string GetVehiclePlateText(Vehicle vehicle) => vehicle.PlateText;

    [ScriptFunctionDefinition("setVehiclePlateText")]
    public bool SetVehiclePlateText(Vehicle vehicle, string text)
    {
        vehicle.PlateText = text;
        return true;
    }

    [ScriptFunctionDefinition("getVehiclePaintjob")]
    public int GetVehiclePaintjob(Vehicle vehicle) => vehicle.PaintJob;

    [ScriptFunctionDefinition("setVehiclePaintjob")]
    public bool SetVehiclePaintjob(Vehicle vehicle, int paintjob)
    {
        vehicle.PaintJob = (byte)paintjob;
        return true;
    }

    [ScriptFunctionDefinition("getVehicleVariant")]
    public (int, int) GetVehicleVariant(Vehicle vehicle)
        => (vehicle.Variants.Variant1, vehicle.Variants.Variant2);

    [ScriptFunctionDefinition("setVehicleVariant")]
    public bool SetVehicleVariant(Vehicle vehicle, int? variant1 = null, int? variant2 = null)
    {
        vehicle.Variants = new VehicleVariants
        {
            Variant1 = (byte)(variant1 ?? vehicle.Variants.Variant1),
            Variant2 = (byte)(variant2 ?? vehicle.Variants.Variant2),
        };
        return true;
    }

    [ScriptFunctionDefinition("getVehicleOverrideLights")]
    public int GetVehicleOverrideLights(Vehicle vehicle) => (int)vehicle.OverrideLights;

    [ScriptFunctionDefinition("setVehicleOverrideLights")]
    public bool SetVehicleOverrideLights(Vehicle vehicle, int lights)
    {
        vehicle.OverrideLights = (VehicleOverrideLights)lights;
        return true;
    }

    [ScriptFunctionDefinition("isVehicleTaxiLightOn")]
    public bool IsVehicleTaxiLightOn(Vehicle vehicle) => vehicle.IsTaxiLightOn;

    [ScriptFunctionDefinition("setVehicleTaxiLightOn")]
    public bool SetVehicleTaxiLightOn(Vehicle vehicle, bool on)
    {
        vehicle.IsTaxiLightOn = on;
        return true;
    }

    [ScriptFunctionDefinition("getVehicleLandingGearDown")]
    public bool GetVehicleLandingGearDown(Vehicle vehicle) => vehicle.IsLandingGearDown;

    [ScriptFunctionDefinition("setVehicleLandingGearDown")]
    public bool SetVehicleLandingGearDown(Vehicle vehicle, bool down)
    {
        vehicle.IsLandingGearDown = down;
        return true;
    }

    [ScriptFunctionDefinition("getVehicleHeadLightColor")]
    public Color GetVehicleHeadLightColor(Vehicle vehicle) => vehicle.HeadlightColor;

    [ScriptFunctionDefinition("setVehicleHeadLightColor")]
    public bool SetVehicleHeadLightColor(Vehicle vehicle, int r, int g, int b, int a = 255)
    {
        vehicle.HeadlightColor = Color.FromArgb(a, r, g, b);
        return true;
    }
}
