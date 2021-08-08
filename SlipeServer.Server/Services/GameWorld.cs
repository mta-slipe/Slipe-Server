using SlipeServer.Packets.Definitions.Lua.Rpc.World;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Timers;

namespace SlipeServer.Server.Services
{
    public class GameWorld
    {
        private readonly MtaServer server;
        private readonly Dictionary<GarageLocation, bool> garageStates;
        private readonly Dictionary<WeaponId, bool> jetpackEnabledWeapons;
        private readonly Timer timeTimer;

        private Color? skyGradientTopColor;
        private Color? skyGradientBottomColor;

        private Color? sunCoreColor;
        private Color? sunCoronaColor;

        private TrafficLightState trafficLightState;
        private bool trafficLightStateForced;

        private byte hour = 0;
        private byte minute = 0;

        #region Properties

        public byte Weather { get; set; }

        private float? fogDistance;
        public float? FogDistance
        {
            get => fogDistance;
            set
            {
                fogDistance = value;
                if (value != null)
                    this.server.BroadcastPacket(new SetFogDistancePacket(value.Value));
                else
                    this.server.BroadcastPacket(new ResetFogDistancePacket());
            }
        }

        private float? farClipDistance;
        public float? FarClipDistance
        {

            get => farClipDistance;
            set
            {
                farClipDistance = value;
                if (value != null)
                    this.server.BroadcastPacket(new SetFarClipDistancePacket(value.Value));
                else
                    this.server.BroadcastPacket(new ResetFarClipDistancePacket());
            }
        }

        private float aircraftMaxHeight = 800;
        public float AircraftMaxHeight
        {

            get => aircraftMaxHeight;
            set
            {
                aircraftMaxHeight = value;
                this.server.BroadcastPacket(new SetAircraftMaxHeightPacket(value));
            }
        }

        private float aircraftMaxVelocity = 1.5f;
        public float AircraftMaxVelocity
        {

            get => aircraftMaxVelocity;
            set
            {
                aircraftMaxVelocity = value;
                this.server.BroadcastPacket(new SetAircraftMaxVelocityPacket(value));
            }
        }

        private bool cloudsEnabled = true;
        public bool CloudsEnabled
        {

            get => cloudsEnabled;
            set
            {
                cloudsEnabled = value;
                this.server.BroadcastPacket(new SetCloudsEnabledPacket(value));
            }
        }

        private float gameSpeed = 1;
        public float GameSpeed
        {

            get => gameSpeed;
            set
            {
                gameSpeed = value;
                this.server.BroadcastPacket(new SetGameSpeedPacket(value));
            }
        }

        private float gravity = 0.008f;
        public float Gravity
        {

            get => gravity;
            set
            {
                gravity = value;
                this.server.BroadcastPacket(new SetGravityPacket(value));
            }
        }

        private bool interiorSoundsEnabled = true;
        public bool InteriorSoundsEnabled
        {

            get => interiorSoundsEnabled;
            set
            {
                interiorSoundsEnabled = value;
                this.server.BroadcastPacket(new SetInteriorSoundsEnabledPacket(value));
            }
        }

        private uint minuteDuration = 1000;
        public uint MinuteDuration
        {

            get => minuteDuration;
            set
            {
                minuteDuration = value;
                this.timeTimer.Interval = value;
                this.server.BroadcastPacket(new SetMinuteDurationPacket(value));
            }
        }

        private int moonSize = 3;
        public int MoonSize
        {

            get => moonSize;
            set
            {
                moonSize = value;
                this.server.BroadcastPacket(new SetMoonSizePacket(value));
            }
        }

        private bool occlusionsEnabled = true;
        public bool OcclusionsEnabled
        {

            get => occlusionsEnabled;
            set
            {
                occlusionsEnabled = value;
                this.server.BroadcastPacket(new SetOcclusionsEnabledPacket(value));
            }
        }

        private float rainLevel = 0;
        public float RainLevel
        {

            get => rainLevel;
            set
            {
                rainLevel = value;
                this.server.BroadcastPacket(new SetRainLevelPacket(value));
            }
        }

        private int sunSize = 1;
        public int SunSize
        {

            get => sunSize;
            set
            {
                sunSize = value;
                this.server.BroadcastPacket(new SetSunSizePacket(value));
            }
        }

        private Vector3 windVelocity = Vector3.Zero;
        public Vector3 WindVelocity
        {

            get => windVelocity;
            set
            {
                windVelocity = value;
                this.server.BroadcastPacket(new SetWindVelocityPacket(value));
            }
        }

        #endregion

        public GameWorld(MtaServer server)
        {
            this.server = server;
            this.garageStates = new Dictionary<GarageLocation, bool>();
            this.jetpackEnabledWeapons = new Dictionary<WeaponId, bool>();

            this.timeTimer = new Timer(this.minuteDuration)
            {
                AutoReset = true,
            };
            this.timeTimer.Start();
            this.timeTimer.Elapsed += IncrementTime;

            server.PlayerJoined += HandlePlayerJoin;
        }

        private void IncrementTime(object sender, ElapsedEventArgs e)
        {
            this.minute++;
            if (this.minute >= 60)
            {
                this.minute %= 60;
                this.hour = (byte)((this.hour + 1) % 24);
            }
        }

        #region methods

