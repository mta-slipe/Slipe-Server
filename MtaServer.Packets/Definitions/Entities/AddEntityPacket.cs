using MtaServer.Packets.Definitions.Entities.Structs;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
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
            uint elementId, 
            byte elementType, 
            uint? parentId, 
            byte interior, 
            ushort dimension, 
            ElementAttachment? attachment,
            bool areCollisionsEnabled,
            bool isCallPropagationEnabled,
            CustomData customData,
            string name,
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
            uint elementId,
            byte elementType,
            uint? parentId,
            byte interior,
            ushort dimension,
            ElementAttachment? attachment,
            bool areCollisionsEnabled,
            bool isCallPropagationEnabled,
            CustomData customData,
            string name,
            byte timeContext,
            Vector3 position,
            Vector3 rotation,
            ushort model,
            byte alpha,
            bool isLowLod,
            uint? lowLodElementId,
            bool isDoubleSided,
            bool isVisibleInAllDimensions,
            PositionRotationAnimation? positionRotationAnimation,
            Vector3 scale,
            bool isFrozen,
            float health
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
            uint elementId,
            byte elementType,
            uint? parentId,
            byte interior,
            ushort dimension,
            ElementAttachment? attachment,
            bool areCollisionsEnabled,
            bool isCallPropagationEnabled,
            CustomData customData,
            string name,
            byte timeContext,
            Vector3 position,
            Vector3 rotation,
            ushort model,
            byte alpha,
            bool isLowLod,
            uint? lowLodElementId,
            bool isDoubleSided,
            bool isVisibleInAllDimensions,
            PositionRotationAnimation? positionRotationAnimation,
            Vector3 scale,
            bool isFrozen,
            float health
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

        public void AddVehicle(
            uint elementId,
            byte elementType,
            uint? parentId,
            byte interior,
            ushort dimension,
            ElementAttachment? attachment,
            bool areCollisionsEnabled,
            bool isCallPropagationEnabled,
            CustomData customData,
            string name,
            byte timeContext
        )
        {
            AddEntity(
                elementId, elementType, parentId, interior,
                dimension, attachment, areCollisionsEnabled,
                isCallPropagationEnabled, customData, name, timeContext
            );
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
