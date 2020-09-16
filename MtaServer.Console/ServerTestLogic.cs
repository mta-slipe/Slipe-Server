using MtaServer.Packets.Definitions.Commands;
using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Definitions.Lua;
using MtaServer.Packets.Definitions.Player;
using MtaServer.Packets.Definitions.Resources;
using MtaServer.Packets.Definitions.Sync;
using MtaServer.Packets.Lua.Camera;
using MtaServer.Packets.Lua.Event;
using MtaServer.Server;
using MtaServer.Server.Elements;
using MtaServer.Server.Elements.Enums;
using MtaServer.Server.PacketHandling;
using MtaServer.Server.PacketHandling.Factories;
using MtaServer.Server.Repositories;
using MtaServer.Server.ResourceServing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace MtaServer.Console
{
    public class ServerTestLogic
    {
        private readonly Server.MtaServer server;
        private readonly IElementRepository elementRepository;
        private readonly RootElement root;
        private readonly IResourceServer resourceServer;

        public ServerTestLogic(Server.MtaServer server, IElementRepository elementRepository, RootElement root, IResourceServer resourceServer)
        {
            this.server = server;
            this.elementRepository = elementRepository;
            this.root = root;
            this.resourceServer = resourceServer;
            this.SetupTestLogic();
        }

        private void SetupTestLogic()
        {
            this.server.PlayerJoined += (player) =>
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

                client.SendPacket(ElementPacketFactory.CreateSetHealthPacket(player, 50));
                client.SendPacket(ElementPacketFactory.CreateSetAlphaPacket(player, 100));

                client.SendPacket(PlayerPacketFactory.CreateShowHudComponentPacket(HudComponent.Money, false));
                client.SendPacket(PlayerPacketFactory.CreateSetFPSLimitPacket(100));
                client.SendPacket(PlayerPacketFactory.CreatePlaySoundPacket(1));
                client.SendPacket(PlayerPacketFactory.CreateSetWantedLevelPacket(4));
                client.SendPacket(PlayerPacketFactory.CreateToggleDebuggerPacket(true));
                client.SendPacket(PlayerPacketFactory.CreateDebugEchoPacket("Object reference not set to an instance of an object", 0, Color.Red));
                client.SendPacket(PlayerPacketFactory.CreateDebugEchoPacket("You successfully got banned", 3));
                //client.SendPacket(PlayerPacketFactory.CreateForcePlayerMapPacket(true)); 
                //client.SendPacket(PlayerPacketFactory.CreateToggleAllControlsPacket(false));

                TestPacketScopes(client);
                TestClientResource(client);
                TestPureSync(client);
                _ = TestEventTrigger(client);
                SetupTestElements(client);
            };
        }

        private void TestPacketScopes(Client client)
        {
            _ = Task.Run(async () =>
            {
                using (var scope = new ClientPacketScope(new Client[] { client }))
                {
                    await Task.Delay(500);
                    client.SendPacket(new ChatEchoPacket(this.root.Id, "After 500 #1", Color.White));
                    await Task.Delay(500);
                    client.SendPacket(new ChatEchoPacket(this.root.Id, "After 500 #2", Color.White));
                }
            });

            _ = Task.Run(async () =>
            {
                using (var scope = new ClientPacketScope(new Client[] { }))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(100);
                        client.SendPacket(new ChatEchoPacket(this.root.Id, $"After 100 #{i + 1}", Color.White));
                    }
                }
            });
        }

        private void TestClientResource(Client client)
        {
            var resourceRoot = new DummyElement()
            {
                Parent = this.root,
                ElementTypeName = "resource",
            }.AssociateWith(server);
            var resourceDyanmic = new DummyElement()
            {
                Parent = resourceRoot,
                ElementTypeName = "resource",
            }.AssociateWith(server);

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

        private void SetupTestElements(Client client)
        {
            var worldObject = new WorldObject(321, new Vector3(5, 0, 3)).AssociateWith(server);

            var entityPacket = AddEntityPacketFactory.CreateAddEntityPacket(new Element[]
            {
                new Water(new Vector3[]
                {
                        new Vector3(-6, 0, 4), new Vector3(-3, 0, 4),
                        new Vector3(-6, 3, 4), new Vector3(-3, 3, 4)
                }).AssociateWith(server),
                new WorldObject(321, new Vector3(5, 0, 3)).AssociateWith(server),
                new Blip(new Vector3(20, 0, 0), BlipIcon.Bulldozer).AssociateWith(server),
                new RadarArea(new Vector2(0, 0), new Vector2(200, 200), Color.FromArgb(100, Color.Aqua)).AssociateWith(server),
                new Marker(new Vector3(5, 0, 2), MarkerType.Cylinder){
                    Color = Color.FromArgb(100, Color.Cyan)
                }.AssociateWith(server),
                new Pickup(new Vector3(0, 5, 3), PickupType.Health, 20).AssociateWith(server),
                new Ped(7, new Vector3(10, 0, 3)).AssociateWith(server),
                new Weapon(355, new Vector3(10, 10, 5))
                {
                    TargetType = WeaponTargetType.Fixed,
                    TargetPosition = new Vector3(10, 10, 5)
                }.AssociateWith(server),
                new Vehicle(602, new Vector3(-10, 5, 3)).AssociateWith(server),
                worldObject
            });
            client.SendPacket(entityPacket);
        }

        private async Task TestEventTrigger(Client client)
        {
            var table = new LuaValue(new Dictionary<LuaValue, LuaValue>()
            {
                ["x"] = 5.5f,
                ["y"] = "string",
                ["z"] = new LuaValue(new Dictionary<LuaValue, LuaValue>() { }),
                ["w"] = false
            });
            table.TableValue?.Add("self", table);

            var packet = new LuaEventPacket("Slipe.Test.ClientEvent", root.Id, new LuaValue[]
            {
                "String value",
                true,
                123,
                table
            });

            await Task.Delay(5000);

            client.SendPacket(packet);
        }
    }
}
