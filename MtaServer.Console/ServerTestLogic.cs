using Force.Crc32;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Packets.Definitions.Entities.Structs;
using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;
using MtaServer.Packets.Definitions.Player;
using MtaServer.Packets.Definitions.Resources;
using MtaServer.Packets.Definitions.Sync;
using MtaServer.Packets.Lua.Camera;
using MtaServer.Server;
using MtaServer.Server.Elements;
using MtaServer.Server.Elements.Enums;
using MtaServer.Server.PacketHandling.Factories;
using MtaServer.Server.Repositories;
using MtaServer.Server.ResourceServing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static MtaServer.Server.PacketHandling.Factories.PlayerPacketFactory;

namespace MtaServer.Console
{
    public class ServerTestLogic
    {
        private readonly IElementRepository elementRepository;
        private readonly RootElement root;
        private readonly IResourceServer resourceServer;

        public ServerTestLogic(IElementRepository elementRepository, RootElement root, IResourceServer resourceServer)
        {
            this.elementRepository = elementRepository;
            this.root = root;
            this.resourceServer = resourceServer;
            this.SetupTestLogic();
        }

        private void SetupTestLogic()
        {
            Player.OnJoin += (player) =>
            {
                var client = player.Client;
                System.Console.WriteLine($"{player.Name} ({client.Version}) ({client.Serial}) has joined the server!");
                client.SendPacket(new SetCameraTargetPacket(player.Id));
                client.SendPacket(new SpawnPlayerPacket(
                    player.Id,
                    flags: 0,
                    position: new Vector3(0, 0, 3),
                    rotation: 0,
                    skin: 7,
                    teamId: 0,
                    interior: 0,
                    dimension: 0,
                    timeContext: 0
                ));
                client.SendPacket(new FadeCameraPacket(CameraFade.In));
                client.SendPacket(new ChatEchoPacket(this.root.Id, "Hello World", Color.White));
                client.SendPacket(new ClearChatPacket());
                client.SendPacket(new ChatEchoPacket(this.root.Id, "Hello World Again", Color.White));
                client.SendPacket(new ConsoleEchoPacket("Hello Console World"));
                client.SendPacket(CreateShowHudComponentPacket(HudComponent.Money, false));
                client.SendPacket(CreateShowHudComponentPacket(HudComponent.Health, false));

                TestClientResource(client);
                TestPureSync(client);
                SetupTestEntities(client);
            };
        }

        private void TestClientResource(Client client)
        {
            var resourceRoot = new DummyElement()
            {
                Parent = this.root
            };
            var resourceDyanmic = new DummyElement()
            {
                Parent = resourceRoot
            };

            var entityPacket = AddEntityPacketFactory.CreateAddEntityPacket(new Element[] { resourceRoot, resourceDyanmic });
            client.SendPacket(entityPacket);

            var testResourceFiles = this.resourceServer.GetResourceFiles("./TestResource");
            client.SendPacket(new ResourceStartPacket(
                "TestResource", 0, resourceRoot.Id, resourceDyanmic.Id, 0, null, null, false, 0, testResourceFiles, new string[0])
            );
        }

        private void TestPureSync(Client client)
        {
            var playerList = new PlayerListPacket(false);
            playerList.AddPlayer(
                playerId: 666,
                timeContext: 0,
                nickname: "Dummy-Player",
                bitsreamVersion: 343,
                buildNumber: 0,

                isDead: false,
                isInVehicle: false,
                hasJetpack: true,
                isNametagShowing: true,
                isNametagColorOverriden: true,
                isHeadless: false,
                isFrozen: false,

                nametagText: "Dummy-Player",
                color: Color.FromArgb(255, 255, 0, 255),
                moveAnimation: 0,

                model: 9,
                teamId: null,

                vehicleId: null,
                seat: null,

                position: new Vector3(5, 0, 3),
                rotation: 0,

                dimension: 0,
                fightingStyle: 0,
                alpha: 255,
                interior: 0,

                weapons: new byte[16]
            );
            client.SendPacket(playerList);

            var data = new byte[] { 0, 0, 0, 0, 2, 46, 33, 240, 8, 159, 255, 240, 8, 4, 116, 11, 186, 246, 64, 0, 73, 144, 129, 19, 48, 0, 0 };
            var puresync = new PlayerPureSyncPacket();
            puresync.Read(data);

            puresync.PlayerId = 666;
            puresync.Latency = 0;

            //_ = Task.Run(async () =>
            //{
            //    for (int i = 0; i < 1000; i++)
            //    {
            //        puresync.Position += new Vector3(0.25f, 0, 0);
            //        client.SendPacket(puresync);
            //        await Task.Delay(250);
            //    }
            //});
        }

