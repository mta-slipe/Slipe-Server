using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using SlipeServer.Packets.Constants;
using System.Drawing;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A player element
/// Players are the representation of any client that connects to the server.
/// Players are synchronised using sync packets that are sent by the client.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Player : Ped
{
    public override ElementType ElementType => ElementType.Player;

    private IClient client = new TemporaryClient();
    public IClient Client
    {
        get => this.client;
        set
        {
            if (this.client is TemporaryClient temporaryClient)
            {
                while (temporaryClient.PacketQueue.TryDequeue(out var queuedPacket))
                    value.SendPacket(queuedPacket.PacketId, queuedPacket.Data, queuedPacket.priority, queuedPacket.reliability);
            }
            this.client = value;
        }
    }

    public Camera Camera { get; }

    private byte wantedLevel = 0;
    public byte WantedLevel
    {
        get => this.wantedLevel;
        set
        {
            if (this.wantedLevel == value)
                return;

            var args = new ElementChangedEventArgs<Player, byte>(this, this.WantedLevel, value, this.IsSync);
            this.wantedLevel = value;
            WantedLevelChanged?.Invoke(this, args);
        }
    }

    /// <summary>
    /// Any elements that are specifically associated with this player. This does not include elements that are associated with the server as a whole.
    /// </summary>
    public IElementCollection AssociatedElements { get; } = new RTreeCompoundElementCollection(); 

    public Element? ContactElement { get; set; }

    public Vector3 AimOrigin { get; set; }
    public Vector3 AimDirection { get; set; }

    public Vector3 CameraPosition { get; set; }
    public Vector3 CameraDirection { get; set; }
    public float CameraRotation { get; set; }

    public bool IsOnGround { get; set; }
    public bool IsDucked { get; set; }
    public bool WearsGoggles { get; set; }
    public bool HasContact { get; set; }
    public bool IsChoking { get; set; }
    public bool AkimboTargetUp { get; set; }
    public bool IsSyncingVelocity { get; set; }
    public bool IsStealthAiming { get; set; }
    public bool IsVoiceMuted { get; set; }
    public bool IsChatMuted { get; set; }
    public List<Ped> SyncingPeds { get; set; }
    public List<Vehicle> SyncingVehicles { get; set; }
    public Controls Controls { get; private set; }

    private Team? team;
    public Team? Team
    {
        get => this.team;
        set
        {
            if (this.team == value)
                return;

            var previousTeam = this.team;
            this.team = value;
            this.TeamChanged?.Invoke(this, new PlayerTeamChangedArgs(this, value, previousTeam));
            this.team?.Players.Add(this);
        }
    }

    public Dictionary<int, PlayerPendingScreenshot> PendingScreenshots { get; } = [];

    private readonly HashSet<Element> subscriptionElements;
    private Dictionary<string, KeyState> BoundKeys { get; }

    private int money;
    public int Money
    {
        get => this.money;
        set
        {
            if (this.money == value)
                return;

            int clampedMoney = Math.Clamp(value, -99999999, 99999999);
            var previousTeam = this.money;
            this.money = clampedMoney;
            this.MoneyChanged?.Invoke(this, new PlayerMoneyChangedEventArgs(this, clampedMoney, true));
        }
    }

    private string? nametagText;
    public string NametagText
    {
        get => this.nametagText ?? this.Name;
        set
        {
            if (this.nametagText == value)
                return;

            var args = new ElementChangedEventArgs<Player, string>(this, this.NametagText, value, this.IsSync);
            this.nametagText = value;
            NametagTextChanged?.Invoke(this, args);
        }
    }

    private bool isNametagShowing = true;
    public bool IsNametagShowing
    {
        get => this.isNametagShowing;
        set
        {
            if (this.isNametagShowing == value)
                return;

            var args = new ElementChangedEventArgs<Player, bool>(this, this.IsNametagShowing, value, this.IsSync);
            this.isNametagShowing = value;
            IsNametagShowingChanged?.Invoke(this, args);
        }
    }

    private Color? nametagColor;
    public Color? NametagColor
    {
        get => this.nametagColor;
        set
        {
            if (this.nametagColor == value)
                return;

            var args = new ElementChangedEventArgs<Player, Color?>(this, this.NametagColor, value, this.IsSync);
            this.nametagColor = value;
            NametagColorChanged?.Invoke(this, args);
        }

    }

    private string DebuggerDisplay => $"{this.Name} ({this.Id})";

    private int pureSyncPacketsCount;

    public Player() : base(0, Vector3.Zero)
    {
        this.Camera = new Camera(this);
        this.subscriptionElements = [];
        this.SyncingPeds = [];
        this.SyncingVehicles = [];
        this.BoundKeys = [];
        this.Controls = new(this);
        this.UpdateAssociatedPlayers();

        this.Disconnected += HandleDisconnect;
    }

    private void HandleDisconnect(Player sender, PlayerQuitEventArgs e)
    {
        if (this.Vehicle != null)
            this.Vehicle.RunAsSync(() => this.Vehicle.RemovePassenger(this));
        EnteringVehicle = null;
    }

    public new Player AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
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
        this.Reset();
        this.health = 100;
        this.armor = 0;

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

    public void SetTransferBoxVisible(bool isVisible)
    {
        this.Client.SendPacket(PlayerPacketFactory.CreateTransferBoxVisiblePacket(isVisible));
    }

    public void ToggleAllControls(bool isEnabled, bool gtaControls = true, bool mtaControls = true)
    {
        this.Client.SendPacket(PlayerPacketFactory.CreateToggleAllControlsPacket(isEnabled, gtaControls, mtaControls));
    }

    public void TriggerCommand(string command, string[] arguments)
    {
        this.CommandEntered?.Invoke(this, new PlayerCommandEventArgs(this, command, arguments));
    }

    public void TriggerDamaged(Element? damager, DamageType damageType, BodyPart bodyPart)
    {
        this.Damaged?.Invoke(this, new PlayerDamagedEventArgs(this, damager, damageType, bodyPart));
    }

    public override void Kill(Element? damager, DamageType damageType, BodyPart bodyPart, ulong animationGroup = 0, ulong animationId = 15)
    {
        this.RunAsSync(() =>
        {
            Reset();
            InvokeWasted(new PedWastedEventArgs(this, damager, damageType, bodyPart, animationGroup, animationId));
        });
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
        if (this.Destroy())
            this.Disconnected?.Invoke(this, new PlayerQuitEventArgs(reason));        
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

    public void Kick(PlayerDisconnectType type = PlayerDisconnectType.CUSTOM)
    {
        this.Kicked?.Invoke(this, new PlayerKickEventArgs(string.Empty, type));
        this.TriggerDisconnected(QuitReason.Kick);
        this.Client.SendPacket(new PlayerDisconnectPacket(type, string.Empty));
        this.Client.IsConnected = false;
        this.Client.SetDisconnected();
        this.Destroy();
    }

    public void Kick(string reason, PlayerDisconnectType type = PlayerDisconnectType.CUSTOM)
    {
        this.Kicked?.Invoke(this, new PlayerKickEventArgs(reason, type));
        this.TriggerDisconnected(QuitReason.Kick);
        this.Client.SendPacket(new PlayerDisconnectPacket(type, reason));
        this.Client.IsConnected = false;
        this.Client.SetDisconnected();
        this.Destroy();
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

    public void SetMoney(int money, bool instant = false)
    {
        int clampedMoney = Math.Clamp(money, -99999999, 99999999);
        var args = new PlayerMoneyChangedEventArgs(this, clampedMoney, instant);
        this.money = clampedMoney;
        MoneyChanged?.Invoke(this, args);
    }

    public void SetBind(KeyConstants.Controls control, KeyState keyState) => SetBind(KeyConstants.ControlToString(control), keyState);
    public void SetBind(KeyConstants.Keys key, KeyState keyState) => SetBind(KeyConstants.KeyToString(key), keyState);

    public void SetBind(string key, KeyState keyState = KeyState.Down)
    {
        if(!KeyConstants.IsValid(key))
            throw new ArgumentException($"Key '{key}' is not valid.", key);

        if (keyState == KeyState.None)
        {
            this.RemoveBind(key);
            return;
        }

        this.BoundKeys[key] = keyState;
        this.KeyBound?.Invoke(this, new PlayerBindKeyArgs(this, key, keyState));
    }

    public void RemoveBind(string key, KeyState keyState = KeyState.Down)
    {
        if (!KeyConstants.IsValid(key))
            throw new ArgumentException($"Key '{key}' is not valid.", key);

        this.BoundKeys.Remove(key);
        this.KeyUnbound?.Invoke(this, new PlayerBindKeyArgs(this, key, keyState));
    }

    public void TriggerLuaEvent(string name, Element? source = null, params LuaValue[] parameters)
    {
        new LuaEventPacket(name, (source ?? this).Id, parameters).SendTo(this);
    }

    public void TriggerPlayerACInfo(IEnumerable<byte> detectedACList, uint d3d9Size, string d3d9MD5, string D3d9SHA256)
    {
        this.AcInfoReceived?.Invoke(this, new PlayerACInfoArgs(detectedACList, d3d9Size, d3d9MD5, D3d9SHA256));
    }

    public void TriggerPlayerDiagnosticInfo(uint level, string message)
    {
        this.DiagnosticInfoReceived?.Invoke(this, new PlayerDiagnosticInfo(level, message));
    }

    public void TriggerPlayerModInfo(string infoType, IEnumerable<ModInfoItem> modInfoItems)
    {
        this.ModInfoReceived?.Invoke(this, new PlayerModInfoArgs(infoType, modInfoItems));
    }

    public void TriggerNetworkStatus(PlayerNetworkStatusType networkStatusType, uint ticks)
    {
        this.NetworkStatusReceived?.Invoke(this, new PlayerNetworkStatusArgs(networkStatusType, ticks));
    }

    public void TriggerResourceStarted(ushort netId)
    {
        this.ResourceStarted?.Invoke(this, new PlayerResourceStartedEventArgs(this, netId));
    }

    public void TriggerBoundKey(BindType bindType, KeyState keyState, string key)
    {
        this.BindExecuted?.Invoke(this, new PlayerBindExecutedEventArgs(this, bindType, keyState, key));
    }

    public void TriggerCursorClicked(byte button,
        Point position,
        Vector3 worldPosition,
        Element? element)
    {
        this.CursorClicked?.Invoke(this, new PlayerCursorClickedEventArgs(button, position, worldPosition, element));
    }

    public void TriggerJoined()
    {
        this.Joined?.Invoke(this, EventArgs.Empty);
    }

    public Blip CreateBlipFor(BlipIcon icon, ushort visibleDistance = 16000, short ordering = 0, byte size = 2, Color? color = null)
    {
        var blip = new Blip(this.position, icon, visibleDistance, ordering)
        {
            Size = size,
            Color = color ?? Color.White
        };

        void attachBlip(Element element, EventArgs args)
        {
            blip.AttachTo(this);
            this.Joined -= attachBlip;
        }

        if (this.Id.Value == 0)
            this.Joined += attachBlip;
        else
            blip.AttachTo(this);

        return blip;
    }

    /// <summary>
    /// Associates an element with the player. Meaning the element will be created for the player
    /// and changes to the element will be relayed to the player.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <returns>Returns the element, allowing for method chaining</returns>
    public T AssociateElement<T>(T element) where T : Element
    {
        this.AssociatedElements.Add(element);

        return element;
    }

    /// <summary>
    /// Removes an element from being associated with the player, meaning the element will no longer be sync'd to the player
    /// </summary>
    /// <param name="element"></param>
    public void RemoveElement(Element element)
    {
        this.AssociatedElements.Remove(element);
    }

    internal bool ShouldSendReturnSyncPacket()
    {
        return this.pureSyncPacketsCount++ % 4 == 0;
    }

    public event ElementChangedEventHandler<Player, byte>? WantedLevelChanged;
    public event ElementChangedEventHandler<Player, string>? NametagTextChanged;
    public event ElementChangedEventHandler<Player, bool>? IsNametagShowingChanged;
    public event ElementChangedEventHandler<Player, Color?>? NametagColorChanged;
    public event ElementEventHandler<Player, PlayerDamagedEventArgs>? Damaged;
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
    public event ElementEventHandler<Player, PlayerMoneyChangedEventArgs>? MoneyChanged;
    public event ElementEventHandler<Player, PlayerBindKeyArgs>? KeyBound;
    public event ElementEventHandler<Player, PlayerBindKeyArgs>? KeyUnbound;
    public event ElementEventHandler<Player, PlayerResourceStartedEventArgs>? ResourceStarted;
    public event ElementEventHandler<Player, PlayerBindExecutedEventArgs>? BindExecuted;
    public event ElementEventHandler<Player, PlayerCursorClickedEventArgs>? CursorClicked;
    public event ElementEventHandler<Player, EventArgs>? Joined;
}
