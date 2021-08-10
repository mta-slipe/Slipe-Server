using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Repositories;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.ResourceServing;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SlipeServer.Console
{
    public class ServerTestLogic
    {
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;
        private readonly RootElement root;
        private readonly IResourceServer resourceServer;
        private readonly GameWorld worldService;
        private readonly DebugLog debugLog;
        private readonly ILogger logger;
        private readonly ChatBox chatBox;
        private readonly ClientConsole console;
        private readonly LuaEventService luaService;
        private readonly ExplosionService explosionService;
        private readonly FireService fireService;
        private Resource? testResource;

        public ServerTestLogic(
            MtaServer server,
            IElementRepository elementRepository,
            RootElement root,
            IResourceServer resourceServer,
            GameWorld world,
            DebugLog debugLog,
            ILogger logger,
            ChatBox chatBox,
            ClientConsole console,
            LuaEventService luaService,
            ExplosionService explosionService,
            FireService fireService
        )
        {
            this.server = server;
            this.elementRepository = elementRepository;
            this.root = root;
            this.resourceServer = resourceServer;
            this.worldService = world;
            this.debugLog = debugLog;
            this.logger = logger;
            this.chatBox = chatBox;
            this.console = console;
            this.luaService = luaService;
            this.explosionService = explosionService;
            this.fireService = fireService;
            this.SetupTestLogic();
        }

        private void SetupTestLogic()
        {
            SetupTestElements();

            this.luaService.AddEventHandler("Slipe.Test.Event", (e) => this.TriggerTestEvent(e.Player));

            this.worldService.SetWeather(Weather.ExtraSunnyDesert);
            this.worldService.CloudsEnabled = false;
            this.worldService.SetTime(13, 37);
            this.worldService.MinuteDuration = 60_000;
            this.worldService.SetSkyGradient(Color.Aqua, Color.Teal);

            this.server.PlayerJoined += OnPlayerJoin;
        }

        private void SetupTestElements()
        {
            this.testResource = new Resource(this.server, this.root, this.resourceServer, "TestResource");

            new WorldObject(321, new Vector3(5, 0, 3)).AssociateWith(server);
            new Water(new Vector3[]
            {
                new Vector3(-6, 0, 4), new Vector3(-3, 0, 4),
                new Vector3(-6, 3, 4), new Vector3(-3, 3, 4)
            }).AssociateWith(server);
            new WorldObject(321, new Vector3(5, 0, 3)).AssociateWith(server);
            new Blip(new Vector3(20, 0, 0), BlipIcon.Bulldozer).AssociateWith(server);
            new RadarArea(new Vector2(0, 0), new Vector2(200, 200), Color.FromArgb(100, Color.Aqua)).AssociateWith(server);
            new Marker(new Vector3(5, 0, 2), MarkerType.Cylinder)
            {
                Color = Color.FromArgb(100, Color.Cyan)
            }.AssociateWith(server);
            new Pickup(new Vector3(0, 5, 3), PickupType.Health, 20).AssociateWith(server);

            var values = Enum.GetValues(typeof(PedModel));
            PedModel randomPedModel = (PedModel)values.GetValue(new Random().Next(values.Length))!;
            new Ped(randomPedModel, new Vector3(10, 0, 3)).AssociateWith(server);

            new WorldObject(ObjectModel.Drugred, new Vector3(15, 0, 3)).AssociateWith(server);
            
            new WeaponObject(355, new Vector3(10, 10, 5))
            {
                TargetType = WeaponTargetType.Fixed,
                TargetPosition = new Vector3(10, 10, 5)
            }.AssociateWith(server);
            var vehicle = new Vehicle(602, new Vector3(-10, 5, 3)).AssociateWith(server);
            var aircraft = new Vehicle(520, new Vector3(10, 5, 3)).AssociateWith(server);
            var forklift = new Vehicle(530, new Vector3(20, 5, 3)).AssociateWith(server);
            var forklift2 = new Vehicle(530, new Vector3(22, 5, 3)).AssociateWith(server);
            var firetruck = new Vehicle(407, new Vector3(30, 5, 3)).AssociateWith(server);
            var firetruck2 = new Vehicle(407, new Vector3(35, 5, 3)).AssociateWith(server);

            vehicle.PedEntered += async (sender, eventArgs) =>
            {
                if (eventArgs.Seat == 1)
                {
                    await Task.Delay(500);
                    eventArgs.Vehicle.RemovePassenger(eventArgs.Ped);
                }
            };
        }

        private void OnPlayerJoin(Player player)
        {
            var client = player.Client;

            this.chatBox.Output($"{player.Name} ({client.Version}) ({client.Serial}) has joined the server!");

            player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
            player.Health = 50;
            player.Alpha = 100;
            player.Camera.Target = player;
            player.Camera.Fade(CameraFade.In);

            this.chatBox.OutputTo(player, "Hello world");
            this.chatBox.ClearFor(player);
            this.chatBox.OutputTo(player, "Hello World Again");

            this.console.OutputTo(player, "Hello Console World");

            this.debugLog.SetVisibleTo(player, true);
            this.debugLog.OutputTo(player, "Test debug message", DebugLevel.Custom, Color.Red);
            this.debugLog.OutputTo(player, "Test debug message 2", DebugLevel.Information);

            player.ShowHudComponent(HudComponent.Money, false);
            player.SetFpsLimit(60);
            player.PlaySound(1);
            player.WantedLevel = 4;
            //player.ForceMapVisible(true);
            //player.ToggleAllControls(false, true, true);

            player.Wasted += async (o, args) =>
            {
                await Task.Delay(500);
                player.Camera.Fade(CameraFade.Out, 1.75f);
                await Task.Delay(2000);
                player.Camera.Fade(CameraFade.In, 0);
                player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
            };
            player.OnCommand += (o, args) => { if (args.Command == "kill") player.Kill(); };
            player.OnCommand += (o, args) => {
                if (args.Command == "boom")
                    this.explosionService.CreateExplosion(player.Position, ExplosionType.Tiny);

                if (args.Command == "m4")
                    player.CurrentWeapon = new Weapon(WeaponId.M4, 500);

                if (args.Command == "assault")
                    player.CurrentWeaponSlot = WeaponSlot.AssaultRifles;

                if (args.Command == "rocket")
                    player.CurrentWeapon = new Weapon(WeaponId.RocketLauncher, 500);

                if (args.Command == "shootrocket")
                {
                    var position = player.Position + new Vector3(0, 0, 0.7f);
                    this.worldService.CreateProjectile(position, player.Rotation, player);
                }

                if (args.Command == "fire")
                    this.fireService.CreateFire(player.Position);

                if (args.Command == "ping")
                    chatBox.OutputTo(player, $"Your ping is {player.Client.Ping}", Color.YellowGreen);

                if (args.Command == "resendmodpackets")
                    player.ResendModPackets();

                if (args.Command == "ac")
                    player.ResendPlayerACInfo();
            };

            player.OnACInfo += (o, args) =>
            {
                logger.LogInformation($"ACInfo for {player.Name} detectedACList:{string.Join(",", args.DetectedACList)} d3d9Size: {args.D3D9Size} d3d9SHA256: {args.D3D9SHA256}");
            };

            //player.AddWeapon(WeaponId.Ak47, 500, true);
            //player.AddWeapon(WeaponId.Tec9, 500, true);
            //player.AddWeapon(WeaponId.Sniper, 500, true);
            //player.AddWeapon(WeaponId.Deagle, 500, true);
            //player.AddWeapon(WeaponId.Golfclub, 500, true);

            //player.RemoveWeapon(WeaponId.Tec9, 500);
            //player.RemoveWeapon(WeaponId.Sniper);
            //player.RemoveWeapon(WeaponId.Deagle, 200);
            //player.SetAmmoCount(WeaponSlot.AssaultRifles, 750, 25);

            player.Weapons.Add(new Weapon(WeaponId.Ak47, 500));
            player.Weapons.Add(new Weapon(WeaponId.Tec9, 500));
            player.Weapons.Add(new Weapon(WeaponId.Sniper, 500));
            player.Weapons.Add(new Weapon(WeaponId.Deagle, 500));
            player.Weapons.Add(new Weapon(WeaponId.Golfclub, 1));
            player.Weapons.Remove(WeaponId.Tec9);
            player.Weapons.Remove(WeaponId.Sniper);
            player.Weapons.First(weapon => weapon.Type == WeaponId.Deagle).Ammo -= 200;
            player.Weapons.First(weapon => weapon.Type == WeaponId.Ak47).Ammo = 750;
            player.Weapons.First(weapon => weapon.Type == WeaponId.Ak47).AmmoInClip = 25;
            
            this.testResource?.StartFor(player);
        }

        private void TriggerTestEvent(Player player)
        {
            var table = new LuaValue(new Dictionary<LuaValue, LuaValue>()
            {
                ["x"] = 5.5f,
                ["y"] = "string",
                ["z"] = new LuaValue(new Dictionary<LuaValue, LuaValue>() { }),
                ["w"] = false
            });
            table.TableValue?.Add("self", table);

            this.luaService.TriggerEvent(player, "Slipe.Test.ClientEvent", root, "String value", true, 23, table);
        }
    }
}
