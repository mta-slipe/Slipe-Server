using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Join
{
    public class PlayerListPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_LIST;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;


        private readonly PacketBuilder builder;

        public PlayerListPacket(bool showInChat)
        {
            this.builder = new PacketBuilder();

            this.builder.Write(showInChat);
        }

        public void AddPlayer(
            uint playerId,
            byte timeContext,
            string nickname,
            ushort bitsreamVersion,
            int buildNumber,

            bool isDead,
            bool isInVehicle,
            bool hasJetpack,
            bool isNametagShowing,
            bool isNametagColorOverriden,
            bool isHeadless,
            bool isFrozen,

            string nametagText,
            Color? color,
            byte moveAnimation,

            ushort model,
            uint? teamId,

            uint? vehicleId,
            byte? seat,

            Vector3? position,
            float? rotation,

            ushort dimension,
            byte fightingStyle,
            byte alpha,
            byte interior,

            byte[] weapons
        )
        {
            builder.WriteElementId(playerId);
            builder.Write(timeContext);
            builder.WriteStringWithByteAsLength(nickname);

            builder.Write(bitsreamVersion);
            builder.Write(buildNumber);

            builder.Write(isDead);
            builder.Write(true);
            builder.Write(isInVehicle);
            builder.Write(hasJetpack);
            builder.Write(isNametagShowing);
            builder.Write(isNametagColorOverriden);
            builder.Write(isHeadless);
            builder.Write(isFrozen);

            builder.WriteStringWithByteAsLength(nametagText);
            if (isNametagColorOverriden)
            {
                if (color == null)
                {
                    throw new Exception($"Can not write player list packet. {nameof(isNametagColorOverriden)} is true, but required data is null");
                }
                else
                {
                    builder.Write(color.Value);
                }
            }

            builder.Write(moveAnimation);
            builder.WriteCompressed(model);
            if (teamId != null)
            {
                builder.Write(true);
                builder.WriteElementId(teamId.Value);
            } else
            {
                builder.Write(false);
            }

            if (isInVehicle)
            {
                if (vehicleId == null || seat == null)
                {
                    throw new Exception($"Can not write player list packet. {nameof(isInVehicle)} is true, but required data is null");
                }
                builder.WriteElementId(vehicleId.Value);
                builder.WriteCapped(seat.Value, 4);
            } else
            {
                if (position == null || rotation == null)
                {
                    throw new Exception($"Can not write player list packet. {nameof(isInVehicle)} is false, but required data is null");
                }
                builder.WriteVector3WithZAsFloat(position.Value);
                builder.WriteFloatFromBits(rotation.Value, 16, -MathF.PI, MathF.PI, false);
            }

            builder.WriteCompressed(dimension);
            builder.Write(fightingStyle);
            builder.WriteCompressed((byte)(255 - alpha));
            builder.Write(interior);

            for (int i = 0; i < 16; ++i)
            {
                if (weapons[i] == 0)
                {
                    builder.Write(false);
                } else
                {
                    builder.Write(true);
                    builder.Write(weapons[i]);
                }
            }
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            return builder.Build();
        }
    }
}
