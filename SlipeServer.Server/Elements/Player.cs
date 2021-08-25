using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums;

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

        private Team? team;
        public Team? Team
        {
            get => this.team;
            set
            {
                var previousTeam = this.team;
                this.team = value;
                this.TeamChanged?.Invoke(this, new PlayerTeamChangedArgs(this, value, previousTeam));
                this.team?.Players.Add(this);
            }
        }

        public Dictionary<int, PlayerPendingScreenshot> PendingScreenshots { get; } = new();

        private readonly HashSet<Element> subscriptionElements;

        
        protected internal Player(Client client) : base(0, Vector3.Zero)
        {
            this.Client = client;
            this.Camera = new Camera(this);
            this.subscriptionElements = new();
        }

        public new Player AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public void SubscribeTo(Element element)
        {
            if (this.IsSubscribedTo(element))
                return;

            this.subscriptionElements.Add(element);
            this.Subscribed?.Invoke(this, new PlayerSubscriptionEventArgs(this, element));
            element.AddSubscriber(this);
        }

        public void UnsubscribeFrom(Element element)
        {
            if (!this.IsSubscribedTo(element))
                return;

            this.subscriptionElements.Remove(element);
            this.UnSubscribed?.Invoke(this, new PlayerSubscriptionEventArgs(this, element));
            element.RemoveSubscriber(this);
        }

        public bool IsSubscribedTo(Element element) => this.subscriptionElements.Contains(element);

        public void Spawn(Vector3 position, float rotation, ushort model, byte interior, ushort dimension)
        {
            this.position = position;
            this.PedRotation = rotation;
            this.model = model;
            this.interior = interior;
            this.dimension = dimension;

            this.Weapons.Clear(false);

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
            this.CommandEntered?.Invoke(this, new PlayerCommandEventArgs(this, command, arguments));
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
                this.VoiceDataReceived?.Invoke(this, new PlayerVoiceStartArgs(this, voiceData));
        }

        public void VoiceDataEnd()
        {
            this.VoiceDataEnded?.Invoke(this, new PlayerVoiceEndArgs(this));
        }

        public void TriggerDisconnected(QuitReason reason)
        {
            this.Disconnected?.Invoke(this, new PlayerQuitEventArgs(reason));
            this.Destroy();
        }

        public void TakeScreenshot(ushort width, ushort height, string tag = "", byte quality = 30, uint maxBandwith = 5000, ushort maxPacketSize = 500)
        {
            quality = Math.Clamp(quality, (byte)0, (byte)100);
            this.Client.SendPacket(ElementPacketFactory.CreateTakePlayerScreenshotPacket(width, height, tag, quality, maxBandwith, maxPacketSize, null));
        }

        public void ScreenshotEnd(int screenshotId)
        {
            var pendingScreenshot = this.PendingScreenshots[screenshotId];
            if (pendingScreenshot != null && pendingScreenshot.Stream != null)
            {
                using (var stream = pendingScreenshot.Stream)
                {
                    this.ScreenshotTaken?.Invoke(this, new ScreenshotEventArgs(pendingScreenshot.Stream, pendingScreenshot.ErrorMessage, pendingScreenshot.Tag));
                }
                this.PendingScreenshots.Remove(screenshotId);
            }
        }

        public void Kick(string reason)
        {
            this.Kicked?.Invoke(this, new PlayerKickEventArgs(reason, PlayerDisconnectType.CUSTOM));
            this.Client.SendPacket(new PlayerDisconnectPacket(PlayerDisconnectType.CUSTOM, reason));
        }

        public void Kick(PlayerDisconnectType type = PlayerDisconnectType.CUSTOM)
        {
            this.Kicked?.Invoke(this, new PlayerKickEventArgs(string.Empty, type));
            this.Client.SendPacket(new PlayerDisconnectPacket(type, string.Empty));
        }

        public void TriggerSync()
        {
            this.PureSynced?.Invoke(this, EventArgs.Empty);
        }

        public void ResendModPackets()
        {
            this.Client.ResendModPackets();
        }

        public void ResendPlayerACInfo()
        {
            this.Client.ResendPlayerACInfo();
        }

        internal void TriggerPlayerACInfo(IEnumerable<byte> detectedACList, uint d3d9Size, string d3d9MD5, string D3d9SHA256)
        {
            this.AcInfoReceived?.Invoke(this, new PlayerACInfoArgs(detectedACList, d3d9Size, d3d9MD5, D3d9SHA256));
        }

        internal void TriggerPlayerDiagnosticInfo(uint level, string message)
        {
            this.DiagnosticInfoReceived?.Invoke(this, new PlayerDiagnosticInfo(level, message));
        }

        internal void TriggerPlayerModInfo(string infoType, IEnumerable<ModInfoItem> modInfoItems)
        {
            this.ModInfoReceived?.Invoke(this, new PlayerModInfoArgs(infoType, modInfoItems));
        }

        internal void TriggerNetworkStatus(PlayerNetworkStatusType networkStatusType, uint ticks)
        {
            this.NetworkStatusReceived?.Invoke(this, new PlayerNetworkStatusArgs(networkStatusType, ticks));
        }

        public event ElementChangedEventHandler<Player, byte>? WantedLevelChanged;
        public event ElementEventHandler<Player, PlayerDamagedEventArgs>? Damaged;
        public event ElementEventHandler<Player, PlayerWastedEventArgs>? Wasted;
        public event ElementEventHandler<Player, PlayerSpawnedEventArgs>? Spawned;
        public event ElementEventHandler<Player, PlayerCommandEventArgs>? CommandEntered;
        public event ElementEventHandler<Player, PlayerVoiceStartArgs>? VoiceDataReceived;
        public event ElementEventHandler<Player, PlayerVoiceEndArgs>? VoiceDataEnded;
        public event ElementEventHandler<Player, PlayerQuitEventArgs>? Disconnected;
        public event ElementEventHandler<Player, ScreenshotEventArgs>? ScreenshotTaken;
        public event ElementEventHandler<Player, PlayerKickEventArgs>? Kicked;
        public event ElementEventHandler<Player, PlayerSubscriptionEventArgs>? Subscribed;
        public event ElementEventHandler<Player, PlayerSubscriptionEventArgs>? UnSubscribed;
        public event ElementEventHandler<Player, EventArgs>? PureSynced;
        public event ElementEventHandler<Player, PlayerACInfoArgs>? AcInfoReceived;
        public event ElementEventHandler<Player, PlayerDiagnosticInfo>? DiagnosticInfoReceived;
        public event ElementEventHandler<Player, PlayerModInfoArgs>? ModInfoReceived;
        public event ElementEventHandler<Player, PlayerNetworkStatusArgs>? NetworkStatusReceived;
        public event ElementEventHandler<Player, PlayerTeamChangedArgs>? TeamChanged;
    }
}
