using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;

public sealed class AddEntityPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_ENTITY_ADD;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    private readonly PacketBuilder builder;
    private uint entityCount;

    public AddEntityPacket()
    {
        this.builder = new PacketBuilder();
        this.entityCount = 0;
    }

    public void AddEntity(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
        ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
        bool isCallPropagationEnabled, CustomData customData, string name,
        byte timeContext
    )
    {
        this.entityCount++;

        this.builder.Write(elementId);
        this.builder.Write(elementType);
        this.builder.Write(parentId ?? (ElementId)PacketConstants.InvalidElementId);
        this.builder.Write(interior);
        this.builder.WriteCompressed(dimension);

        this.builder.Write(attachment != null);
        if (attachment != null)
        {
            this.builder.Write(attachment.Value.ElementId);
            this.builder.WriteVector3WithZAsFloat(attachment.Value.AttachmentPosition);
            this.builder.WriteVectorAsUshorts(attachment.Value.AttachmentRotation);
        }

        this.builder.Write(areCollisionsEnabled);
        this.builder.Write(isCallPropagationEnabled);

        this.builder.WriteCompressed((ushort)customData.Items.Count());
        foreach (var item in customData.Items)
        {
            this.builder.WriteStringWithByteAsLength(item.Name);
            this.builder.Write(item.Data);
        }

        this.builder.WriteCompressed((ushort)name.Length);
        this.builder.WriteStringWithoutLength(name);
        this.builder.Write(timeContext);
    }

    public void AddObject(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
        ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
        bool isCallPropagationEnabled, CustomData customData, string name,
        byte timeContext, Vector3 position, Vector3 rotation, ushort model,
        byte alpha, bool isLowLod, ElementId? lowLodElementId, bool isDoubleSided, bool isBreakable,
        bool isVisibleInAllDimensions, PositionRotationAnimation? positionRotationAnimation,
        Vector3 scale, bool isFrozen, float health, bool isBroken, bool isRespawnable
    )
    {
        AddEntity(
            elementId, elementType, parentId, interior,
            dimension, attachment, areCollisionsEnabled,
            isCallPropagationEnabled, customData, name, timeContext
        );

        this.builder.WriteVector3WithZAsFloat(position);
        this.builder.WriteVectorAsUshorts(rotation);
        this.builder.WriteCompressed(model);
        this.builder.WriteCompressed((byte)(255 - alpha));
        this.builder.Write(isLowLod);
        this.builder.Write(lowLodElementId ?? (ElementId)PacketConstants.InvalidElementId);
        this.builder.Write(isDoubleSided);
        this.builder.Write(isBreakable);
        this.builder.Write(isVisibleInAllDimensions);

        this.builder.Write(positionRotationAnimation != null);
        if (positionRotationAnimation != null)
        {
            this.builder.Write(positionRotationAnimation);
        }

        if (scale.X == scale.Y && scale.Y == scale.Z)
        {
            this.builder.Write(true);
            this.builder.Write(scale.X == 1);
            if (scale.X != 1)
            {
                this.builder.Write(scale.X);
            }
        } else
        {
            this.builder.Write(false);
            this.builder.Write(scale);
        }

        this.builder.Write(isFrozen);
        this.builder.WriteFloatFromBits(health, 11, 0, 1023.5f, true);

        this.builder.Write(isBroken);
        this.builder.Write(isRespawnable);
    }

    private void WritePositionRotationAnimation(PositionRotationAnimation positionRotationAnimation, bool resumeMode = true)
    {
        this.builder.Write(resumeMode);

        if (resumeMode)
        {
            var now = DateTime.UtcNow;
            ulong elapsedTime = (ulong)(now - positionRotationAnimation.StartTime).Ticks / 10000;
            ulong timeRemaining = 0;
            if (positionRotationAnimation.EndTime > now)
            {
                timeRemaining = (ulong)(positionRotationAnimation.EndTime - now).Ticks / 10000;
            }
            this.builder.WriteCompressed(elapsedTime);
            this.builder.WriteCompressed(timeRemaining);
        } else
        {
            var duration = (positionRotationAnimation.EndTime - positionRotationAnimation.StartTime).Ticks / 10000;
            this.builder.WriteCompressed((ulong)duration);
        }

        this.builder.WriteVector3WithZAsFloat(positionRotationAnimation.SourcePosition);
        this.builder.Write(positionRotationAnimation.SourceRotation * (MathF.PI / 180));

        this.builder.WriteVector3WithZAsFloat(positionRotationAnimation.TargetPosition);
        this.builder.Write(positionRotationAnimation.DeltaRotationMode);
        if (positionRotationAnimation.DeltaRotationMode)
        {
            this.builder.Write(positionRotationAnimation.DeltaRotation * (MathF.PI / 180));
        } else
        {
            this.builder.Write(positionRotationAnimation.TargetRotation * (MathF.PI / 180));
        }

        this.builder.Write(positionRotationAnimation.EasingType);
        this.builder.Write(positionRotationAnimation.EasingPeriod);
        this.builder.Write(positionRotationAnimation.EasingAmplitude);
        this.builder.Write(positionRotationAnimation.EasingOvershoot);
    }

    public void AddWeapon(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
        ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
        bool isCallPropagationEnabled, CustomData customData, string name,
        byte timeContext, Vector3 position, Vector3 rotation, ushort model,
        byte alpha, bool isLowLod, ElementId? lowLodElementId, bool isDoubleSided, bool isBreakable,
        bool isVisibleInAllDimensions, PositionRotationAnimation? positionRotationAnimation,
        Vector3 scale, bool isFrozen, float health, byte targetType, ElementId? targetElementId,
        byte? boneTarget, byte? wheelTarget, Vector3? targetPosition, bool isChanged, ushort? damagePerHit,
        float? accuracy, float? targetRange, float? weaponRange, bool disableWeaponModel,
        bool instantReload, bool shootIfTargetBlocked, bool shootIfTargetOutOfRange,
        bool checkBuildings, bool checkCarTires, bool checkDummies, bool checkObjects,
        bool checkPeds, bool checkVehicles, bool ignoreSomeObjectForCamera, bool seeThroughStuff,
        bool shootThroughStuff, byte weaponState, ushort ammo, ushort clipAmmo, ElementId ownerId, bool isBroken, bool isRespawnable
    )
    {
        AddObject(
            elementId, elementType, parentId, interior,
            dimension, attachment, areCollisionsEnabled,
            isCallPropagationEnabled, customData, name, timeContext,
            position, rotation, model, alpha, isLowLod,
            lowLodElementId, isDoubleSided, isBreakable, isVisibleInAllDimensions,
            positionRotationAnimation, scale, isFrozen, health, isBroken, isRespawnable
        );

        this.builder.WriteCapped(targetType, 3);
        if (targetType == 1 && targetElementId != null)
        {
            this.builder.Write(targetElementId.Value);
            if (boneTarget != null)
            {
                this.builder.Write(boneTarget.Value);
            }
            if (wheelTarget != null)
            {
                this.builder.Write(wheelTarget.Value);
            }

            if (boneTarget == null && wheelTarget == null)
                throw new Exception($"Can not write weapon with target type {targetType} and no wheel or bone target");

        } else if (targetType == 0)
        {
            if (targetPosition == null)
                throw new Exception($"Can not write weapon with target type {targetType} and no target position");
            this.builder.WriteCompressedVector3(targetPosition.Value);
        }

        if (isChanged &&
            damagePerHit != null &&
            accuracy != null &&
            targetRange != null &&
            weaponRange != null
        )
        {
            this.builder.Write(true);
            this.builder.WriteCapped(damagePerHit.Value, 12);
            this.builder.Write(accuracy.Value);
            this.builder.Write(targetRange.Value);
            this.builder.Write(weaponRange.Value);
        } else
        {
            this.builder.Write(false);
        }

        this.builder.Write(disableWeaponModel);
        this.builder.Write(instantReload);
        this.builder.Write(shootIfTargetBlocked);
        this.builder.Write(shootIfTargetOutOfRange);
        this.builder.Write(checkBuildings);
        this.builder.Write(checkCarTires);
        this.builder.Write(checkDummies);
        this.builder.Write(checkObjects);
        this.builder.Write(checkPeds);
        this.builder.Write(checkVehicles);
        this.builder.Write(ignoreSomeObjectForCamera);
        this.builder.Write(seeThroughStuff);
        this.builder.Write(shootThroughStuff);

        this.builder.WriteCapped(weaponState, 4);
        this.builder.Write(ammo);
        this.builder.Write(clipAmmo);
        this.builder.Write(ownerId);
    }

    public void AddPickup(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.WriteVector3WithZAsFloat(position);
        this.builder.WriteCompressed(model);
        this.builder.Write(isVisible);
        this.builder.WriteCapped(pickupType, 3);

        if (armor != null)
        {
            this.builder.WritePlayerArmor(armor.Value);
        }
        if (health != null)
        {
            this.builder.WritePlayerHealth(health.Value);
        }
        if (weaponType != null && ammo != null)
        {
            this.builder.WriteCapped(weaponType.Value, 6);
            this.builder.WriteCompressed(ammo.Value);
        }
    }

    public void AddVehicle(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
        ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
        bool isCallPropagationEnabled, CustomData customData, string name,
        byte timeContext, Vector3 position, Vector3 rotation, ushort model,
        float health, byte blownState, Color[] colors, byte paintJob, VehicleDamage damage,
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

        this.builder.WriteVector3WithZAsFloat(position);
        this.builder.WriteVectorAsUshorts(rotation);
        this.builder.Write((byte)(model - 400));
        this.builder.WriteVehicleHealth(health);

        this.builder.WriteCapped(blownState, 2);

        this.builder.WriteCapped((byte)colors.Length - 1, 2);
        foreach (var color in colors)
        {
            this.builder.Write(color);
        }
        this.builder.WriteCapped(paintJob, 2);

        WriteVehicleDamage(damage.Doors, 3);
        WriteVehicleDamage(damage.Wheels, 2);
        WriteVehicleDamage(damage.Panels, 2);
        WriteVehicleDamage(damage.Lights, 2);


        this.builder.Write(variant1);
        this.builder.Write(variant2);

        if (turret != null)
        {
            this.builder.Write((short)(turret.Value.X * (32767.0f / MathF.PI)));
            this.builder.Write((short)(turret.Value.Y * (32767.0f / MathF.PI)));
        }

        if (adjustableProperty != null)
        {
            this.builder.WriteCompressed(adjustableProperty.Value);
        }

        foreach (var doorRatio in doorRatios)
        {
            if (doorRatio == 0 || doorRatio == 1)
            {
                this.builder.Write(false);
                this.builder.Write(doorRatio == 1);
            } else
            {
                this.builder.Write(true);
                this.builder.WriteFloatFromBits(doorRatio, 10, 0, 1, true);
            }
        }

        this.builder.Write((byte)upgrades.Length);
        foreach (var upgrade in upgrades)
        {
            this.builder.Write(upgrade);
        }

        this.builder.WriteStringWithoutLength(plateText.PadRight(8).Substring(0, 8), 8);
        this.builder.WriteCapped(overrideLights, 2);

        this.builder.Write(isLandingGearDown);
        this.builder.Write(isSirenActive);
        this.builder.Write(isFuelTankExplodable);
        this.builder.Write(isEngineOn);
        this.builder.Write(isLocked);
        this.builder.Write(areDoorsUndamageable);
        this.builder.Write(isDamageProof);
        this.builder.Write(isFrozen);
        this.builder.Write(isDerailed);
        this.builder.Write(isDerailable);
        this.builder.Write(trainDirection);
        this.builder.Write(isTaxiLightOn);

        this.builder.WriteCompressed((byte)(255 - alpha));

        this.builder.Write(headlightColor != Color.White);
        if (headlightColor != Color.White)
            this.builder.Write(headlightColor);

        this.builder.Write(handling != null);
        if (handling != null)
            WriteVehicleHandling(handling.Value);

        WriteSirens(sirens);
    }

    private void WriteVehicleHandling(VehicleHandling handling)
    {
        this.builder.Write(handling.Mass);

        this.builder.Write(handling.TurnMass);
        this.builder.Write(handling.DragCoefficient);
        this.builder.Write(handling.CenterOfMass);
        this.builder.Write(handling.PercentSubmerged);

        this.builder.Write(handling.TractionMultiplier);

        this.builder.Write((byte)handling.DriveType);
        this.builder.Write((byte)handling.EngineType);
        this.builder.Write(handling.NumberOfGears);

        this.builder.Write(handling.EngineAcceleration);
        this.builder.Write(handling.EngineInertia);
        this.builder.Write(handling.MaxVelocity);

        this.builder.Write(handling.BrakeDeceleration);
        this.builder.Write(handling.BrakeBias);
        this.builder.Write(handling.Abs);

        this.builder.Write(handling.SteeringLock);
        this.builder.Write(handling.TractionLoss);
        this.builder.Write(handling.TractionBias);

        this.builder.Write(handling.SuspensionForceLevel);
        this.builder.Write(handling.SuspensionDampening);
        this.builder.Write(handling.SuspensionHighSpeedDampening);
        this.builder.Write(handling.SuspensionUpperLimit);
        this.builder.Write(handling.SuspensionLowerLimit);
        this.builder.Write(handling.SuspensionFrontRearBias);
        this.builder.Write(handling.SuspensionAntiDiveMultiplier);

        this.builder.Write(handling.CollisionDamageMultiplier);

        this.builder.Write(handling.ModelFlags);
        this.builder.Write(handling.HandlingFlags);
        this.builder.Write(handling.SeatOffsetDistance);
        this.builder.Write(handling.AnimGroup);
    }

    private void WriteVehicleDamage(byte[] damageStates, int bitCap)
    {
        for (int i = 0; i < damageStates.Length; i++)
        {
            this.builder.WriteCapped(damageStates[i], bitCap);
        }
    }

    private void WriteSirens(VehicleSirenSet? sirenSet)
    {
        this.builder.Write(sirenSet != null);
        if (sirenSet != null)
        {
            this.builder.Write(sirenSet.Value.Count);
            this.builder.Write((byte)sirenSet.Value.SirenType);

            foreach (var siren in sirenSet.Value.Sirens)
            {
                this.builder.Write(true);
                this.builder.Write(siren.Id);
                this.builder.Write(siren.Position);
                this.builder.Write(siren.Color, true, true);
                this.builder.Write(siren.SirenMinAlpha);
                this.builder.Write(siren.Is360);
                this.builder.Write(siren.UsesLineOfSightCheck);
                this.builder.Write(siren.UsesRandomizer);
                this.builder.Write(siren.IsSilent);
            }
        }
    }

    public void AddMarker(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
        ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
        bool isCallPropagationEnabled, CustomData customData, string name,
        byte timeContext, Vector3 position, byte markerType, float size,
        Color color, Vector3? targetPosition, Color? targetArrowColor, float? targetArrowSize,
        bool ignoreAlphaLimits
    )
    {
        AddEntity(
            elementId, elementType, parentId, interior,
            dimension, attachment, areCollisionsEnabled,
            isCallPropagationEnabled, customData, name, timeContext
        );

        this.builder.WriteVector3WithZAsFloat(position);
        this.builder.WriteCapped(markerType, 3);
        this.builder.Write(size);
        this.builder.Write(color, true);

        if (markerType == 0 || markerType == 1)
        {
            this.builder.Write(targetPosition != null);
            if (targetPosition != null)
            {
                this.builder.WriteVector3WithZAsFloat(targetPosition.Value);
            }

            if (markerType == 0)
            {
                this.builder.Write(targetArrowColor ?? Color.White, true);
                this.builder.Write(targetArrowSize ?? 0.625f);
            }
        }
        
        this.builder.Write(ignoreAlphaLimits);
    }

    public void AddBlip(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.WriteVector3WithZAsFloat(position);
        this.builder.WriteCompressed(ordering);
        this.builder.WriteCapped(visibleDistance, 14);
        this.builder.WriteCapped(icon, 6);
        this.builder.WriteCapped(size, 5);
        this.builder.Write(color, true);
    }

    public void AddRadarArea(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.WriteVector2(position);
        this.builder.WriteVector2(size);
        this.builder.Write(color, true);
        this.builder.Write(isFlashing);
    }

    public void AddTeam(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
        ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
        bool isCallPropagationEnabled, CustomData customData, string name,
        byte timeContext, string teamName, Color color, bool isFriendlyFireEnabled,
        ElementId[] playerIds
    )
    {
        AddEntity(
            elementId, elementType, parentId, interior,
            dimension, attachment, areCollisionsEnabled,
            isCallPropagationEnabled, customData, name, timeContext
        );

        this.builder.WriteCompressed((ushort)teamName.Length);
        this.builder.WriteStringWithoutLength(teamName);
        this.builder.Write(color);
        this.builder.Write(isFriendlyFireEnabled);
        this.builder.Write((uint)playerIds.Length);
        foreach (var playerId in playerIds)
        {
            this.builder.Write(playerId);
        }
    }

    public void AddPed(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
        ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
        bool isCallPropagationEnabled, CustomData customData, string name,
        byte timeContext, Vector3 position, ushort model, float rotation,
        float health, float armor, ElementId? vehicleId, byte? seat,
        bool hasJetpack, bool isSyncable, bool isHeadless, bool isFrozen,
        byte alpha, byte moveAnimation, PedClothing[] clothes,
        PedWeapon[] weapons, byte currentSlot, PedAnimationData? animation
    )
    {
        AddEntity(
            elementId, elementType, parentId, interior,
            dimension, attachment, areCollisionsEnabled,
            isCallPropagationEnabled, customData, name, timeContext
        );

        this.builder.WriteVector3WithZAsFloat(position);
        this.builder.WriteCompressed(model);
        this.builder.WriteFloatFromBits(rotation, 16, -MathF.PI, MathF.PI, false);
        this.builder.WritePlayerHealth(health);
        this.builder.WritePlayerArmor(armor);

        this.builder.Write(vehicleId != null && seat != null);
        if (vehicleId != null && seat != null)
        {
            this.builder.Write(vehicleId.Value);
            this.builder.WriteCapped(seat.Value, 4);
        }

        this.builder.Write(hasJetpack);
        this.builder.Write(isSyncable);
        this.builder.Write(isHeadless);
        this.builder.Write(isFrozen);

        this.builder.WriteCompressed((byte)(255 - alpha));
        this.builder.Write(moveAnimation);

        this.builder.Write((byte)clothes.Length);
        foreach (var clothingItem in clothes)
        {
            this.builder.WriteStringWithByteAsLength(clothingItem.Texture);
            this.builder.WriteStringWithByteAsLength(clothingItem.Model);
            this.builder.Write(clothingItem.Type);
        }

        foreach (var weapon in weapons)
        {
            this.builder.Write(weapon.Slot);
            this.builder.Write(weapon.Type);
            this.builder.Write(weapon.Ammo);
        }

        this.builder.Write((byte)0xFF);
        this.builder.Write(currentSlot);

        this.builder.Write(animation != null);
        if (animation != null)
        {
            this.builder.Write(animation.BlockName);
            this.builder.Write(animation.AnimationName);
            this.builder.Write(animation.Time);
            this.builder.Write(animation.IsLooped);
            this.builder.Write(animation.UpdatesPosition);
            this.builder.Write(animation.IsInterruptable);
            this.builder.Write(animation.FreezesLastFrame);
            this.builder.Write(animation.BlendTime);
            this.builder.Write(animation.RestoresTask);
            this.builder.Write(animation.ElapsedTime);
            this.builder.Write(animation.Speed);
        }
    }

    public record PedAnimationData(string BlockName, string AnimationName, int Time, bool IsLooped, bool UpdatesPosition, bool IsInterruptable, bool FreezesLastFrame, int BlendTime, bool RestoresTask, float ElapsedTime, float Speed);

    public void AddDummy(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.WriteCompressed((ushort)type.Length);
        this.builder.WriteStringWithoutLength(type);

        this.builder.Write(position != Vector3.Zero);
        if (position != Vector3.Zero)
        {
            this.builder.WriteVector3WithZAsFloat(position);
        }
    }

    public void AddColshape(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.WriteCapped(colShapeType, 3);
        this.builder.WriteVector3WithZAsFloat(position);
        this.builder.Write(isEnabled);
        this.builder.Write(autoCallEvent);
    }

    public void AddColCircle(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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
        this.builder.Write(radius);
    }

    public void AddColSphere(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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
        this.builder.Write(radius);
    }

    public void AddColCuboid(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.WriteVector3WithZAsFloat(size);
    }

    public void AddColRectangle(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.WriteVector2(size);
    }

    public void AddColTube(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.Write(radius);
        this.builder.Write(height);
    }

    public void AddColPolygon(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
        ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
        bool isCallPropagationEnabled, CustomData customData, string name,
        byte timeContext, byte colShapeType, Vector3 position, bool isEnabled,
        bool autoCallEvent, Vector2[] vertices, Vector2 height
    )
    {
        AddColshape(
            elementId, elementType, parentId, interior,
            dimension, attachment, areCollisionsEnabled,
            isCallPropagationEnabled, customData, name, timeContext,
            colShapeType, position, isEnabled, autoCallEvent
        );

        this.builder.WriteCompressed((uint)vertices.Length);
        foreach (Vector2 vertex in vertices)
        {
            this.builder.WriteVector2(vertex);
        }

        this.builder.Write(height);
    }



    public void AddWater(
        ElementId elementId, byte elementType, ElementId? parentId, byte interior,
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

        this.builder.Write((byte)vertices.Length);
        foreach (var vertex in vertices)
        {
            this.builder.Write((short)vertex.X);
            this.builder.Write((short)vertex.Y);
            this.builder.Write(vertex.Z);
        }
        this.builder.Write(isShallow);
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var finalBuilder = new PacketBuilder();
        finalBuilder.WriteCompressed(this.entityCount);

        finalBuilder.Write(this.builder.Build());

        return finalBuilder.Build();
    }
}
