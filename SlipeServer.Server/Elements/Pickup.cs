using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System;
using System.Numerics;
using System.Threading;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A pickup element
/// Pickups are rotating objects that have logic when a player collides with them.
/// Some default pickups are weapon, health and armor pickups. But you can also have custom pickups with custom logic when hit
/// </summary>
public class Pickup : Element
{
    public override ElementType ElementType => ElementType.Pickup;

    public ushort Model{ get; private set; }
    public PickupType PickupType { get; private set; }


    public float? Armor { get; private set; }
    public float? Health { get; private set; }
    public WeaponId? WeaponType { get; private set; }
    public ushort? Ammo { get; private set; }

    public bool IsVisible { get; set; } = true;
    public CollisionShape CollisionShape { get; init; }
    public bool OnFootOnly { get; set; } = true;
    public uint RespawnTime { get; set; }
    public bool IsUsable { get; set; } = true;

    private readonly Lock useLock = new();


    public Pickup(Vector3 position, PickupType type, float amount) : base()
    {
        this.Position = position;
        this.PickupType = type;

        if (type == PickupType.Health)
        {
            this.Health = amount;
            this.Model = (ushort)ObjectModel.Health;
        } else if (type == PickupType.Armor)
        {
            this.Armor = amount;
            this.Model = (ushort)ObjectModel.Bodyarmour;
        } else
            throw new Exception($"Can not use health / armor pickup constructor for {type}");

        this.CollisionShape = new CollisionSphere(position, 2);
        this.CollisionShape.ElementEntered += HandleCollisionHit;

        this.PositionChanged += UpdatePosition;
    }

    public Pickup(Vector3 position, WeaponId type, ushort ammo) : base()
    {
        this.Position = position;
        this.Ammo = ammo;
        this.WeaponType = type;
        this.Model = WeaponConstants.ModelsPerWeapon[(WeaponId)type];

        this.PickupType = PickupType.Weapon;

        this.CollisionShape = new CollisionSphere(position, 2);
        this.CollisionShape.ElementEntered += HandleCollisionHit;
    }

    public Pickup(Vector3 position, ushort model) : base()
    {
        this.Position = position;
        this.Model = model;
        this.PickupType = PickupType.Custom;

        this.CollisionShape = new CollisionSphere(position, 2);
        this.CollisionShape.ElementEntered += HandleCollisionHit;
    }

    public Pickup(Vector3 position, PickupModel model) : base()
    {
        this.Position = position;
        this.Model = (ushort)model;
        this.PickupType = PickupType.Custom;

        this.CollisionShape = new CollisionSphere(position, 2);
        this.CollisionShape.ElementEntered += HandleCollisionHit;
    }

    public void ChangeToOrUpdateHealthPickup(float amount)
    {
        this.Health = amount;
        this.Model = (ushort)ObjectModel.Health;
        this.PickupType = PickupType.Health;
        this.PickupTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeToOrUpdateArmorPickup(float amount)
    {
        this.Armor = amount;
        this.Model = (ushort)ObjectModel.Bodyarmour;
        this.PickupType = PickupType.Armor;
        this.PickupTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeToOrUpdateWeaponPickup(WeaponId weapon, ushort ammo)
    {
        this.Position = position;
        this.Ammo = ammo;
        this.WeaponType = weapon;
        this.Model = WeaponConstants.ModelsPerWeapon[weapon];
        this.PickupTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeToOrUpdateCustomPickup(ushort model)
    {
        this.Model = model;
        this.PickupType = PickupType.Custom;
        this.PickupTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeToOrUpdateCustomPickup(PickupModel model)
    {
        this.Model = (ushort)model;
        this.PickupType = PickupType.Custom;
        this.PickupTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleCollisionHit(CollisionShape collisionShape, CollisionShapeHitEventArgs args)
    {
        if (args.Element is Player player)
        {
            lock (this.useLock)
            {
                if (CanBeUsedBy(player) && this.IsVisible)
                    Use(player);
            }
        }
    }

    public void Use(Player player)
    {
        if (!this.IsUsable)
            return;

        switch (this.PickupType)
        {
            case PickupType.Health:
                player.Health = MathF.Min(player.Health + (this.Health ?? 100), 200.0f);
                break;

            case PickupType.Armor:
                player.Armor = MathF.Min(player.Armor + (this.Armor ?? 100), 100.0f);
                break;

            case PickupType.Weapon:
                player.AddWeapon(this.WeaponType ?? WeaponId.Ak47, this.Ammo ?? 1, false);
                break;
            default:
                break;
        }

        if (this.RespawnTime > 0)
        {
            this.IsVisible = false;
            var timer = new System.Timers.Timer(this.RespawnTime)
            {
                Enabled = true,
                AutoReset = false,
            };
            timer.Elapsed += ResetPickup;
            timer.Start();
        }

        this.Used?.Invoke(this, new(player, this.IsVisible));
    }

    private void ResetPickup(object? sender, System.Timers.ElapsedEventArgs e)
    {
        this.IsVisible = true;
        this.Reset?.Invoke(this, EventArgs.Empty);
    }

    public bool CanBeUsedBy(Player player)
    {
        if (this.OnFootOnly && player.Vehicle != null)
            return false;

        return this.PickupType switch
        {
            PickupType.Health => (player.Health < 200.0f),
            PickupType.Armor => (player.Armor < 100.0f),
            PickupType.Weapon or PickupType.Custom => true,
            _ => false,
        };
    }

    public new Pickup AssociateWith(IMtaServer server)
    {
        this.CollisionShape.AssociateWith(server);
        base.AssociateWith(server);
        return this;
    }

    private void UpdatePosition(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        this.CollisionShape.Position = args.NewValue;
    }

    public event ElementEventHandler<Pickup, EventArgs>? PickupTypeChanged;
    public event ElementEventHandler<Pickup, PickupUsedEventArgs>? Used;
    public event ElementEventHandler<Pickup, EventArgs>? Reset;
}
