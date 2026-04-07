using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using static SlipeServer.Server.Services.IGameWorld;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions(IGameWorld gameWorld, IZoneService zoneService)
{
    private static readonly Dictionary<string, WorldSpecialProperty> specialPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hovercars"] = WorldSpecialProperty.Hovercars,
        ["aircars"] = WorldSpecialProperty.Aircars,
        ["extrabunny"] = WorldSpecialProperty.ExtraBunny,
        ["extrajump"] = WorldSpecialProperty.ExtraJump,
        ["randomfoliage"] = WorldSpecialProperty.RandomFoliage,
        ["snipermoon"] = WorldSpecialProperty.SniperMoon,
        ["extraairresistance"] = WorldSpecialProperty.ExtraAirResistance,
        ["underworldwarp"] = WorldSpecialProperty.UnderWorldWarp,
        ["vehiclesunglare"] = WorldSpecialProperty.VehicleSunGlare,
        ["coronaztest"] = WorldSpecialProperty.CoronaGlareDisabled,
        ["watercreatures"] = WorldSpecialProperty.WaterCreatures,
        ["burnflippedcars"] = WorldSpecialProperty.BurnFlippedCars,
        ["fireballdestruct"] = WorldSpecialProperty.FireBallAircraftDestruction,
        ["roadsignstext"] = WorldSpecialProperty.RoadSignText,
        ["extendedwatercannons"] = WorldSpecialProperty.ExtendedWaterCannons,
        ["tunnelweatherblend"] = WorldSpecialProperty.TunnelWeatherBlending,
        ["ignorefirestate"] = WorldSpecialProperty.IgnoreFireState,
        ["flyingcomponents"] = WorldSpecialProperty.FlyingComponents,
        ["vehicleburnexplosions"] = WorldSpecialProperty.VehicleBurnExplosions,
        ["vehicle_engine_autostart"] = WorldSpecialProperty.VehicleEngineAutoStart,
    };
}

