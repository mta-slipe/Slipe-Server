using SlipeServer.Packets.Definitions.Map.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Services;

public interface IGameWorld
{
    float AircraftMaxHeight { get; set; }
    float AircraftMaxVelocity { get; set; }
    bool AreInteriorSoundsEnabled { get; set; }
    bool CloudsEnabled { get; set; }
    float? FarClipDistance { get; set; }
    float? FogDistance { get; set; }
    byte FpsLimit { get; set; }
    float GameSpeed { get; set; }
    float Gravity { get; set; }
    HeatHaze? HeatHaze { get; set; }
    float MaxJetpackHeight { get; set; }
    uint MinuteDuration { get; set; }
    int MoonSize { get; set; }
    bool OcclusionsEnabled { get; set; }
    byte PreviousWeather { get; }
    float RainLevel { get; set; }
    IReadOnlyDictionary<WorldSpecialProperty, bool> SpecialPropertyStates { get; }
    int SunSize { get; set; }
    (byte hour, byte minute) Time { get; }
    Color? WaterColor { get; set; }
    WaterLevels WaterLevels { get; set; }
    float WaveHeight { get; set; }
    byte Weather { get; set; }
    byte WeatherBlendStopHour { get; }
    Vector3 WindVelocity { get; set; }
    IReadOnlyCollection<WorldObjectRemoval> WorldObjectRemovals { get; }

    void CreateProjectile(Vector3 from, Vector3 direction, Element sourceElement, DamageType weaponType = DamageType.WEAPONTYPE_ROCKET, ushort model = 345);
    (Color, Color)? GetSkyGradient();
    (Color, Color)? GetSunColor();
    Tuple<byte, byte> GetTime();
    bool IsGarageOpen(GarageLocation garage);
    bool IsGlitchEnabled(GlitchType glitchType);
    bool IsJetpackWeaponEnabled(WeaponId weapon);
    void RemoveWorldModel(ushort model, Vector3 position, float range, byte interior = 0);
    void RestoreAllWorldModels();
    void RestoreWorldModel(ushort model, Vector3 position, float range, byte interior = 0);
    void SetGarageOpen(GarageLocation garage, bool isOpen);
    void SetGlitchEnabled(GlitchType glitchType, bool enabled);
    void SetJetpackWeaponEnabled(WeaponId weapon, bool isEnabled);
    void SetSkyGradient(Color top, Color bottom);
    void SetSpecialPropertyEnabled(WorldSpecialProperty property, bool enabled);
    void SetSunColor(Color core, Color corona);
    void SetTime(byte hour, byte minute);
    void SetTrafficLightState(TrafficLightState state, bool forced = false);
    void SetWeather(byte weather);
    void SetWeather(Weather weather);
    void SetWeatherBlended(byte weather, byte hours = 1);
    void SetWeatherBlended(Weather weather, byte hours = 1);


    public enum WorldSpecialProperty
    {
        Hovercars,
        Aircars,
        ExtraBunny,
        ExtraJump,
        RandomFoliage,
        SniperMoon,
        ExtraAirResistance,
        UnderWorldWarp,
        VehicleSunGlare,
        CoronaGlareDisabled,
        WaterCreatures,
        BurnFlippedCars,
        FireBallAircraftDestruction,
        RoadSignText,
        ExtendedWaterCannons,
        TunnelWeatherBlending,
        IgnoreFireState,
        FlyingComponents,
        VehicleBurnExplosions,
        VehicleEngineAutoStart
    }
}
