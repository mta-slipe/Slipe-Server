using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Map
{
    public class MapInfoPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_MAP_INFO;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public byte Weather { get; set; }
        public byte WeatherBlendingTo { get; set; }
        public byte BlendedWeatherHour { get; set; }
        public (Color top, Color bottom)? SkyGradient { get; set; }
        public (byte intensity, byte randomShift, ushort speedMin, ushort speedMax, short scanSizeX, short scanSizeY, ushort renderSizeX, ushort renderSizeY, bool isInsideBuilder)? HeatHaze { get; set; }
        public (ushort hour, ushort minute) Time { get; set; }
        public ulong MinuteDuration { get; set; }
        public (bool isRadarShowing, bool areNametagsShowing, bool areCloudsEnabled) Flags { get; set; }
        public float Gravity { get; set; }
        public float GameSpeed { get; set; }
        public float WaveHeight { get; set; }
        public float SeaLevel { get; set; }
        public float? NonSeaLevel { get; set; }
        public float? OutsideWorldSeaLevel { get; set; }
        public ushort FpsLimit { get; set; }
        public bool[] GarageStates { get; set; } = new bool[50];

        public (
            bool closeRangeDamage, bool quickReload, bool fastFire, bool fastMove, bool crouchBug, 
            bool hitAnimation, bool fastSprint, bool badDrivebyHitboxes, bool quickStand
        ) BugsEnabled { get; set; }

        public float MaximumJetpackHeight { get; set; }
        public Color? WaterColor { get; set; }
        public bool AreInteriorSoundsEnabled { get; set; }
        public float? RainLevel { get; set; }
        public int? MoonSize { get; set; }
        public float? SunSize { get; set; }
        public (Color core, Color corona)? SunColor { get; set; }
        public Vector3? WindVelocity { get; set; }
        public float? FarClipDistance { get; set; }
        public float? FogDistance { get; set; }
        public float AircraftMaxHeight { get; set; }
        public float AircraftMaxVelocity { get; set; }

        // Weapons 


        public (ushort model, float radius, Vector3 position, byte interior)[] RemovedWorldModels { get; set; } 
            = System.Array.Empty<(ushort model, float radius, Vector3 position, byte interior)>();

        public bool OcclusionsEnabled { get; set; }




        public MapInfoPacket()
        {

        }

        public override void Read(byte[] bytes)
        {

        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            this.WriteWeather(builder);
            this.WriteTime(builder);

            builder.Write(new bool[]
            {
                this.Flags.areNametagsShowing,
                this.Flags.isRadarShowing,
                this.Flags.areCloudsEnabled,
            });

            builder.Write(this.Gravity);
            builder.Write(this.GameSpeed == 1.0f);
            if (this.GameSpeed != 1.0f)
                builder.Write(this.GameSpeed);

            this.WriteSea(builder);

            builder.WriteCompressed(this.FpsLimit);

            builder.Write(this.GarageStates);

            builder.Write(new bool[]
            {
                this.BugsEnabled.closeRangeDamage,
                this.BugsEnabled.quickReload,
                this.BugsEnabled.fastFire,
                this.BugsEnabled.fastMove,
                this.BugsEnabled.crouchBug,
                this.BugsEnabled.hitAnimation,
                this.BugsEnabled.fastSprint,
                this.BugsEnabled.badDrivebyHitboxes,
                this.BugsEnabled.quickStand,
            });

            builder.Write(this.MaximumJetpackHeight);

            builder.Write(this.WaterColor.HasValue);
            if (this.WaterColor.HasValue)
                builder.Write(this.WaterColor.Value, true);

            builder.Write(this.AreInteriorSoundsEnabled);

            builder.Write(this.RainLevel.HasValue);
            if (this.RainLevel.HasValue)
                builder.Write(this.RainLevel.Value);

            builder.Write(this.MoonSize.HasValue);
            if (this.MoonSize.HasValue)
                builder.Write(this.MoonSize.Value);

            builder.Write(this.SunSize.HasValue);
            if (this.SunSize.HasValue)
                builder.Write(this.SunSize.Value);

            builder.Write(this.SunColor.HasValue);
            if (this.SunColor.HasValue)
            {
                builder.Write(this.SunColor.Value.core);
                builder.Write(this.SunColor.Value.corona);
            }

            builder.Write(this.WindVelocity.HasValue);
            if (this.WindVelocity.HasValue)
                builder.Write(this.WindVelocity.Value);

            builder.Write(this.FarClipDistance.HasValue);
            if (this.FarClipDistance.HasValue)
                builder.Write(this.FarClipDistance.Value);

            builder.Write(this.FogDistance.HasValue);
            if (this.FogDistance.HasValue)
                builder.Write(this.FogDistance.Value);

            builder.Write(this.AircraftMaxHeight);
            builder.Write(this.AircraftMaxVelocity);

            // weapons

            this.WriteRemovals(builder);

            builder.Write(this.OcclusionsEnabled);


            return builder.Build();
        }

        private void WriteWeather(PacketBuilder builder)
        {

            builder.Write(this.Weather);
            builder.Write(this.WeatherBlendingTo);
            builder.Write(this.BlendedWeatherHour);

            builder.Write(this.SkyGradient.HasValue);
            if (this.SkyGradient.HasValue)
            {
                builder.Write(this.SkyGradient.Value.top);
                builder.Write(this.SkyGradient.Value.bottom);
            }

            builder.Write(this.HeatHaze.HasValue);
            if (this.HeatHaze.HasValue)
            {
                builder.Write(this.HeatHaze.Value.intensity);
                builder.Write(this.HeatHaze.Value.randomShift);
                builder.WriteCapped(this.HeatHaze.Value.speedMin, 10);
                builder.WriteCapped(this.HeatHaze.Value.speedMax, 10);
                builder.WriteCapped((short)(this.HeatHaze.Value.scanSizeX + 1000), 11);
                builder.WriteCapped((short)(this.HeatHaze.Value.scanSizeY + 1000), 11);
                builder.WriteCapped(this.HeatHaze.Value.renderSizeX, 10);
                builder.WriteCapped(this.HeatHaze.Value.renderSizeY, 10);
                builder.Write(this.HeatHaze.Value.isInsideBuilder);
            }
        }

        private void WriteTime(PacketBuilder builder)
        {
            builder.Write(this.Time.hour);
            builder.Write(this.Time.minute);

            builder.WriteCompressed(this.MinuteDuration);
        }

        private void WriteSea(PacketBuilder builder)
        {
            builder.Write(this.WaveHeight);
            builder.Write(this.SeaLevel);

            builder.Write(this.NonSeaLevel.HasValue);
            if (this.NonSeaLevel.HasValue)
                builder.Write(this.NonSeaLevel.Value);

            builder.Write(this.OutsideWorldSeaLevel.HasValue);
            if (this.OutsideWorldSeaLevel.HasValue)
                builder.Write(this.OutsideWorldSeaLevel.Value);
        }

        private void WriteRemovals(PacketBuilder builder)
        {
            builder.Write(this.WaveHeight);
            builder.Write(this.SeaLevel);

            builder.Write(this.NonSeaLevel.HasValue);
            if (this.NonSeaLevel.HasValue)
                builder.Write(this.NonSeaLevel.Value);

            builder.Write(this.OutsideWorldSeaLevel.HasValue);
            if (this.OutsideWorldSeaLevel.HasValue)
                builder.Write(this.OutsideWorldSeaLevel.Value);
        }

        private void WriteFoo(PacketBuilder builder)
        {

        }
    }
}
