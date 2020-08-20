using MtaServer.Packets.Builder;
using MtaServer.Packets.Reader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Xml;

namespace MtaServer.Packets.Structures
{
    public class FullKeySyncStructure : ISyncStructure
    {
        public bool LeftShoulder1 { get; set; }
        public bool RightShoulder1 { get; set; }
        public bool ButtonSquare { get; set; }
        public bool ButtonCross { get; set; }
        public bool ButtonCircle { get; set; }
        public bool ButtonTriangle { get; set; }
        public bool ShockButton { get; set; }
        public bool PedWalk { get; set; }

        public byte ButtonSquareByte { get; set; }
        public byte ButtonCrossByte { get; set; }

        public Vector2 LeftStick { get; set; }


        public FullKeySyncStructure()
        {

        }

        public FullKeySyncStructure(
            Vector2 leftStick,
            bool leftShoulder1,
            bool leftShoulder2,
            bool buttonSquare,
            bool buttonCross,
            bool buttonCircle,
            bool buttonTriangle,
            bool shockButton,
            bool pedWalk
        )
        {
            LeftStick = leftStick;
            LeftShoulder1 = leftShoulder1;
            RightShoulder1 = leftShoulder2;
            ButtonSquare = buttonSquare;
            ButtonCross = buttonCross;
            ButtonCircle = buttonCircle;
            ButtonTriangle = buttonTriangle;
            ShockButton = shockButton;
            PedWalk = pedWalk;
        }

        public void Read(PacketReader reader)
        {
            LeftShoulder1 = reader.GetBit();
            RightShoulder1 = reader.GetBit();
            ButtonSquare = reader.GetBit();
            ButtonCross = reader.GetBit();
            ButtonCircle = reader.GetBit();
            ButtonTriangle = reader.GetBit();
            ShockButton = reader.GetBit();
            PedWalk = reader.GetBit();

            //this.ButtonSquareByte = reader.GetBit() ? reader.GetByte() : (byte)0;
            //this.ButtonCrossByte = reader.GetBit() ? reader.GetByte() : (byte)0;

            LeftStick = new Vector2(
                (float)(reader.GetByte() * 128.0f / 127.0f),
                (float)(reader.GetByte() * 128.0f / 127.0f)
            );

        }

        public void Write(PacketBuilder builder)
        {
            builder.Write(new bool[] {
                LeftShoulder1,
                RightShoulder1,
                ButtonSquare,
                ButtonCross,
                ButtonCircle,
                ButtonTriangle,
                ShockButton,
                PedWalk
            });
            builder.Write(new byte[]
            {
                (byte)((float)LeftStick.X * 127.0f / 128.0f),
                (byte)((float)LeftStick.Y * 127.0f / 128.0f),
            });
        }
    }
}
