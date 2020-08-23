using MtaServer.Packets.Builder;
using MtaServer.Packets.Definitions.Entities.Structs;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class AddEntityPacket : Packet
    { 
        public override PacketId PacketId => PacketId.PACKET_ID_ENTITY_ADD;
        public override PacketFlags Flags => PacketFlags.PACKET_HIGH_PRIORITY | PacketFlags.PACKET_RELIABLE | PacketFlags.PACKET_SEQUENCED;

        private readonly PacketBuilder builder;
        private uint entityCount;

        public AddEntityPacket()
        {
            this.builder = new PacketBuilder();
            this.entityCount = 0;
        }

        public void AddEntity(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext
        )
        {
            this.entityCount++;

            builder.WriteElementId(elementId);
            builder.Write(elementType);
            builder.WriteElementId(parentId ?? PacketConstants.InvalidElementId);
            builder.Write(interior);
            builder.WriteCompressed(dimension);

            builder.Write(attachment != null);
            if (attachment != null)
            {
                builder.WriteElementId(attachment.Value.ElementId);
                builder.WriteVector3WithZAsFloat(attachment.Value.AttachmentPosition);
                builder.WriteVectorAsUshorts(attachment.Value.AttachmentRotation);
            }

            builder.Write(areCollisionsEnabled);
            builder.Write(isCallPropagationEnabled);

            builder.WriteCompressed((ushort)customData.Items.Length);
            foreach (var item in customData.Items)
            {
                builder.Write(item.Name);
                builder.Write(item.Data);
            }

            builder.WriteCompressed((ushort)name.Length);
            builder.WriteStringWithoutLength(name);
            builder.Write(timeContext);
        }

        public void AddObject(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3 position, Vector3 rotation, ushort model,
            byte alpha, bool isLowLod, uint? lowLodElementId, bool isDoubleSided,
            bool isVisibleInAllDimensions, PositionRotationAnimation? positionRotationAnimation,
            Vector3 scale, bool isFrozen, float health
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteVector3WithZAsFloat(position);
            builder.WriteVectorAsUshorts(rotation);
            builder.WriteCompressed(model);
            builder.WriteCompressed((byte)(255 - alpha));
            builder.Write(isLowLod);
            builder.WriteElementId(lowLodElementId ?? PacketConstants.InvalidElementId);
            builder.Write(isDoubleSided);
            builder.Write(isVisibleInAllDimensions);

            builder.Write(positionRotationAnimation != null);
            if (positionRotationAnimation != null)
            {
                WritePositionRotationAnimation(positionRotationAnimation.Value);
            }

            if (scale.X == scale.Y && scale.Y == scale.Z)
            {
                builder.Write(true);
                builder.Write(scale.X == 1);
                if (scale.X != 1)
                {
                    builder.Write(scale.X);
                }
            }
            else
            {
                builder.Write(false);
                builder.Write(scale);
            }

            builder.Write(isFrozen);
            builder.WriteFloatFromBits(health, 11, 0, 1023.5f, true);
        }

        private void WritePositionRotationAnimation(PositionRotationAnimation positionRotationAnimation, bool resumeMode = true)
        {
            builder.Write(resumeMode);

            if (resumeMode)
            {
                var now = DateTime.UtcNow;
                ulong elapsedTime = (ulong)(now - positionRotationAnimation.StartTime).Ticks / 10000;
                ulong timeRemaining = 0;
                if (positionRotationAnimation.EndTime > now)
                {
                    timeRemaining = (ulong)(positionRotationAnimation.EndTime - now).Ticks / 10000;
                }
                builder.WriteCompressed(elapsedTime);
                builder.WriteCompressed(timeRemaining);
            } else
            {
                var duration = (positionRotationAnimation.EndTime - positionRotationAnimation.StartTime).Ticks / 10000;
                builder.WriteCompressed((ulong)duration);
            }

            builder.WriteVector3WithZAsFloat(positionRotationAnimation.SourcePosition);
            builder.Write(positionRotationAnimation.SourceRotation * (MathF.PI / 180));

            builder.WriteVector3WithZAsFloat(positionRotationAnimation.TargetPosition);
            builder.Write(positionRotationAnimation.DeltaRotationMode);
            if (positionRotationAnimation.DeltaRotationMode)
            {
                builder.Write(positionRotationAnimation.DeltaRotation * (MathF.PI / 180));
            } else
            {
                builder.Write(positionRotationAnimation.TargetRotation * (MathF.PI / 180));
            }

            builder.Write(positionRotationAnimation.EasingType);
            builder.Write(positionRotationAnimation.EasingPeriod);
            builder.Write(positionRotationAnimation.EasingAmplitude);
            builder.Write(positionRotationAnimation.EasingOvershoot);
        }

        public void AddWeapon(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3 position, Vector3 rotation, ushort model,
            byte alpha, bool isLowLod, uint? lowLodElementId, bool isDoubleSided,
            bool isVisibleInAllDimensions, PositionRotationAnimation? positionRotationAnimation,
            Vector3 scale, bool isFrozen, float health, byte targetType, uint? targetElementId,
            byte? boneTarget, byte? wheelTarget, Vector3? targetPosition, bool isChanged, ushort? damagePerHit,
            float? accuracy, float? targetRange, float? weaponRange, bool disableWeaponModel,
            bool instantReload, bool shootIfTargetBlocked, bool shootIfTargetOutOfRange,
            bool checkBuildings, bool checkCarTires, bool checkDummies, bool checkObjects,
            bool checkPeds, bool checkVehicles, bool ignoreSomeObjectForCamera, bool seeThroughStuff,
            bool shootThroughStuff, byte weaponState, ushort ammo, ushort clipAmmo, uint ownerId
        )
        {
            AddObject(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext,
                position, rotation, model, alpha, isLowLod,
                lowLodElementId, isDoubleSided, isVisibleInAllDimensions,
                positionRotationAnimation, scale, isFrozen, health
            );

            builder.WriteCapped(targetType, 3);
            if (targetType == 1 && targetElementId != null)
            {
                builder.WriteElementId(targetElementId.Value);
                if (boneTarget != null)
                {
                    builder.Write(boneTarget.Value);
                }
                if (wheelTarget != null)
                {
                    builder.Write(wheelTarget.Value);
                }

                if (boneTarget == null && wheelTarget == null)
                    throw new Exception($"Can not write weapon with target type {targetType} and no wheel or bone target");

            } else if (targetType == 0)
            {
                if (targetPosition == null)
                    throw new Exception($"Can not write weapon with target type {targetType} and no target position");
                builder.WriteCompressedVector3(targetPosition.Value);
            }

            if (isChanged &&
                damagePerHit != null &&
                accuracy != null &&
                targetRange != null &&
                weaponRange != null
            )
            {
                builder.Write(true);
                builder.WriteCapped(damagePerHit.Value, 12);
                builder.Write(accuracy.Value);
                builder.Write(targetRange.Value);
                builder.Write(weaponRange.Value);
            } else
            {
                builder.Write(false);
            }

            builder.Write(disableWeaponModel);
            builder.Write(instantReload);
            builder.Write(shootIfTargetBlocked);
            builder.Write(shootIfTargetOutOfRange);
            builder.Write(checkBuildings);
            builder.Write(checkCarTires);
            builder.Write(checkDummies);
            builder.Write(checkObjects);
            builder.Write(checkPeds);
            builder.Write(checkVehicles);
            builder.Write(ignoreSomeObjectForCamera);
            builder.Write(seeThroughStuff);
            builder.Write(shootThroughStuff);

            builder.WriteCapped(weaponState, 4);
            builder.Write(ammo);
            builder.Write(clipAmmo);
            builder.WriteElementId(ownerId);
        }

        public void AddPickup(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3 position, ushort model, bool isVisible,
            byte pickupType, float? armor, float? health, byte? weaponType,
            ushort? ammo
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteVector3WithZAsFloat(position);
            builder.WriteCompressed(model);
            builder.Write(isVisible);
            builder.WriteCapped(pickupType, 3);

            if (armor != null)
            {
                builder.WritePlayerArmor(armor.Value);
            }
            if (health != null)
            {
                builder.WritePlayerHealth(health.Value);
            }
            if (weaponType != null && ammo != null)
            {
                builder.WriteCapped(weaponType.Value, 6);
                builder.WriteCompressed(ammo.Value);
            }
        }

        public void AddVehicle(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3 position, Vector3 rotation, ushort model,
            float health, Color[] colors,  byte paintJob, VehicleDamage damage,
            byte variant1, byte variant2, Vector2? turret, ushort? adjustableProperty,
            float[] doorRatios, byte[] upgrades, string plateText, byte overrideLights,
            bool isLandingGearDown, bool isSirenActive, bool isFuelTankExplodable,
            bool isEngineOn, bool isLocked, bool areDoorsUndamageable, bool isDamageProof,
            bool isFrozen, bool isDerailed, bool isDerailable, bool trainDirection, 
            bool isTaxiLightOn, float alpha, Color headlightColor, VehicleHandling? handling,
            VehicleSirenSet? sirens
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteVector3WithZAsFloat(position);
            builder.WriteVectorAsUshorts(rotation);
            builder.Write((byte)(model - 400));
            builder.WriteVehicleHealth(health);

            builder.WriteCapped((byte)colors.Length - 1, 2);
            foreach (var color in colors)
            {
                builder.Write(color);
            }
            builder.WriteCapped(paintJob, 2);

            WriteVehicleDamage(damage.Doors, 3);
            WriteVehicleDamage(damage.Wheels, 2);
            WriteVehicleDamage(damage.Panels, 2);
            WriteVehicleDamage(damage.Lights, 2);


            builder.Write(variant1);
            builder.Write(variant2);

            if (turret != null)
            {
                builder.Write((short)(turret.Value.X * (32767.0f / MathF.PI)));
                builder.Write((short)(turret.Value.Y * (32767.0f / MathF.PI)));
            }

            if (adjustableProperty != null)
            {
                builder.WriteCompressed(adjustableProperty.Value);
            }

            foreach (var doorRatio in doorRatios)
            {
                if (doorRatio == 0 || doorRatio == 1)
                {
                    builder.Write(false);
                    builder.Write(doorRatio == 1);
                } else
                {
                    builder.Write(true);
                    builder.WriteFloatFromBits(doorRatio, 10, 0, 1, true);
                }
            }

            builder.Write((byte)upgrades.Length);
            foreach (var upgrade in upgrades)
            {
                builder.Write(upgrade);
            }

            builder.WriteStringWithoutLength(plateText.PadRight(8).Substring(0, 8));
            builder.WriteCapped(overrideLights, 2);

            builder.Write(isLandingGearDown);
            builder.Write(isSirenActive);
            builder.Write(isFuelTankExplodable);
            builder.Write(isEngineOn);
            builder.Write(isLocked);
            builder.Write(areDoorsUndamageable);
            builder.Write(isDamageProof);
            builder.Write(isFrozen);
            builder.Write(isDerailed);
            builder.Write(isDerailable);
            builder.Write(trainDirection);
            builder.Write(isTaxiLightOn);

            builder.WriteCompressed((byte)(255 - alpha));

            builder.Write(headlightColor != Color.White);
            if (headlightColor != Color.White)
            {
                builder.Write(headlightColor);
            }

            builder.Write(handling != null);
            if (handling != null)
            {
                WriteVehicleHandling(handling.Value);
            }

            WriteSirens(sirens);
        }

        private void WriteVehicleHandling(VehicleHandling handling)
        {
            builder.Write(handling.Mass);

            builder.Write(handling.TurnMass);
            builder.Write(handling.DragCoefficient);
            builder.Write(handling.CenterOfMass);
            builder.Write(handling.PercentSubmerged);

            builder.Write(handling.TractionMultiplier);

            builder.Write(handling.DriveType);
            builder.Write(handling.EngineType);
            builder.Write(handling.NumberOfGears);

            builder.Write(handling.EngineAcceleration);
            builder.Write(handling.EngineInertia);
            builder.Write(handling.MaxVelocity);

            builder.Write(handling.BrakeDeceleration);
            builder.Write(handling.BrakeBids);
            builder.Write(handling.Abs);

            builder.Write(handling.SteeringLock);
            builder.Write(handling.TractionLoss);
            builder.Write(handling.TractionBias);

            builder.Write(handling.SuspensionForceLevel);
            builder.Write(handling.SuspensionDampening);
            builder.Write(handling.SuspensionHighSpeedDampening);
            builder.Write(handling.SuspennsionUpperLimit);
            builder.Write(handling.SuspenionLowerLimit);
            builder.Write(handling.SuspensionFrontRearBias);
            builder.Write(handling.SuspensionAntiDiveMultiplier);

            builder.Write(handling.CollisionDamageMultiplier);

            builder.Write(handling.ModelFlags);
            builder.Write(handling.HandlingFlags);
            builder.Write(handling.SeatOffsetDistance);
            builder.Write(handling.AnimGroup);
        }

        private void WriteVehicleDamage(byte[] damageStates, int bitCap)
        {
            for (int i = 0; i < damageStates.Length; i++)
            {
                builder.WriteCapped(damageStates[i], bitCap);
            }
        }

        private void WriteSirens(VehicleSirenSet? sirenSet)
        {
            builder.Write(sirenSet != null);
            if (sirenSet != null)
            {
                builder.Write((byte)sirenSet.Value.Sirens.Length);
                builder.Write(sirenSet.Value.SirenType);

                foreach (var siren in sirenSet.Value.Sirens)
                {
                    builder.Write(siren.Id);
                    builder.Write(siren.Position);
                    builder.Write(siren.Color, true, true);
                    builder.Write(siren.SirenMinAlpha);
                    builder.Write(siren.Is360);
                    builder.Write(siren.UsesLineOfSightCheck);
                    builder.Write(siren.UsesRandomizer);
                    builder.Write(siren.IsSilent);
                }
            }
        }

        public void AddMarker(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3 position, byte markerType, float size,
            Color color, Vector3? targetPosition
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteVector3WithZAsFloat(position);
            builder.WriteCapped(markerType, 3);
            builder.Write(size);
            builder.Write(color, true);

            if (markerType == 0 || markerType == 1)
            {
                builder.Write(targetPosition != null);
                if (targetPosition != null)
                {
                    builder.WriteVector3WithZAsFloat(targetPosition.Value);
                }
            }
        }

        public void AddBlip(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3 position, short ordering, ushort visibleDistance,
            byte icon, byte size, Color color
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteVector3WithZAsFloat(position);
            builder.WriteCompressed(ordering);
            builder.WriteCapped(visibleDistance, 14);
            builder.WriteCapped(icon, 6);
            if (icon == 0)
            {
                builder.WriteCapped(size, 5);
                builder.Write(color, true);
            }
        }

        public void AddRadarArea(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector2 position, Vector2 size, Color color,
            bool isFlashing
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteVector2(position);
            builder.WriteVector2(size);
            builder.Write(color, true);
            builder.Write(isFlashing);
        }

        public void AddTeam(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, string teamName, Color color, bool isFriendlyFireEnabled,
            uint[] playerIds
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteCompressed((ushort)teamName.Length);
            builder.WriteStringWithoutLength(teamName);
            builder.Write(color);
            builder.Write(isFriendlyFireEnabled);
            builder.Write((uint)playerIds.Length);
            foreach (var playerId in playerIds)
            {
                builder.WriteElementId(playerId);
            }
        }

        public void AddPed(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3 position, ushort model, float rotation,
            float health, float armor, uint? vehicleId, byte? seat,
            bool hasJetpack, bool isSyncable, bool isHeadless, bool isFrozen,
            byte alpha, byte moveAnimation, PedClothing[] clothes,
            PedWeapon[] weapons, byte currentSlot
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteVector3WithZAsFloat(position);
            builder.WriteCompressed(model);
            builder.WriteFloatFromBits(rotation, 16, -MathF.PI, MathF.PI, false);
            builder.WritePlayerHealth(health);
            builder.WritePlayerArmor(armor);

            builder.Write(vehicleId != null && seat != null);
            if (vehicleId != null && seat != null)
            {
                builder.WriteElementId(vehicleId.Value);
                builder.WriteCapped(seat.Value, 4);
            }

            builder.Write(hasJetpack);
            builder.Write(isSyncable);
            builder.Write(isHeadless);
            builder.Write(isFrozen);

            builder.WriteCompressed((byte)(255 - alpha));
            builder.Write(moveAnimation);

            builder.Write((byte)clothes.Length);
            foreach (var clothingItem in clothes)
            {
                builder.WriteStringWithByteAsLength(clothingItem.Texture);
                builder.WriteStringWithByteAsLength(clothingItem.Model);
                builder.Write(clothingItem.Type);
            }

            foreach (var weapon in weapons)
            {
                builder.Write(weapon.Slot);
                builder.Write(weapon.Type);
                builder.Write(weapon.Ammo);
            }

            builder.Write((byte)0xFF);
            builder.Write(currentSlot);
        }

        public void AddDummy(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, string type, Vector3 position
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteCompressed((ushort)type.Length);
            builder.WriteStringWithoutLength(type);

            builder.Write(position != Vector3.Zero);
            if (position != Vector3.Zero)
            {
                builder.WriteVector3WithZAsFloat(position);
            }
        }

        public void AddColshape(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, byte colShapeType, Vector3 position,
            bool isEnabled, bool autoCallEvent
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.WriteCapped(colShapeType, 3);
            builder.WriteVector3WithZAsFloat(position);
            builder.Write(isEnabled);
            builder.Write(autoCallEvent);
        }

        public void AddColCircle(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, byte colShapeType,  Vector3 position, bool isEnabled,
            bool autoCallEvent, float radius
        )
        {
            AddColshape(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext,
                colShapeType, position, isEnabled, autoCallEvent
            );
            builder.Write(radius);
        }

        public void AddColSphere(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, byte colShapeType, Vector3 position, bool isEnabled,
            bool autoCallEvent, float radius
        )
        {
            AddColshape(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext,
                colShapeType, position, isEnabled, autoCallEvent
            );
            builder.Write(radius);
        }

        public void AddColCuboid(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, byte colShapeType, Vector3 position, bool isEnabled,
            bool autoCallEvent, Vector3 size
        )
        {
            AddColshape(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext,
                colShapeType, position, isEnabled, autoCallEvent
            );

            builder.WriteVector3WithZAsFloat(size);
        }

        public void AddColRectangle(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, byte colShapeType, Vector3 position, bool isEnabled,
            bool autoCallEvent, Vector2 size
        )
        {
            AddColshape(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext,
                colShapeType, position, isEnabled, autoCallEvent
            );

            builder.WriteVector2(size);
        }

        public void AddColTube(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, byte colShapeType, Vector3 position, bool isEnabled,
            bool autoCallEvent, float radius, float height
        )
        {
            AddColshape(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext,
                colShapeType, position, isEnabled, autoCallEvent
            );

            builder.Write(radius);
            builder.Write(height);
        }

        public void AddColPolygon(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, byte colShapeType, Vector3 position, bool isEnabled,
            bool autoCallEvent, Vector2[] vertices
        )
        {
            AddColshape(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext,
                colShapeType, position, isEnabled, autoCallEvent
            );

            builder.WriteCompressed((uint)vertices.Length);
            foreach (Vector2 vertex in vertices)
            {
                builder.WriteVector2(vertex);
            }
        }



        public void AddWater(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3[] vertices, bool isShallow
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );

            builder.Write((byte)vertices.Length);
            foreach (var vertex in vertices)
            {
                builder.Write((short)vertex.X);
                builder.Write((short)vertex.Y);
                builder.Write(vertex.Z);
            }
            builder.Write(isShallow);
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var finalBuilder = new PacketBuilder();
            finalBuilder.WriteCompressed(entityCount);

            finalBuilder.Write(builder.Build());

            return finalBuilder.Build();
        }
    }
}
