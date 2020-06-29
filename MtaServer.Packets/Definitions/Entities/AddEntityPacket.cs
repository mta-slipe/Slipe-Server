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
                // TODO: Write position rotation animation
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

        public void AddWeapon(
            uint elementId, byte elementType, uint? parentId, byte interior,
            ushort dimension, ElementAttachment? attachment, bool areCollisionsEnabled,
            bool isCallPropagationEnabled, CustomData customData, string name,
            byte timeContext, Vector3 position, Vector3 rotation, ushort model,
            byte alpha, bool isLowLod, uint? lowLodElementId, bool isDoubleSided,
            bool isVisibleInAllDimensions, PositionRotationAnimation? positionRotationAnimation,
            Vector3 scale, bool isFrozen, float health
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
                builder.WriteFloatFromBits(armor.Value, 8, 0, 127.5f, true);
            }
            if (health != null)
            {
                builder.WriteFloatFromBits(health.Value, 8, 0, 255, true);
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
            byte timeContext
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );
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
            builder.WriteFloatFromBits(health, 8, 0, 255, true, false);
            builder.WriteFloatFromBits(armor, 8, 0, 127.5f, true, false);

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
            bool autoCallEvent
        )
        {
            AddColshape(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext,
                colShapeType, position, isEnabled, autoCallEvent
            );
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
