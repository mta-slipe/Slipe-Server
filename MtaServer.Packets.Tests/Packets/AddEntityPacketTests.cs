using FluentAssertions;
using MtaServer.Packets.Definitions.Entities.Structs;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;
using MtaServer.Packets.Definitions.Sync;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Drawing;
using System.Numerics;
using Xunit;

namespace MtaServer.Packets.Tests.Packets
{
    public class AddEntityPacketTests
    {
        private readonly byte[] testPacket = new byte[]
        {
            249, 155, 2, 8, 255, 255, 192, 48, 120, 116, 168, 202, 230, 232, 64, 238, 194, 232, 202, 228,
            0, 9, 245, 254, 0, 0, 0, 1, 0, 129, 251, 254, 0, 0, 0, 1, 0, 129, 245, 254, 6, 0, 0, 1, 0, 129,
            251, 254, 6, 0, 0, 1, 0, 128, 156, 2, 1, 255, 255, 192, 48, 88, 118, 168, 202, 230, 232, 64, 222,
            196, 212, 202, 198, 232, 0, 0, 0, 0, 1, 217, 254, 0, 0, 128, 128, 0, 0, 0, 0, 0, 0, 65, 1, 131, 255,
            255, 160, 0, 4, 4, 6, 135, 157, 2, 2, 255, 255, 192, 48, 120, 114, 168, 202, 230, 232, 64, 196, 216,
            210, 224, 0, 0, 160, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 196, 39, 137, 224, 32, 63, 255, 252, 3, 7, 135,
            234, 140, 174, 110, 132, 14, 76, 44, 140, 46, 68, 12, 46, 76, 172, 32, 0, 0, 0, 0, 0, 0, 0, 29, 0,
            96, 29, 0, 119, 16, 193, 108, 153, 240, 32, 39, 255, 252, 3, 7, 135, 106, 140, 174, 110, 132, 13,
            172, 46, 77, 108, 174, 64, 0, 2, 128, 0, 0, 0, 0, 0, 0, 8, 8, 0, 0, 1, 0, 2, 46, 45, 146, 128, 8,
            13, 255, 255, 0, 193, 225, 218, 163, 43, 155, 161, 3, 131, 75, 27, 91, 171, 128, 0, 0, 0, 0, 0, 160,
            0, 0, 2, 2, 1, 116, 6, 153, 194, 132, 8, 29, 255, 255, 0, 193, 225, 194, 163, 43, 155, 161, 3, 131,
            43, 32, 0, 1, 64, 0, 0, 0, 0, 0, 2, 2, 5, 107, 255, 254, 200, 200, 225, 184, 0, 7, 248, 5, 16, 16,
            75, 255, 254, 1, 131, 195, 181, 70, 87, 55, 66, 7, 118, 86, 23, 6, 246, 224, 0, 1, 64, 0, 1, 64, 0,
            0, 10, 4, 0, 0, 0, 0, 0, 0, 3, 24, 12, 31, 255, 253, 178, 0, 0, 0, 112, 65, 84, 213, 84, 213, 170,
            170, 133, 0, 0, 4, 1, 248, 0, 6, 66, 16, 0, 2, 66, 25, 255, 195, 160, 12, 200, 2, 104, 9, 70, 4, 2,
            255, 255, 128, 96, 240, 241, 81, 149, 205, 208, 129, 217, 149, 161, 165, 141, 177, 148, 0, 3, 99, 252,
            0, 80, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 3, 43, 65, 223, 240, 0, 0, 0, 15, 240, 0, 0, 0, 0, 0, 0, 0, 2, 1,
            64, 40, 5, 0, 160, 20, 2, 0, 83, 76, 73, 80, 69, 32, 32, 32, 32, 4, 111, 147, 112, 219, 0
        };

        [Fact]
        public void WritePacket_MatchesExpectedByteArray()
        {
            var packet = new AddEntityPacket();
            packet.AddWater(667, (byte)17, null, 0, 0,
                null, true, true, new CustomData(), "Test water",
                0, new Vector3[] {
                        new Vector3(-6, 0, 4), new Vector3(-3, 0, 4),
                        new Vector3(-6, 3, 4), new Vector3(-3, 3, 4)
                }, false);
            packet.AddObject(
                668, (byte)3, null, 0, 0,
                null, true, false, new CustomData(), "Test object",
                0, new Vector3(0, -5, 3), Vector3.Zero, 321,
                255, false, null, true, true, null, Vector3.One * 3,
                false, 1000f
            );
            packet.AddBlip(669, (byte)5, null, 0, 0,
                null, true, true, new CustomData(), "Test blip",
                0, new Vector3(20, 0, 0), 0, 2500, 56, 1, Color.White);
            packet.AddRadarArea(670, (byte)7, null, 0, 0,
                null, true, true, new CustomData(), "Test radar area",
                0, new Vector2(0, 0), new Vector2(250, 250), Color.FromArgb(100, Color.DarkGoldenrod), true);
            packet.AddMarker(671, (byte)4, null, 0, 0,
                null, true, true, new CustomData(), "Test marker",
                0, new Vector3(5, 0, 2), (byte)2, 2, Color.FromArgb(100, Color.DarkCyan), null);
            packet.AddPickup(672, (byte)6, null, 0, 0,
                null, true, true, new CustomData(), "Test pickup",
                0, new Vector3(0, 5, 3), 349, true, (byte)2, null, null, 25, 0);
            packet.AddPed(673, (byte)14, null, 0, 0,
                null, true, true, new CustomData(), "Test ped",
                0, new Vector3(10, 0, 3), 181, 45, 100, 50, null, null,
                true, true, true, false, 200, 0, new PedClothing[0], new PedWeapon[0], 0);
            packet.AddWeapon(674, (byte)18, null, 0, 0,
                null, true, true, new CustomData(), "Test weapon",
                0, new Vector3(5, 5, 5), Vector3.Zero, 355, 255, false, null,
                true, true, null, Vector3.One, false, 100, (byte)0,
                null, null, null, new Vector3(10, 10, 5), true, 10, 1, 100, 200,
                false, false, true, true, true, true, true, true, true, true,
                true, true, true, (byte)0, 1000, 50, 666);
            packet.AddVehicle(675, (byte)2, null, 0, 0,
                null, true, true, new CustomData(), "Test vehicle",
                0, new Vector3(-10, 5, 3), Vector3.Zero, 602, 1000, new Color[] {
                        Color.Red, Color.Blue
                }, 0, new VehicleDamage()
                {
                    Doors = new byte[] { 0, 0, 0, 0, 0, 0 },
                    Wheels = new byte[] { 0, 0, 0, 0 },
                    Panels = new byte[] { 0, 0, 0, 0, 0, 0, 0 },
                    Lights = new byte[] { 0, 0, 0, 0 }
                }, 0, 0, null, null, new float[] {
                        0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f
                }, new byte[0], "SLIPE", 0, true, false, false, false, false,
                false, false, false, false, false, false, true, 200, Color.MediumPurple, null, null);

            var result = packet.Write();

            result.Should().Equal(this.testPacket);
        }
    }
}