        private void HandlePlayerJoin(Player player)
        {
            if (this.fogDistance != null)
                player.Client.SendPacket(new SetFogDistancePacket(this.fogDistance.Value));

            if (this.farClipDistance != null)
                player.Client.SendPacket(new SetFarClipDistancePacket(this.farClipDistance.Value));

            player.Client.SendPacket(new SetAircraftMaxHeightPacket(this.aircraftMaxHeight));
            player.Client.SendPacket(new SetAircraftMaxVelocityPacket(this.aircraftMaxVelocity));
            player.Client.SendPacket(new SetCloudsEnabledPacket(this.cloudsEnabled));
            player.Client.SendPacket(new SetGameSpeedPacket(this.gameSpeed));

            foreach(var kvPair in this.garageStates)
            {
                player.Client.SendPacket(new SetGarageOpenPacket((byte)kvPair.Key, kvPair.Value));
            }

            player.Client.SendPacket(new SetGravityPacket(this.gravity));
            player.Client.SendPacket(new SetInteriorSoundsEnabledPacket(this.interiorSoundsEnabled));

            foreach (var kvPair in this.jetpackEnabledWeapons)
            {
                player.Client.SendPacket(new SetJetpackWeaponEnabledPacket((byte)kvPair.Key, kvPair.Value));
            }

            player.Client.SendPacket(new SetMinuteDurationPacket(this.minuteDuration));
            player.Client.SendPacket(new SetMoonSizePacket(this.moonSize));
            player.Client.SendPacket(new SetOcclusionsEnabledPacket(this.occlusionsEnabled));
            player.Client.SendPacket(new SetRainLevelPacket(this.rainLevel));

            if (this.skyGradientTopColor != null && this.skyGradientBottomColor != null)
                player.Client.SendPacket(new SetSkyGradientPacket(this.skyGradientTopColor, this.skyGradientBottomColor));

            if (this.sunCoreColor != null && this.sunCoronaColor != null)
                player.Client.SendPacket(new SetSunColorPacket(this.sunCoreColor.Value, this.sunCoronaColor.Value));

            player.Client.SendPacket(new SetSunSizePacket(this.sunSize));
            player.Client.SendPacket(new SetTimePacket(this.hour, this.minute));
            player.Client.SendPacket(new SetTrafficLightStatePacket((byte)this.trafficLightState, this.trafficLightStateForced));
            player.Client.SendPacket(new SetWeatherPacket(this.Weather));
            player.Client.SendPacket(new SetWindVelocityPacket(this.windVelocity));
        }

        public void SetWeather(byte weather)
        {
            this.Weather = weather;
            this.server.BroadcastPacket(new SetWeatherPacket(weather));
        }

        public void SetWeather(Weather weather) => SetWeather((byte)weather);

        public void SetWeatherBlended(byte weather, byte hours = 1)
        {
            this.Weather = weather;
            this.server.BroadcastPacket(new SetWeatherBlendedPacket(weather, (byte)((this.hour + hours) % 24)));
        }

        public void SetWeatherBlended(Weather weather, byte hours = 1) => SetWeatherBlended((byte)weather, hours);

        public void SetGarageOpen(GarageLocation garage, bool isOpen)
        {
            this.garageStates[garage] = isOpen;
            this.server.BroadcastPacket(new SetGarageOpenPacket((byte)garage, isOpen));
        }

        public bool IsGarageOpen(GarageLocation garage)
        {
            this.garageStates.TryGetValue(garage, out bool value);
            return value;
        }

        public void SetJetpackWeaponEnabled(WeaponId weapon, bool isEnabled)
        {
            this.jetpackEnabledWeapons[weapon] = isEnabled;
            this.server.BroadcastPacket(new SetJetpackWeaponEnabledPacket((byte)weapon, isEnabled));
        }

        public bool IsJetpackWeaponEnabled(WeaponId weapon)
        {
            this.jetpackEnabledWeapons.TryGetValue(weapon, out bool value);
            return value;
        }

        public void SetSkyGradient(Color top, Color bottom)
        {
            this.skyGradientTopColor = top;
            this.skyGradientBottomColor = bottom;

            this.server.BroadcastPacket(new SetSkyGradientPacket(top, bottom));
        }

        public Tuple<Color, Color>? GetSkyGradient()
        {
            return (this.skyGradientTopColor != null && this.skyGradientBottomColor != null) ?
                new Tuple<Color, Color>(this.skyGradientTopColor.Value, this.skyGradientBottomColor.Value) :
                null;
        }

        public void SetSunColor(Color core, Color corona)
        {
            this.sunCoreColor = core;
            this.sunCoronaColor = corona;

            this.server.BroadcastPacket(new SetSunColorPacket(core, corona));
        }

        public Tuple<Color, Color>? GetSunColor()
        {
            return (this.sunCoreColor != null && this.sunCoronaColor != null) ?
                new Tuple<Color, Color>(this.sunCoreColor.Value, this.sunCoronaColor.Value) :
                null;
        }

        public void SetTrafficLightState(TrafficLightState state, bool forced = false)
        {
            this.trafficLightState = state;
            this.trafficLightStateForced = forced;
            this.server.BroadcastPacket(new SetTrafficLightStatePacket((byte)state, forced));
        }

        public void SetTime(byte hour, byte minute)
        {
            this.hour = hour;
            this.minute = minute;
            this.server.BroadcastPacket(new SetTimePacket(hour, minute));
        }

        public Tuple<byte, byte> GetTime()
        {
            return new Tuple<byte, byte>(hour, minute);
        }

        public void CreateProjectile(Vector3 from, Vector3 direction, uint sourceElement)
        {
            this.server.BroadcastPacket(new ProjectileSyncPacket(from, direction, sourceElement));
        }

        #endregion
    }
}