        private void SetupTestEntities(Client client)
        {
            var entitypacket = new AddEntityPacket();
            entitypacket.AddWater(667, (byte)ElementType.Water, null, 0, 0,
                null, true, true, new CustomData(), "Test water",
                0, new Vector3[] {
                        new Vector3(-6, 0, 4), new Vector3(-3, 0, 4),
                        new Vector3(-6, 3, 4), new Vector3(-3, 3, 4)
                }, false);
            entitypacket.AddObject(
                668, (byte)ElementType.Object, null, 0, 0,
                null, true, false, new CustomData(), "Test object",
                0, new Vector3(0, -5, 3), Vector3.Zero, 321,
                255, false, null, true, true, null, Vector3.One * 3,
                false, 1000f
            );
            entitypacket.AddBlip(669, (byte)ElementType.Blip, null, 0, 0,
                null, true, true, new CustomData(), "Test blip",
                0, new Vector3(20, 0, 0), 0, 2500, 56, 1, Color.White);
            entitypacket.AddRadarArea(670, (byte)ElementType.RadarArea, null, 0, 0,
                null, true, true, new CustomData(), "Test radar area",
                0, new Vector2(0, 0), new Vector2(250, 250), Color.FromArgb(100, Color.DarkGoldenrod), true);
            entitypacket.AddMarker(671, (byte)ElementType.Marker, null, 0, 0,
                null, true, true, new CustomData(), "Test marker",
                0, new Vector3(5, 0, 2), (byte)MarkerType.Cylinder, 2, Color.FromArgb(100, Color.DarkCyan), null);
            entitypacket.AddPickup(672, (byte)ElementType.Pickup, null, 0, 0,
                null, true, true, new CustomData(), "Test pickup",
                0, new Vector3(0, 5, 3), 349, true, (byte)PickupType.Weapon, null, null, 25, 0);
            entitypacket.AddPed(673, (byte)ElementType.Ped, null, 0, 0,
                null, true, true, new CustomData(), "Test ped",
                0, new Vector3(10, 0, 3), 181, 45, 100, 50, null, null,
                true, true, true, false, 200, 0, new PedClothing[0], new PedWeapon[0], 0);
            entitypacket.AddWeapon(674, (byte)ElementType.Weapon, null, 0, 0,
                null, true, true, new CustomData(), "Test weapon",
                0, new Vector3(5, 5, 5), Vector3.Zero, 355, 255, false, null,
                true, true, null, Vector3.One, false, 100, (byte)WeaponTargetType.Vector,
                null, null, null, new Vector3(10, 10, 5), true, 10, 1, 100, 200,
                false, false, true, true, true, true, true, true, true, true,
                true, true, true, (byte)WeaponState.Ready, 1000, 50, 666);
            entitypacket.AddVehicle(675, (byte)ElementType.Vehicle, null, 0, 0,
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


            entitypacket.AddObject(
                676, (byte)ElementType.Object, null, 0, 0,
                null, true, false, new CustomData(), "Moving test object",
                0, new Vector3(0, -5, 3), Vector3.Zero, 321,
                255, false, null, true, true, new PositionRotationAnimation()
                {
                    SourcePosition = new Vector3(0, -5, 3),
                    SourceRotation = new Vector3(0, 0, 0),
                    TargetPosition = new Vector3(0, 45, 3),
                    TargetRotation = new Vector3(0, 0, 360),
                    DeltaRotationMode = true,
                    DeltaRotation = new Vector3(0, 0, 360),
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow + TimeSpan.FromSeconds(10),
                    EasingType = "Linear"
                }, Vector3.One * 3,
                false, 1000f
            );
            client.SendPacket(entitypacket);

            _ = Task.Run(async () =>
            {
                await Task.Delay(10000);
                var packet = new RemoveEntityPacket();
                packet.AddEntity(676);
                client.SendPacket(packet);
            });
        }
    }
}
