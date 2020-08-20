using MtaServer.Packets;
using MtaServer.Packets.Definitions.Entities.Structs;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;
using MtaServer.Server.Elements;
using MtaServer.Server.Elements.ColShapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MtaServer.Server.PacketHandling.Builders
{
    public class AddEntityPacketBuilder
    {
        private readonly AddEntityPacket packet;

        public AddEntityPacketBuilder()
        {
            this.packet = new AddEntityPacket();
        }

        public void AddDummy(DummyElement element)
        {
            packet.AddDummy(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext, 
                element.ElementTypeName, element.Position);
        }

        public void AddObject(WorldObject element)
        {
            packet.AddObject(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Position, element.Rotation, element.Model, element.Alpha, element.IsLowLod, element.LowLodElement?.Id, 
                element.DoubleSided, element.IsVisibleInAllDimensions, element.Movement, element.Scale, element.IsFrozen, element.Health
            );
        }

        public void AddBlip(Blip element)
        {
            packet.AddBlip(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Position, element.Ordering, element.VisibleDistance, (byte)element.Icon, element.Size, element.Color
            );
        }

        public void AddColShape(CollisionShape element)
        {
            switch (element)
            {
                case CollisionCircle collisionCircle:
                    packet.AddColCircle(collisionCircle.Id, (byte)collisionCircle.ElementType, collisionCircle.Parent?.Id ?? 0, collisionCircle.Interior, collisionCircle.Dimension,
                        null, collisionCircle.AreCollisionsEnabled, collisionCircle.IsCallPropagationEnabled, new CustomData(), collisionCircle.Name, collisionCircle.TimeContext,
                        (byte)ColShapeType.Circle, collisionCircle.Position, collisionCircle.IsEnabled, collisionCircle.AutoCallEvent, collisionCircle.Radius
                    );
                    break;

                case CollisionCuboid collisionCuboid:
                    packet.AddColCuboid(collisionCuboid.Id, (byte)collisionCuboid.ElementType, collisionCuboid.Parent?.Id ?? 0, collisionCuboid.Interior,
                        collisionCuboid.Dimension, null, collisionCuboid.AreCollisionsEnabled, collisionCuboid.IsCallPropagationEnabled, new CustomData(),
                        collisionCuboid.Name, collisionCuboid.TimeContext, (byte)ColShapeType.Cuboid, collisionCuboid.Position, collisionCuboid.IsEnabled,
                        collisionCuboid.AutoCallEvent, collisionCuboid.Dimensions
                    );
                    break;

                case CollisionRectangle collisionRectangle:
                    packet.AddColRectangle(collisionRectangle.Id, (byte)collisionRectangle.ElementType, collisionRectangle.Parent?.Id ?? 0, collisionRectangle.Interior,
                        collisionRectangle.Dimension, null, collisionRectangle.AreCollisionsEnabled, collisionRectangle.IsCallPropagationEnabled, new CustomData(),
                        collisionRectangle.Name, collisionRectangle.TimeContext, (byte)ColShapeType.Rectangle, collisionRectangle.Position, collisionRectangle.IsEnabled,
                        collisionRectangle.AutoCallEvent, collisionRectangle.Dimensions
                    );
                    break;

                case CollisionSphere collisionShpere:
                    packet.AddColSphere(collisionShpere.Id, (byte)collisionShpere.ElementType, collisionShpere.Parent?.Id ?? 0, collisionShpere.Interior, collisionShpere.Dimension,
                        null, collisionShpere.AreCollisionsEnabled, collisionShpere.IsCallPropagationEnabled, new CustomData(), collisionShpere.Name, collisionShpere.TimeContext,
                        (byte)ColShapeType.Sphere, collisionShpere.Position, collisionShpere.IsEnabled, collisionShpere.AutoCallEvent, collisionShpere.Radius
                    );
                    break;

                case CollisionTube collisionTube:
                    packet.AddColTube(collisionTube.Id, (byte)collisionTube.ElementType, collisionTube.Parent?.Id ?? 0, collisionTube.Interior, collisionTube.Dimension,
                        null, collisionTube.AreCollisionsEnabled, collisionTube.IsCallPropagationEnabled, new CustomData(), collisionTube.Name, collisionTube.TimeContext,
                        (byte)ColShapeType.Tube, collisionTube.Position, collisionTube.IsEnabled, collisionTube.AutoCallEvent, collisionTube.Radius, collisionTube.Height
                    );
                    break;

                case CollisionPolygon collisionPolygon:
                    packet.AddColPolygon(collisionPolygon.Id, (byte)collisionPolygon.ElementType, collisionPolygon.Parent?.Id ?? 0, collisionPolygon.Interior, collisionPolygon.Dimension,
                        null, collisionPolygon.AreCollisionsEnabled, collisionPolygon.IsCallPropagationEnabled, new CustomData(), collisionPolygon.Name, collisionPolygon.TimeContext,
                        (byte)ColShapeType.Polygon, collisionPolygon.Position, collisionPolygon.IsEnabled, collisionPolygon.AutoCallEvent, collisionPolygon.Vertices
                    );
                    break;

            }
        }

        public void AddMarker(Marker element)
        {
            packet.AddMarker(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Position, (byte)element.MarkerType, element.Size, element.Color, element.TargetPosition
            );
        }

        public void AddPed(Ped element)
        {
            packet.AddPed(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Position, element.Model, element.PedRotation, element.Health, element.Armor, element.Vehicle?.Id, element.Seat,
                element.HasJetpack, element.IsSyncable, element.IsHeadless, element.IsFrozen, element.Alpha, (byte)element.MoveAnimation,
                element.Clothes, element.Weapons, element.CurrentWeapon?.Slot ?? 0
            );
        }

        public void AddPickup(Pickup element)
        {
            packet.AddPickup(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Position, element.Model, element.IsVisible, (byte)element.PickupType, element.Armor, element.Health, (byte?)element.WeaponType, element.Ammo
            );
        }

        public void AddRadarArea(RadarArea element)
        {
            packet.AddRadarArea(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Position2, element.Size, element.Color, element.IsFlashing
            );
        }

        public void AddTeam(Team element)
        {
            packet.AddTeam(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.TeamName, element.Color, element.IsFriendlyFireEnabled, element.Players.Select(p => p.Id).ToArray()
            );
        }

        public void AddWater(Water element)
        {
            packet.AddWater(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Vertices.ToArray(), element.IsShallow
            );
        }

        public void AddVehicle(Vehicle element)
        {
            packet.AddVehicle(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Position, element.Rotation, element.Model, element.Health, element.Colors, element.PaintJob, element.Damage, element.Variant1,
                element.Variant2, element.TurretDirection, element.AdjustableProperty, element.DoorRatios, element.Upgrades.Select(u => (byte)u).ToArray(), element.PlateText, 
                element.OverrideLights, element.IsLandingGearDown, element.IsSirenActive, element.IsFuelTankExplodable, element.IsEngineOn, element.IsLocked, 
                element.AreDoorsUndamageable, element.IsDamageProof, element.IsFrozen, element.IsDerailed, element.IsDerailable, element.TrainDirection, element.IsTaxiLightOn, 
                element.Alpha, element.HeadlightColor, element.Handling, element.Sirens
            );
        }

        public void AddWeapon(Weapon element)
        {
            packet.AddWeapon(element.Id, (byte)element.ElementType, element.Parent?.Id ?? 0, element.Interior, element.Dimension,
                null, element.AreCollisionsEnabled, element.IsCallPropagationEnabled, new CustomData(), element.Name, element.TimeContext,
                element.Position, element.Rotation, element.Model, element.Alpha, element.IsLowLod, element.LowLodElement?.Id,
                element.DoubleSided, element.IsVisibleInAllDimensions, element.Movement, element.Scale, element.IsFrozen, element.Health,
                (byte)element.TargetType, element.TargetElement?.Id, element.BoneTarget, element.WheelTarget, element.TargetPosition, element.IsChanged,
                element.DamagePerHit, element.Accuracy, element.TargetRange, element.WeaponRange, element.DisableWeaponModel, element.InstantReload,
                element.ShootIfTargetBlocked, element.ShootIfTargetOutOfRange, element.CheckBuildings, element.CheckCarTires, element.CheckDummies,
                element.CheckObjects, element.CheckPeds, element.CheckVehicles, element.IgnoreSomeObjectForCamera, element.SeeThroughStuff,
                element.ShootThroughStuff, (byte)element.WeaponState, element.Ammo, element.ClipAmmo, element.Owner?.Id ?? PacketConstants.InvalidElementId
            );
        }



        public AddEntityPacket Build()
        {
            return this.packet;
        }
    }
}
