using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Factories;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Elements
{
    public class Player: Ped
    {
        public override ElementType ElementType => ElementType.Player;

        public Client Client { get; }
        public Camera Camera { get; }

        private byte wantedLevel = 0;
        public byte WantedLevel
        {
            get => this.wantedLevel;
            set
            {
                var args = new ElementChangedEventArgs<Player, byte>(this, this.WantedLevel, value, this.IsSync);
                this.wantedLevel = value;
                WantedLevelChanged?.Invoke(this, args);
            }
        }

        public Element? ContactElement { get; set; }

        public Vector3 AimOrigin { get; set; }
        public Vector3 AimDirection { get; set; }

        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraDirection { get; set; }
        public float CameraRotation { get; set; }

        public bool IsInWater { get; set; }
        public bool IsOnGround { get; set; }
        public bool IsDucked { get; set; }
        public bool WearsGoggles { get; set; }
        public bool HasContact { get; set; }
        public bool IsChoking { get; set; }
        public bool AkimboTargetUp { get; set; }
        public bool IsOnFire { get; set; }
        public bool IsSyncingVelocity { get; set; }
        public bool IsStealthAiming { get; set; }
        public bool IsVoiceMuted { get; set; }
        public bool IsChatMuted { get; set; }

        public Dictionary<int, PlayerPendingScreenshot> PendingScreenshots { get; } = new Dictionary<int, PlayerPendingScreenshot>();

        protected internal Player(Client client) : base(0, Vector3.Zero)
        {
            this.Client = client;
            this.Camera = new Camera(this);
        }

        public new Player AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public void Spawn(Vector3 position, float rotation, ushort model, byte interior, ushort dimension)
        {
            this.position = position;
            this.PedRotation = rotation;
            this.model = model;
            this.interior = interior;
            this.dimension = dimension;

            this.Spawned?.Invoke(this, new PlayerSpawnedEventArgs(this));
        }

        public void ShowHudComponent(HudComponent hudComponent, bool isVisible)
        {
            this.Client.SendPacket(PlayerPacketFactory.CreateShowHudComponentPacket(hudComponent, isVisible));
        }

        public void SetFpsLimit(ushort limit)
        {
            this.Client.SendPacket(PlayerPacketFactory.CreateSetFPSLimitPacket(limit));
        }

        public void PlaySound(byte sound)
        {
            this.Client.SendPacket(PlayerPacketFactory.CreatePlaySoundPacket(sound));
        }

        public void ForceMapVisible(bool isVisible)
        {
            this.Client.SendPacket(PlayerPacketFactory.CreateForcePlayerMapPacket(isVisible));
        }

        public void ToggleAllControls(bool isEnabled, bool gtaControls = true, bool mtaControls = true)
        {
            this.Client.SendPacket(PlayerPacketFactory.CreateToggleAllControlsPacket(isEnabled, gtaControls, mtaControls));
        }

        public void TriggerCommand(string command, string[] arguments)
        {
            this.OnCommand?.Invoke(this, new PlayerCommandEventArgs(this, command, arguments));
        }

        public void TriggerDamaged(Element? damager, WeaponType damageType, BodyPart bodyPart)
        {
            this.Damaged?.Invoke(this, new PlayerDamagedEventArgs(this, damager, damageType, bodyPart));
        }

        public void Kill(Element? damager, WeaponType damageType, BodyPart bodyPart, ulong animationGroup = 0, ulong animationId = 15)
        {
            this.RunAsSync(() =>
            {
                this.health = 0;
                this.Wasted?.Invoke(this, new PlayerWastedEventArgs(this, damager, damageType, bodyPart, animationGroup, animationId));
            });
        }

        public void Kill(WeaponType damageType = WeaponType.WEAPONTYPE_UNARMED, BodyPart bodyPart = BodyPart.Torso)
        {
            this.Kill(null, damageType, bodyPart);
        }

        public void VoiceDataStart(byte[] voiceData)
        {
            if (!this.IsVoiceMuted)
                this.OnVoiceData?.Invoke(this, new PlayerVoiceStartArgs(this, voiceData));
        }

        public void VoiceDataEnd()
        {
            this.OnVoiceDataEnd?.Invoke(this, new PlayerVoiceEndArgs(this));
        }

        public void TriggerDisconnected(QuitReason reason)
        {
            this.Disconnected?.Invoke(this, new PlayerQuitEventArgs(reason));
            this.Destroy();
        }

        public void TakeScreenshot(ushort width, ushort height, string tag = "", byte quality = 30, uint maxBandwith = 5000, ushort maxPacketSize = 500)
        {
            quality = Math.Clamp(quality, (byte)0, (byte)100);
            this.Client.SendPacket(ElementPacketFactory.CreateTakePlayerScreenshotPacket(this, width, height, tag, quality, maxBandwith, maxPacketSize, null));
        }

        public void ScreenshotEnd(int screenshotId)
        {
            var pendingScreenshot = this.PendingScreenshots[screenshotId];
            if (pendingScreenshot != null && pendingScreenshot.Stream != null)
            {
                using (var stream = pendingScreenshot.Stream)
                {
                    this.OnScreenshot?.Invoke(this, new ScreenshotEventArgs(pendingScreenshot.Stream, pendingScreenshot.ErrorMessage, pendingScreenshot.Tag));
                }
                this.PendingScreenshots.Remove(screenshotId);
            }
        }

        public event ElementChangedEventHandler<Player, byte>? WantedLevelChanged;
        public event EventHandler<PlayerDamagedEventArgs>? Damaged;
        public event EventHandler<PlayerWastedEventArgs>? Wasted;
        public event EventHandler<PlayerSpawnedEventArgs>? Spawned;
        public event EventHandler<PlayerCommandEventArgs>? OnCommand;
        public event EventHandler<PlayerVoiceStartArgs>? OnVoiceData;
        public event EventHandler<PlayerVoiceEndArgs>? OnVoiceDataEnd;
        public event EventHandler<PlayerQuitEventArgs>? Disconnected;
        public event EventHandler<ScreenshotEventArgs>? OnScreenshot;
    }
}
