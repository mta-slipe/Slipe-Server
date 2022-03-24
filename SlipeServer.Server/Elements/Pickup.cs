using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements
{
    public class Pickup : Element
    {
        public override ElementType ElementType => ElementType.Pickup;

        public ushort Model { get; set; }
        public bool IsVisible { get; set; } = true;
        public PickupType PickupType { get; set; }
        public float? Armor { get; set; }
        public float? Health { get; set; }
        public WeaponType? WeaponType { get; set; }
        public ushort? Ammo{ get; set; }
        public CollisionShape CollisionShape { get; init; }
        public bool OnFootOnly { get; set; } = true;
        public uint RespawnTime { get; set; }

        private readonly object useLock = new();


        public Pickup(Vector3 position, PickupType type, float amount)
        {
            this.Position = position;
            this.PickupType = type;

            if (type == PickupType.Health)
            {
                this.Health = amount;
                this.Model = (ushort)ObjectModel.Health;
            }
            else if (type == PickupType.Armor)
            {
                this.Armor = amount;
                this.Model = (ushort)ObjectModel.Bodyarmour;
            }
            else
                throw new Exception($"Can not use health / armor pickup constructor for {type}");

            this.CollisionShape = new CollisionSphere(position, 2);
            this.CollisionShape.ElementEntered += HandleCollisionHit;
        }

        public Pickup(Vector3 position, WeaponType type, ushort ammo)
        {
            this.Position = position;
            this.Ammo = ammo;
            this.WeaponType = type;
            this.Model = WeaponConstants.ModelsPerWeapon[(WeaponId)type];

            this.PickupType = PickupType.Weapon;

            this.CollisionShape = new CollisionSphere(position, 2);
            this.CollisionShape.ElementEntered += HandleCollisionHit;
        }

        public Pickup(Vector3 position, ushort model)
        {
            this.Position = position;
            this.Model = model;
            this.PickupType = PickupType.Custom;

            this.CollisionShape = new CollisionSphere(position, 2);
            this.CollisionShape.ElementEntered += HandleCollisionHit;
        }

        public Pickup(Vector3 position, PickupModel model)
        {
            this.Position = position;
            this.Model = (ushort)model;
            this.PickupType = PickupType.Custom;

            this.CollisionShape = new CollisionSphere(position, 2);
            this.CollisionShape.ElementEntered += HandleCollisionHit;
        }

        private void HandleCollisionHit(Element element)
        {
            if (element is Player player)
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
            switch (this.PickupType)
            {
                case PickupType.Health:
                    player.Health = MathF.Min(player.Health + (this.Health ?? 100), 200.0f);
                    break;

                case PickupType.Armor:
                    player.Armor = MathF.Min(player.Armor + (this.Armor ?? 100), 100.0f);
                    break;

                case PickupType.Weapon:
                    player.AddWeapon((WeaponId)(this.WeaponType ?? Elements.WeaponType.WEAPONTYPE_AK47), this.Ammo ?? 1, false);
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

        public new Pickup AssociateWith(MtaServer server)
        {
            this.CollisionShape.AssociateWith(server);
            return server.AssociateElement(this);
        }

        public event ElementEventHandler<Pickup, PickupUsedEventArgs>? Used;
        public event ElementEventHandler<Pickup, EventArgs>? Reset;
    }
}
