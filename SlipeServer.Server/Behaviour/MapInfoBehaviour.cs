using SlipeServer.Packets.Definitions.Map;
using SlipeServer.Packets.Definitions.Map.Structs;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Services;
using SlipeServer.Server.Structs;
using System;
using System.Linq;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsbile for sending map info packets on player join
/// </summary>
public class MapInfoBehaviour
{
    private readonly IGameWorld gameWorld;
    private readonly IWeaponConfigurationService weaponConfigurationService;

    public MapInfoBehaviour(MtaServer server, IGameWorld gameWorld, IWeaponConfigurationService weaponConfigurationService)
    {
        this.gameWorld = gameWorld;
        this.weaponConfigurationService = weaponConfigurationService;

        server.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        new MapInfoPacket()
        {
            Weather = this.gameWorld.PreviousWeather,
            WeatherBlendingTo = this.gameWorld.Weather,
            BlendedWeatherHour = this.gameWorld.WeatherBlendStopHour,
            SkyGradient = this.gameWorld.GetSkyGradient(),
            HeatHaze = MapHeatHaze(this.gameWorld.HeatHaze),
            Time = this.gameWorld.Time,
            MinuteDuration = this.gameWorld.MinuteDuration,
            Flags = (true, true, this.gameWorld.CloudsEnabled),
            Gravity = this.gameWorld.Gravity,
            GameSpeed = this.gameWorld.GameSpeed,
            WaveHeight = this.gameWorld.WaveHeight,
            SeaLevel = this.gameWorld.WaterLevels.SeaLevel,
            NonSeaLevel = this.gameWorld.WaterLevels.NonSeaLevel,
            OutsideWorldSeaLevel = this.gameWorld.WaterLevels.OutsideSeaLevel,
            FpsLimit = this.gameWorld.FpsLimit,
            GarageStates = Enum.GetValues<GarageLocation>().Select(x => this.gameWorld.IsGarageOpen(x)).ToArray(),
            BugsEnabled = (
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_CLOSEDAMAGE),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_QUICKRELOAD),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_FASTFIRE),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_FASTMOVE),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_CROUCHBUG),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_HITANIM),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_FASTSPRINT),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_BADDRIVEBYHITBOX),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_QUICKSTAND),
                this.gameWorld.IsGlitchEnabled(GlitchType.GLITCH_VEHICLE_RAPID_STOP)
            ),
            MaximumJetpackHeight = this.gameWorld.MaxJetpackHeight,
            WaterColor = this.gameWorld.WaterColor,
            AreInteriorSoundsEnabled = this.gameWorld.AreInteriorSoundsEnabled,
            RainLevel = this.gameWorld.RainLevel,
            MoonSize = this.gameWorld.MoonSize,
            SunSize = this.gameWorld.SunSize,
            SunColor = this.gameWorld.GetSunColor(),
            WindVelocity = this.gameWorld.WindVelocity,
            FarClipDistance = this.gameWorld.FarClipDistance,
            FogDistance = this.gameWorld.FogDistance,
            AircraftMaxHeight = this.gameWorld.AircraftMaxHeight,
            AircraftMaxVelocity = this.gameWorld.AircraftMaxVelocity,
            WeaponProperties = Enum.GetValues<WeaponId>().Select(x => new MapInfoWeaponProperty()
            {
                WeaponType = (byte)x,
                EnabledWhenUsingJetpack = false,
                WeaponConfigurations = Enum.GetValues<WeaponSkillLevel>().Select(skill =>
                {
                    return MapWeaponConfiguration(this.weaponConfigurationService.GetWeaponConfiguration(x, skill));
                }).ToArray()
            }).ToArray(),
            RemovedWorldModels = this.gameWorld.WorldObjectRemovals,
            OcclusionsEnabled = this.gameWorld.OcclusionsEnabled,
            SpecialProperties = GetSpecialPropertyEnableds(),
        }.SendTo(player);
    }

    private bool[] GetSpecialPropertyEnableds()
    {
        return [
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.UnderWorldWarp],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.ExtraAirResistance],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.SniperMoon],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.RandomFoliage],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.ExtraJump],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.ExtraBunny],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.Aircars],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.Hovercars],

            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.BurnFlippedCars],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.WaterCreatures],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.CoronaGlareDisabled],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.VehicleSunGlare],

            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.FireBallAircraftDestruction],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.RoadSignText],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.ExtendedWaterCannons],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.TunnelWeatherBlending],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.IgnoreFireState],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.FlyingComponents],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.VehicleBurnExplosions],
            this.gameWorld.SpecialPropertyStates[IGameWorld.WorldSpecialProperty.VehicleEngineAutoStart],
        ];
    }

    private (byte intensity, byte randomShift, ushort speedMin, ushort speedMax, short scanSizeX, short scanSizeY, ushort renderSizeX, ushort renderSizeY, bool isInsideBuilder)? MapHeatHaze(HeatHaze? heatHaze)
    {
        if (heatHaze == null)
            return null;

        return (heatHaze.Value.Intensity, heatHaze.Value.RandomShift, heatHaze.Value.MinSpeed, heatHaze.Value.MaxSpeed,
            (short)heatHaze.Value.ScanSize.X, (short)heatHaze.Value.ScanSize.Y,
            (ushort)heatHaze.Value.RenderSize.X, (ushort)heatHaze.Value.RenderSize.Y,
            heatHaze.Value.IsEnabledInsideBuildings);
    }

    private MapInfoWeaponConfiguration MapWeaponConfiguration(WeaponConfiguration weaponConfiguration)
    {
        return new MapInfoWeaponConfiguration()
        {
            WeaponType = (int)weaponConfiguration.WeaponType,
            TargetRange = weaponConfiguration.TargetRange,
            WeaponRange = weaponConfiguration.WeaponRange,
            Flags = weaponConfiguration.Flags,
            MaximumClipAmmo = weaponConfiguration.MaximumClipAmmo,
            Damage = weaponConfiguration.Damage,
            Accuracy = weaponConfiguration.Accuracy,
            MoveSpeed = weaponConfiguration.MoveSpeed,
            AnimationLoopStart = weaponConfiguration.AnimationLoopStart,
            AnimationLoopStop = weaponConfiguration.AnimationLoopStop,
            AnimationLoopBulletFire = weaponConfiguration.AnimationLoopBulletFire,
            Animation2LoopStart = weaponConfiguration.Animation2LoopStart,
            Animation2LoopStop = weaponConfiguration.Animation2LoopStop,
            Animation2LoopBulletFire = weaponConfiguration.Animation2LoopBulletFire,
            AnimationBreakoutTime = weaponConfiguration.AnimationBreakoutTime
        };
    }
}
