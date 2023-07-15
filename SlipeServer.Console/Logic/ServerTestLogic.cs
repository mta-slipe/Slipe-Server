using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SlipeServer.Console.Elements;
using SlipeServer.Console.LuaValues;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Enums.VehicleUpgrades;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Packets.Structs;
using SlipeServer.Server;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Services;
using SlipeServer.Server.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SlipeServer.Packets.Constants.KeyConstants;

namespace SlipeServer.Console.Logic;

public class TestLogic
{
    public TestLogic()
    {
        System.Console.WriteLine("TestLogic started");
    }
}

public class ServerTestLogic
{
    private readonly MtaServer<CustomPlayer> server;
    private readonly IElementCollection elementCollection;
    private readonly RootElement root;
    private readonly GameWorld worldService;
    private readonly DebugLog debugLog;
    private readonly ILogger logger;
    private readonly ChatBox chatBox;
    private readonly ClientConsole console;
    private readonly LuaEventService luaService;
    private readonly ExplosionService explosionService;
    private readonly FireService fireService;
    private readonly TextItemService textItemService;
    private readonly IResourceProvider resourceProvider;
    private readonly CommandService commandService;
    private readonly WeaponConfigurationService weaponConfigurationService;
    private readonly GameWorld gameWorld;
    private readonly IElementIdGenerator elementIdGenerator;
    private Resource? testResource;
    private Resource? secondTestResource;
    private Resource? thirdTestResource;

    private readonly Random random = new();
    private RadarArea? RadarArea { get; set; }
    private Marker? Marker { get; set; }
    private Blip? BlipA { get; set; }
    private Blip? BlipB { get; set; }
    private WorldObject? WorldObject { get; set; }
    private WorldObject? Bin { get; set; }
    private Vehicle? Vehicle { get; set; }
    private Vehicle? Aircraft { get; set; }
    private Vehicle? Taxi { get; set; }
    private Vehicle? Rhino { get; set; }
    private Vehicle? Elegy { get; set; }
    private Vehicle? Flash { get; set; }
    private Vehicle? Stratum { get; set; }
    private Vehicle? Sultan { get; set; }
    private Vehicle? Jester { get; set; }
    private Vehicle? Uranus { get; set; }
    private Vehicle? Club { get; set; }
    private Vehicle? Slamvan { get; set; }
    private Vehicle? Remmington { get; set; }
    private Vehicle? FrozenVehicle { get; set; }
    private Vehicle? PrivateVehicle { get; set; }
    private Vehicle? Roadtrain { get; set; }
    private Vehicle? Trailer1 { get; set; }
    private Vehicle? Trailer2 { get; set; }
    private Ped? Ped { get; set; }
    private Ped? Ped2 { get; set; }
    private Ped? CJ { get; set; }
    private readonly Team slipeDevsTeam;

    public ServerTestLogic(
        MtaServer<CustomPlayer> server,
        IElementCollection elementCollection,
        RootElement root,
        GameWorld world,
        DebugLog debugLog,
        ILogger logger,
        ChatBox chatBox,
        ClientConsole console,
        LuaEventService luaService,
        ExplosionService explosionService,
        FireService fireService,
        TextItemService textItemService,
        IResourceProvider resourceProvider,
        CommandService commandService,
        WeaponConfigurationService weaponConfigurationService,
        GameWorld gameWorld,
        IElementIdGenerator elementIdGenerator
    )
    {
        this.server = server;
        this.elementCollection = elementCollection;
        this.root = root;
        this.worldService = world;
        this.debugLog = debugLog;
        this.logger = logger;
        this.chatBox = chatBox;
        this.console = console;
        this.luaService = luaService;
        this.explosionService = explosionService;
        this.fireService = fireService;
        this.textItemService = textItemService;
        this.resourceProvider = resourceProvider;
        this.commandService = commandService;
        this.weaponConfigurationService = weaponConfigurationService;
        this.elementIdGenerator = elementIdGenerator;
        this.gameWorld = gameWorld;

        this.slipeDevsTeam = new Team("Slipe devs", Color.FromArgb(255, 255, 81, 81));
        this.SetupTestLogic();
    }

    private void SetupTestLogic()
    {
        SetupTestElements();
        SetupTestCommands();

        this.luaService.AddEventHandler("Slipe.Test.Event", (e) => this.TriggerTestEvent(e.Player));
        this.luaService.AddEventHandler("Slipe.Test.SampleEvent", (e) => this.HandleSampleEvent(e));

        this.worldService.SetWeather(Weather.ExtraSunnyDesert);
        this.worldService.CloudsEnabled = false;
        this.worldService.SetTime(13, 37);
        this.worldService.MinuteDuration = 60_000;
        this.worldService.SetSkyGradient(Color.Aqua, Color.Teal);
        this.worldService.SetGlitchEnabled(GlitchType.GLITCH_FASTSPRINT, true);

        this.server.PlayerJoined += OnPlayerJoin;
    }

    private void SetupTestElements()
    {
        this.testResource = this.resourceProvider.GetResource("TestResource");
        this.secondTestResource = this.resourceProvider.GetResource("SecondTestResource");
        this.secondTestResource.NoClientScripts[$"{this.secondTestResource!.Name}/testfile.lua"] =
            Encoding.UTF8.GetBytes("outputChatBox(\"I AM A NOT CACHED MESSAGE\")");
        this.secondTestResource.NoClientScripts[$"blabla.lua"] = new byte[] { };

        this.thirdTestResource = this.resourceProvider.GetResource("MetaXmlTestResource");

        new WorldObject(321, new Vector3(5, 0, 3)).AssociateWith(this.server);
        new Water(new Vector3[]
        {
                new Vector3(-6, 0, 4), new Vector3(-3, 0, 4),
                new Vector3(-6, 3, 4), new Vector3(-3, 3, 4)
        }).AssociateWith(this.server);
        new WorldObject(321, new Vector3(5, 0, 3)).AssociateWith(this.server);
        this.BlipA = new Blip(new Vector3(20, 0, 0), BlipIcon.Marker, 50).AssociateWith(this.server);
        this.BlipB = new Blip(new Vector3(15, 0, 0), BlipIcon.Marker, 50).AssociateWith(this.server);
        this.RadarArea = new RadarArea(new Vector2(0, 0), new Vector2(200, 200), Color.FromArgb(100, Color.Aqua)).AssociateWith(this.server);

        this.Marker = new Marker(new Vector3(5, 0, 2), MarkerType.Cylinder)
        {
            Color = Color.FromArgb(100, Color.Cyan)
        }.AssociateWith(this.server);

        new Pickup(new Vector3(0, -5, 3), PickupType.Health, 20)
        {
            RespawnTime = 5000
        }.AssociateWith(this.server);
        new Pickup(new Vector3(3, -5, 3), PickupType.Armor, 20)
        {
            RespawnTime = 5000
        }.AssociateWith(this.server);
        new Pickup(new Vector3(5, -5, 3), WeaponId.Ak47, 100)
        {
            RespawnTime = 5000
        }.AssociateWith(this.server);

        var values = Enum.GetValues(typeof(PedModel));
        PedModel randomPedModel = (PedModel)values.GetValue(new Random().Next(values.Length))!;
        this.Ped = new Ped(randomPedModel, new Vector3(10, 0, 3)).AssociateWith(this.server);
        this.Ped2 = new Ped(PedModel.Ballas3, new Vector3(10, 5, 3)).AssociateWith(this.server);
        this.CJ = new Ped(0, new Vector3(-2, -12, 3)).AssociateWith(this.server);
        this.CJ.Clothing.Shirt = 0; // no shirt
        this.CJ.Clothing.Head = 5; // pink hair
        this.CJ.Clothing.Trousers = 9; // heart-shaped panties
        this.CJ.Clothing.Shoes = 8; // sandal + sock
        this.CJ.Clothing.Glasses = 1; // zorro


        this.WorldObject = new WorldObject(ObjectModel.Drugred, new Vector3(15, 0, 3)).AssociateWith(this.server);
        this.Bin = new WorldObject(ObjectModel.BinNt07LA, new Vector3(-15, 0, 3)).AssociateWith(this.server);

        new WeaponObject(355, new Vector3(10, 10, 5))
        {
            TargetType = WeaponTargetType.Fixed,
            TargetPosition = new Vector3(10, 10, 5)
        }.AssociateWith(this.server);

        this.FrozenVehicle = new Vehicle(602, new Vector3(0, 0, 10)).AssociateWith(this.server);
        this.FrozenVehicle.IsFrozen = true;
        
        this.PrivateVehicle = new Vehicle(602, new Vector3(-10.58f, -5.70f, 3.11f)).AssociateWith(this.server);
        this.PrivateVehicle.CanEnter = (Ped ped, Vehicle vehicle) =>
        {
            if (ped is Player player)
            {
                this.chatBox.OutputTo(player, $"Sorry, this is private vehicle.");
            }
            return false;
        };

        this.Roadtrain = new Vehicle(VehicleModel.Roadtrain, new Vector3(10, 30, 5)).AssociateWith(this.server);
        this.Trailer1 = new Vehicle(VehicleModel.Trailer1, new Vector3(15, 30, 3)).AssociateWith(this.server);
        this.Trailer2 = new Vehicle(VehicleModel.Trailer2, new Vector3(20, 30, 3)).AssociateWith(this.server);

        var vehicle = new Vehicle(602, new Vector3(-10, 5, 3)).AssociateWith(this.server);
        vehicle.HeadlightColor = Color.Red;

        this.Aircraft = new Vehicle(520, new Vector3(10, -10, 3)).AssociateWith(this.server);
        this.Vehicle = new Vehicle(530, new Vector3(20, 5, 3)).AssociateWith(this.server);
        this.Taxi = new Vehicle((ushort)VehicleModel.Taxi, new Vector3(20, -5, 3)).AssociateWith(this.server);
        this.Rhino = new Vehicle((ushort)VehicleModel.Rhino, new Vector3(20, -25, 3)).AssociateWith(this.server);

        this.Elegy = new Vehicle(562, new Vector3(30, -20, 3)).AssociateWith(this.server);
        this.Flash = new Vehicle(565, new Vector3(34, -20, 3)).AssociateWith(this.server);
        this.Stratum = new Vehicle(561, new Vector3(38, -20, 3)).AssociateWith(this.server);
        this.Sultan = new Vehicle(560, new Vector3(42, -20, 3)).AssociateWith(this.server);
        this.Jester = new Vehicle(559, new Vector3(46, -20, 3)).AssociateWith(this.server);
        this.Uranus = new Vehicle(558, new Vector3(50, -20, 3)).AssociateWith(this.server);
        this.Club = new Vehicle(589, new Vector3(54, -20, 3)).AssociateWith(this.server);
        this.Slamvan = new Vehicle(535, new Vector3(58, -20, 3)).AssociateWith(this.server);
        this.Remmington = new Vehicle(534, new Vector3(62, -20, 3)).AssociateWith(this.server);

        var forklift2 = new Vehicle(530, new Vector3(22, 5, 3)).AssociateWith(this.server);
        var firetruck = new Vehicle(407, new Vector3(30, 5, 3)).AssociateWith(this.server);
        var firetruck2 = new Vehicle(407, new Vector3(35, 5, 3)).AssociateWith(this.server);

        var polygon1 = new CollisionPolygon(new Vector3(0, -25, 0), new Vector2[] { new Vector2(-25, -25), new Vector2(-25, -50), new Vector2(-50, -25) }).AssociateWith(this.server);
        var polygon2 = new CollisionPolygon(new Vector3(0, 25, 0), new Vector2[] { new Vector2(25, 25), new Vector2(25, 50), new Vector2(50, 25) }).AssociateWith(this.server);

        vehicle.PedEntered += async (sender, eventArgs) =>
        {
            if (eventArgs.Seat == 1)
            {
                await Task.Delay(500);
                eventArgs.Vehicle.RemovePassenger(eventArgs.Ped);
            }
        };

        vehicle.PedLeft += async (sender, eventArgs) =>
        {
            if (eventArgs.Seat == 0)
            {
                await Task.Delay(1500);
                vehicle?.Respawn();
                this.logger.LogInformation("Vehicle has been respawned.");
            }
        };

        var circle = new CollisionCircle(new Vector2(0, 25), 3).AssociateWith(this.server);
        var sphere = new CollisionSphere(new Vector3(0, 25, 0), 3).AssociateWith(this.server);
        var tube = new CollisionTube(new Vector3(0, 25, 0), 3, 3).AssociateWith(this.server);
        var polygon = new CollisionPolygon(new Vector3(0, -25, 0), new Vector2[] { new Vector2(-25, -25), new Vector2(-25, -50), new Vector2(-50, -25) }).AssociateWith(this.server);
        var rectangle = new CollisionRectangle(new Vector2(50, 20), new Vector2(2, 2)).AssociateWith(this.server);
        var cuboid = new CollisionCuboid(new Vector3(30, 20, 4), new Vector3(2, 2, 2)).AssociateWith(this.server);
        Task.Run(async () =>
        {
            int i = 0;
            while (true)
            {
                await Task.Delay(1000);
                this.WorldObject.Model = (ushort)ObjectModel.Drugblue;
                this.Vehicle.Model = (ushort)VehicleModel.Bobcat;
                this.Ped.Model = (ushort)this.random.Next(20, 25);
                this.Taxi.IsTaxiLightOn = !this.Taxi.IsTaxiLightOn;
                await Task.Delay(1000);
                this.WorldObject.Model = (ushort)ObjectModel.Drugred;
                this.Vehicle.Model = (ushort)VehicleModel.BMX;
                this.Ped.Model = (ushort)this.random.Next(20, 25);
                this.Taxi.IsTaxiLightOn = !this.Taxi.IsTaxiLightOn;
                this.Taxi.PlateText = $"i {i++}";

                this.WorldObject.Scale = Vector3.One * ((float)this.random.NextDouble() * 3 + 1);
            }
        });

        var shape = new CollisionCircle(new Vector2(0, 25), 3).AssociateWith(this.server);

        circle.RadiusChanged += async (Element sender, ElementChangedEventArgs<float> args) =>
        {
            await Task.Delay(100);
            if (circle.Radius < 20)
                circle.Radius += .03f;
        };

        sphere.RadiusChanged += async (Element sender, ElementChangedEventArgs<float> args) =>
        {
            await Task.Delay(100);
            if (sphere.Radius < 20)
                sphere.Radius += .03f;
        };

        tube.RadiusChanged += async (Element sender, ElementChangedEventArgs<float> args) =>
        {
            await Task.Delay(100);
            if (tube.Radius < 20)
                tube.Radius += .03f;
        };

        tube.HeightChanged += async (Element sender, ElementChangedEventArgs<float> args) =>
        {
            await Task.Delay(100);
            if (tube.Height < 20)
                tube.Height += .03f;
        };

        polygon.HeightChanged += async (Element sender, ElementChangedEventArgs<Vector2> args) =>
        {
            await Task.Delay(100);
            if (polygon.Height.X > -3)
                polygon.Height = new Vector2(polygon.Height.X - .03f, polygon.Height.Y - .03f);
        };

        polygon.PointPositionChanged += async (Element sender, CollisionPolygonPointPositionChangedArgs args) =>
        {
            await Task.Delay(100);
            if (args.Position.X < 0.0f)
                args.Polygon.SetPointPosition(args.Index, new Vector2(args.Position.X + 0.03f, args.Position.Y));
        };

        rectangle.DimensionsChanged += async (Element sender, ElementChangedEventArgs<Vector2> args) =>
        {
            await Task.Delay(100);
            if (args.NewValue.Y < 10.0f)
                rectangle.Dimensions = args.OldValue + new Vector2(0.03f, 0.03f);
        };

        cuboid.DimensionsChanged += async (Element sender, ElementChangedEventArgs<Vector3> args) =>
        {
            await Task.Delay(100);
            if (args.NewValue.Y < 10.0f)
                cuboid.Dimensions = args.OldValue + new Vector3(0.03f, 0.03f, 0.03f);
        };

        Task.Run(async () =>
        {
            await Task.Delay(5000);
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(2000);
                polygon.AddPoint(new Vector2(this.random.Next(-20, 20), this.random.Next(-20, 20)));
            }
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(2000);
                polygon.RemovePoint(0);
            }
        });

        circle.Radius = 3;
        sphere.Radius = 3;
        tube.Radius = 3;
        tube.Height = 3;
        polygon.Height = new Vector2(10, 15);
        polygon.SetPointPosition(0, new Vector2(-25, -25));
        rectangle.Dimensions = new Vector2(2, 2);
        cuboid.Dimensions = new Vector3(2, 2, 2);
    }

    private void SetupTestCommands()
    {
        this.commandService.AddCommand("radararea").Triggered += (source, args) =>
        {
            this.RadarArea!.Color = Color.FromArgb(this.random.Next(0, 255), this.random.Next(0, 255), this.random.Next(0, 255), this.random.Next(0, 255));
            this.RadarArea.Size = new Vector2(this.random.Next(100, 200), this.random.Next(100, 200));
            this.RadarArea.IsFlashing = this.random.Next(2) == 1;
            this.chatBox.OutputTo(args.Player, "You have randomized radar area!", Color.YellowGreen);
        };
        this.commandService.AddCommand("kill").Triggered += (source, args) => args.Player.Kill();
        this.commandService.AddCommand("spawn").Triggered += (source, args) => args.Player.Spawn(new Vector3(20, 0, 3), 0, 9, 0, 0);

        this.commandService.AddCommand("night").Triggered += (source, args) => this.worldService.SetTime(0, 0);
        this.commandService.AddCommand("day").Triggered += (source, args) => this.worldService.SetTime(13, 37);

        bool flip = false;
        this.commandService.AddCommand("blip").Triggered += (source, args) =>
        {
            var values = Enum.GetValues(typeof(BlipIcon));
            BlipIcon randomBlipIcon = (BlipIcon)values.GetValue(this.random.Next(values.Length))!;

            this.BlipB!.Icon = randomBlipIcon;
            this.BlipA!.Color = Color.FromArgb(this.random.Next(0, 255), this.random.Next(0, 255), this.random.Next(0, 255), this.random.Next(0, 255));
            this.BlipA.Size = (byte)this.random.Next(1, 4);
            this.BlipA.VisibleDistance = (ushort)this.random.Next(30, 100);
            flip = !flip;
            if (flip)
            {
                this.BlipA.Ordering = 1;
                this.BlipB.Ordering = 2;
            } else
            {
                this.BlipA.Ordering = 2;
                this.BlipB.Ordering = 1;
            }
        };

        this.commandService.AddCommand("boom").Triggered += (source, args)
            => this.explosionService.CreateExplosion(args.Player.Position, ExplosionType.Tiny);

        this.commandService.AddCommand("m4").Triggered += (source, args)
            => args.Player.CurrentWeapon = new Weapon(WeaponId.M4, 500);

        this.commandService.AddCommand("assault").Triggered += (source, args)
            => args.Player.CurrentWeaponSlot = WeaponSlot.AssaultRifles;

        this.commandService.AddCommand("rocket").Triggered += (source, args)
            => args.Player.CurrentWeapon = new Weapon(WeaponId.RocketLauncher, 500);

        this.commandService.AddCommand("shootrocket").Triggered += (source, args) =>
        {
            var position = args.Player.Position + new Vector3(0, 0, 0.7f);
            this.worldService.CreateProjectile(position, args.Player.Rotation, args.Player);
        };

        this.commandService.AddCommand("fire").Triggered += (source, args)
            => this.fireService.CreateFire(args.Player.Position);

        this.commandService.AddCommand("ts").Triggered += (source, args)
            => args.Player.TakeScreenshot(256, 256, "lowqualitytag", 30);

        this.commandService.AddCommand("tshq").Triggered += (source, args)
            => args.Player.TakeScreenshot(960, 540, "highqualitytag", 70);

        this.commandService.AddCommand("ping").Triggered += (source, args)
            => this.chatBox.OutputTo(args.Player, $"Your ping is {args.Player.Client.Ping}", Color.YellowGreen);

        this.commandService.AddCommand("kickme").Triggered += (source, args)
            => args.Player.Kick("You have been kicked by slipe");

        this.commandService.AddCommand("a51").Triggered += (source, args)
            => args.Player.Position = new Vector3(216.46f, 1895.05f, 17.28f);

        this.commandService.AddCommand("updildo").Triggered += (source, args)
            => new WorldObject(321, args.Player.Position + args.Player.Up * 2).AssociateWith(this.server);

        this.commandService.AddCommand("rightdildo").Triggered += (source, args)
            => new WorldObject(321, args.Player.Position + args.Player.Right * 2).AssociateWith(this.server);

        this.commandService.AddCommand("forwarddildo").Triggered += (source, args)
            => new WorldObject(321, args.Player.Position + args.Player.Forward * 2).AssociateWith(this.server);

        this.commandService.AddCommand("playerlist").Triggered += (source, args) =>
        {
            var players = this.elementCollection.GetByType<Player>();
            foreach (var remotePlayer in players)
                this.chatBox.OutputTo(args.Player, remotePlayer.Name);

            var text = string.Join('\n', players.Select(x => x.Name));
            var textItem = this.textItemService.CreateTextItemFor(args.Player, text, Vector2.Zero, 5);
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                this.textItemService.DeleteTextItemFor(args.Player, textItem);
            });
        };

        this.commandService.AddCommand("increment").Triggered += (source, args)
            => args.Player.GetAndIncrementTimeContext();

        this.commandService.AddCommand("resendmodpackets").Triggered += (source, args)
            => args.Player.ResendModPackets();

        this.commandService.AddCommand("ac").Triggered += (source, args)
            => args.Player.ResendPlayerACInfo();

        this.commandService.AddCommand("setmaxplayers").Triggered += (source, args) =>
        {
            if (args.Arguments.Length > 0)
            {
                if (ushort.TryParse(args.Arguments[0], out ushort slots))
                {
                    this.server.SetMaxPlayers(slots);
                    this.logger.LogInformation("Slots has been changed to: {slots}", slots);
                }
            }
        };

        this.commandService.AddCommand("vehicle").Triggered += (source, args) =>
        {
            if (args.Arguments.Length > 0)
            {
                if (Enum.TryParse(args.Arguments[0], true, out VehicleModel model) && Enum.IsDefined(model))
                {
                    var vehicle = (new Vehicle(model, args.Player.Position)).AssociateWith(this.server);
                    args.Player.WarpIntoVehicle(vehicle);
                }
            }
        };

        this.commandService.AddCommand("weapon").Triggered += (source, args) =>
        {
            if (args.Arguments.Length > 0)
            {
                if (Enum.TryParse(args.Arguments[0], true, out WeaponId weapon) && Enum.IsDefined(weapon))
                {
                    args.Player.CurrentWeapon = new Weapon(weapon, 500);
                }
            }
        };


        this.commandService.AddCommand("upvehicle").Triggered += (source, args) =>
        {
            if (args.Arguments.Length > 0)
            {
                if (Enum.TryParse(args.Arguments[0], true, out VehicleModel model) && Enum.IsDefined(model))
                {
                    var vehicle = (new Vehicle(model, args.Player.Position + args.Player.Up * 10)).AssociateWith(this.server);
                }
            }
        };

        this.commandService.AddCommand("changeskin").Triggered += (source, args) =>
        {
            if (args.Arguments.Length > 0)
            {
                if (Enum.TryParse<PedModel>(args.Arguments[0], true, out var pedModel))
                {
                    args.Player.Model = (ushort)pedModel;
                } else if (ushort.TryParse(args.Arguments[0], out ushort model))
                {
                    args.Player.Model = model;
                }
            } else
            {
                args.Player.Model = (ushort)this.random.Next(20, 25);
            }
        };

        this.commandService.AddCommand("togglecontrol").Triggered += (source, args)
            => args.Player.Controls.JumpEnabled = !args.Player.Controls.JumpEnabled;

        this.commandService.AddCommand("jetpack").Triggered += (source, args)
            => args.Player.HasJetpack = !args.Player.HasJetpack;

        this.commandService.AddCommand("landinggear").Triggered += (source, args)
            => this.Aircraft!.IsLandingGearDown = !this.Aircraft!.IsLandingGearDown;

        this.commandService.AddCommand("turret").Triggered += (source, args) =>
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(30);
                    this.Rhino!.TurretRotation = new Vector2(-MathF.Atan2(this.Rhino.Position.X - args.Player.Position.X, this.Rhino.Position.Y - args.Player.Position.Y) + MathF.PI, 0);
                }
            });
        };

        this.commandService.AddCommand("marker").Triggered += (source, args) =>
        {
            var typeValues = Enum.GetValues(typeof(MarkerType));
            MarkerType? randomMarkerType = (MarkerType?)typeValues.GetValue(this.random.Next(typeValues.Length));
            var iconValues = Enum.GetValues(typeof(MarkerIcon));
            MarkerIcon? randomMarkerIcon = (MarkerIcon?)iconValues.GetValue(this.random.Next(iconValues.Length));

            this.Marker!.Color = Color.FromArgb(this.random.Next(0, 255), this.random.Next(0, 255), this.random.Next(0, 255), this.random.Next(0, 255));
            this.Marker!.Size = this.random.Next(1, 10) / 10.0f + 1.0f;
            if (randomMarkerType.HasValue)
                this.Marker!.MarkerType = randomMarkerType.Value;
            if (randomMarkerIcon.HasValue)
                this.Marker!.MarkerIcon = randomMarkerIcon.Value;

            this.Marker!.TargetPosition = new Vector3(this.random.Next(0, 20) - 5, this.random.Next(0, 20), this.random.Next(-50, 50));
            this.chatBox.OutputTo(args.Player, "You have randomized marker!", Color.YellowGreen);
        };

        this.commandService.AddCommand("closevehicles").Triggered += (source, args) =>
        {
            var vehicles = this.elementCollection.GetWithinRange<Vehicle>(args.Player.Position, 25, ElementType.Vehicle);
            this.chatBox.OutputTo(args.Player, $"There are {vehicles.Count()} vehicles near you.");
            foreach (var vehicle in vehicles)
                this.chatBox.OutputTo(args.Player, $"- {(VehicleModel)vehicle.Model}.");

        };

        this.commandService.AddCommand("vehicleUpdates").Triggered += (source, args) =>
        {
            for (int i = 0; i < 8; i++)
            {
                Task.Run(async () =>
                {
                    for (int j = 0; j < 250; j++)
                    {
                        this.Aircraft!.Position += (j % 2 == 0 ? -1 : 1) * Vector3.UnitZ;
                        await Task.Delay(1);
                    }
                });
            }
        };

        this.commandService.AddCommand("camerainterior").Triggered += (source, args) =>
        {
            if (args.Arguments.Length > 0)
            {
                if (byte.TryParse(args.Arguments[0], out byte interior))
                {
                    args.Player.Camera.Interior = interior;
                    this.logger.LogInformation("Camera interior changed to: {interior}", interior);
                }
            }
        };

        this.commandService.AddCommand("setmydata").Triggered += (sender, args) =>
        {
            string key = args.Arguments[0];
            string value = args.Arguments[1];
            this.chatBox.OutputTo(args.Player, $"This Is setmydata command Handler, key value -> {key}, {value}");
            args.Player.SetData(key, value, DataSyncType.Broadcast);
        };

        this.commandService.AddCommand("setmysubbeddata").Triggered += (sender, args) =>
        {
            string key = args.Arguments[0];
            string value = args.Arguments[1];
            this.chatBox.OutputTo(args.Player, $"This Is setmydata command Handler, key value -> {key}, {value}");
            args.Player.SetData(key, value, DataSyncType.Subscribe);
        };

        this.commandService.AddCommand("subtodata").Triggered += (sender, args) =>
        {
            string key = args.Arguments[0];
            args.Player.SubscribeToData(args.Player, key);
        };

        this.commandService.AddCommand("unsubfromdata").Triggered += (sender, args) =>
        {
            string key = args.Arguments[0];
            args.Player.UnsubscribeFromData(args.Player, key);
        };

        this.commandService.AddCommand("unsubfromalldata").Triggered += (sender, args) =>
        {
            args.Player.UnsubscribeFromAllData(args.Player);
        };

        this.commandService.AddCommand("getmydata").Triggered += (sender, args) =>
        {
            string key = args.Arguments[0];
            this.chatBox.OutputTo(args.Player, $"Your Key, Value => {key} , {args.Player.GetData(key)?.StringValue}");
        };

        this.commandService.AddCommand("pedsync").Triggered += (source, args) =>
        {
            this.Ped2?.RunAsSync(() =>
            {
                var random = new Random();
                this.Ped2.Position += new Vector3(random.Next(0, 3) * .1f, random.Next(0, 3) * .1f, random.Next(0, 3) * .1f);

                var packet = new Packets.Definitions.Ped.PedSyncPacket(new List<Packets.Structs.PedSyncData>()
                {
                        new()
                        {
                            SourceElementId = this.Ped2.Id,
                            TimeSyncContext = this.Ped2.TimeContext,
                            Flags = Packets.Enums.PedSyncFlags.Position,
                            Position = this.Ped2.Position
                        }
                });
                args.Player.Client.SendPacket(packet);
            });
        };

        this.commandService.AddCommand("rotateped").Triggered += (source, args) =>
        {
            this.Ped2!.PedRotation = args.Player.PedRotation;
        };

        this.commandService.AddCommand("heatmebro").Triggered += (source, args) =>
        {
            this.worldService.HeatHaze = new HeatHaze()
            {
                Intensity = args.Arguments.Length > 0 ? byte.Parse(args.Arguments[0]) : (byte)0,
            };
        };

        this.commandService.AddCommand("water").Triggered += (source, args) =>
        {
            this.worldService.WaterLevels = new WaterLevels()
            {
                SeaLevel = args.Arguments.Length > 0 ? float.Parse(args.Arguments[0]) : (float)0,
            };
        };

        this.commandService.AddCommand("watercolor").Triggered += (source, args) =>
        {
            this.worldService.WaterColor = args.Arguments.Length > 0 ?
                Color.FromKnownColor(Enum.Parse<KnownColor>(args.Arguments[0], true)) :
                null;
        };

        this.commandService.AddCommand("wave").Triggered += (source, args) =>
        {
            this.worldService.WaveHeight = args.Arguments.Length > 0 ? float.Parse(args.Arguments[0]) : (float)0;
        };

        this.commandService.AddCommand("maxjetpack").Triggered += (source, args) =>
        {
            this.worldService.MaxJetpackHeight = args.Arguments.Length > 0 ? float.Parse(args.Arguments[0]) : (float)0;
        };

        this.commandService.AddCommand("fpslimit").Triggered += (source, args) =>
        {
            this.worldService.FpsLimit = args.Arguments.Length > 0 ? byte.Parse(args.Arguments[0]) : (byte)0;
        };

        this.commandService.AddCommand("cameramatrix").Triggered += (source, args) =>
        {
            args.Player.Camera.SetMatrix(new Vector3(20, 20, 20), new Vector3(200, 200, 0));
        };

        this.commandService.AddCommand("kungfu").Triggered += (source, args) =>
        {
            args.Player.FightingStyle = FightingStyle.KungFu;
        };

        this.commandService.AddCommand("alpha").Triggered += (source, args) =>
        {
            if (args.Arguments.Length > 0 && byte.TryParse(args.Arguments[0], out var alpha))
                args.Player.Alpha = alpha;
            else
                args.Player.Alpha = 255;
        };

        this.commandService.AddCommand("cj").Triggered += (source, args) =>
        {
            args.Player.Model = 0;
        };

        this.commandService.AddCommand("mcdonalds").Triggered += (source, args) =>
        {
            args.Player.SetStat(Packets.Enums.PedStat.FAT, 1000);
        };

        this.commandService.AddCommand("salad").Triggered += (source, args) =>
        {
            args.Player.SetStat(Packets.Enums.PedStat.FAT, 0);
        };

        this.commandService.AddCommand("gym").Triggered += (source, args) =>
        {
            args.Player.SetStat(Packets.Enums.PedStat.BODY_MUSCLE, 1000);
        };

        this.commandService.AddCommand("lazy").Triggered += (source, args) =>
        {
            args.Player.SetStat(Packets.Enums.PedStat.BODY_MUSCLE, 0);
        };

        this.commandService.AddCommand("pro").Triggered += (source, args) =>
        {
            foreach (var stat in Server.Constants.WeaponConstants.WeaponStatsPerWeapon)
                args.Player.SetWeaponStat(stat.Key, 1000);
        };

        this.commandService.AddCommand("money").Triggered += (source, args) =>
        {
            args.Player.ShowHudComponent(HudComponent.Money, true);
            args.Player.SetMoney(this.random.Next(0, 1000), false);
        };

        this.commandService.AddCommand("moneyinstant").Triggered += (source, args) =>
        {
            args.Player.ShowHudComponent(HudComponent.Money, true);
            args.Player.Money = this.random.Next(0, 1000);
        };

        this.commandService.AddCommand("movefrozenvehicle").Triggered += (source, args) =>
        {
            if (this.FrozenVehicle != null)
            {
                if (args.Arguments.Length > 0)
                    this.FrozenVehicle.Position = args.Player.Position;
                else
                    this.FrozenVehicle.Position = args.Player.Position + new Vector3(0, 0, 3);
            }
        };

        this.commandService.AddCommand("latent").Triggered += (source, args) =>
        {
            this.luaService.TriggerLatentEvent("Slipe.Test.ClientEvent", this.testResource!, this.root, 1, this.root, 50, "STRING");
        };

        this.commandService.AddCommand("dim").Triggered += (source, args) =>
        {
            if (args.Arguments.Length > 0)
                args.Player.Dimension = ushort.Parse(args.Arguments[0]);
            else
                args.Player.Dimension = 0;
        };

        this.commandService.AddCommand("taxidim").Triggered += (source, args) =>
        {
            if (this.Taxi == null)
                return;

            if (args.Arguments.Length > 0)
                this.Taxi.Dimension = ushort.Parse(args.Arguments[0]);
            else
                this.Taxi.Dimension = 0;
        };

        this.commandService.AddCommand("trailerpls").Triggered += (source, args) =>
        {
            var vehicle = args.Player.Vehicle;
            if (vehicle == null)
                return;

            var newTrailer = new Vehicle(VehicleModel.Trailer3, vehicle.Position + (vehicle.Forward * -15))
            {
                Rotation = vehicle.Rotation
            }.AssociateWith(this.server);
            newTrailer.AttachToTower(vehicle);
        };

        this.commandService.AddCommand("notrailerpls").Triggered += (source, args) =>
        {
            args.Player.Vehicle?.AttachTrailer(null);
        };

        this.commandService.AddCommand("attach").Triggered += (source, args) =>
        {
            this.Bin?.AttachTo(args.Player, new Vector3(1, 0, 0));
        };

        this.commandService.AddCommand("detach").Triggered += (source, args) =>
        {
            this.Bin?.DetachFrom(args.Player);
        };

        this.commandService.AddCommand("moveattach").Triggered += (source, args) =>
        {
            if (this.Bin?.Attachment == null)
                return;

            this.Bin.Attachment.PositionOffset += new Vector3(1, 0, 0);
        };

        this.commandService.AddCommand("fueltank").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.IsFuelTankExplodable = !args.Player.Vehicle.IsFuelTankExplodable;
        };

        this.commandService.AddCommand("moveit").Triggered += async (source, args) =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var element = new WorldObject(321, new Vector3(0, 0, 3))
            {
                Velocity = new Vector3(0, 1, 0)
            }.AssociateWith(this.server);
            await Task.Delay(1000);
            stopwatch.Stop();
            var distance = Vector3.Distance(element.Position, new Vector3(0, 0, 3));
            this.logger.LogInformation("Element travelled {distance} units in {ms} ms", distance, stopwatch.ElapsedMilliseconds);
            element.Destroy();
        };

        this.commandService.AddCommand("deleteeverything").Triggered += (source, args) =>
        {
            this.server.BroadcastPacket(VehiclePacketFactory.CreateDestroyAllPacket());
            this.server.BroadcastPacket(BlipPacketFactory.CreateDestroyAllPacket());
            this.server.BroadcastPacket(WorldObjectPacketFactory.CreateDestroyAllPacket());
            this.server.BroadcastPacket(RadarAreaPacketFactory.CreateDestroyAllPacket());
            this.server.BroadcastPacket(MarkerPacketFactory.CreateDestroyAllPacket());
            this.server.BroadcastPacket(PickupPacketFactory.CreateDestroyAllPacket());
        };

        this.commandService.AddCommand("slipelua").Triggered += async (source, args) =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                await this.resourceProvider.GetResource("SlipeTestResource").StartForAsync(args.Player);
            }
            catch(Exception ex)
            {
                this.logger.LogError(ex, "Failed to start Slipe Lua test resource for {playerName}, {ex}", args.Player.Name, ex.ToString());
            }
            stopwatch.Stop();
            this.logger.LogInformation("Starting Slipe Lua test resource for {playerName} took {milliseconds}ms", args.Player.Name, stopwatch.ElapsedMilliseconds);
        };

        this.commandService.AddCommand("variant").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            if (args.Arguments.Length < 2)
                return;

            if (!byte.TryParse(args.Arguments[0], out var variant1) || !byte.TryParse(args.Arguments[1], out var variant2))
                return;

            args.Player.Vehicle.Variants = new()
            {
                Variant1 = variant1,
                Variant2 = variant2
            };
        };

        this.commandService.AddCommand("damageproof").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.IsDamageProof = !args.Player.Vehicle.IsDamageProof;
            this.chatBox.OutputTo(args.Player, $"Vehicle damage proof state: {args.Player.Vehicle.IsDamageProof}");
        };

        this.commandService.AddCommand("damageproofdoors").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.AreDoorsDamageProof = !args.Player.Vehicle.AreDoorsDamageProof;
            this.chatBox.OutputTo(args.Player, $"Vehicle doors damage proof state: {args.Player.Vehicle.AreDoorsDamageProof}");
        };

        this.commandService.AddCommand("derailable").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.IsDerailable = !args.Player.Vehicle.IsDerailable;
            this.chatBox.OutputTo(args.Player, $"Train derailable state: {args.Player.Vehicle.IsDerailable}");
        };

        this.commandService.AddCommand("derailed").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.IsDerailed = !args.Player.Vehicle.IsDerailed;
            this.chatBox.OutputTo(args.Player, $"Train derailed state: {args.Player.Vehicle.IsDerailed}");
        };

        this.commandService.AddCommand("traindirection").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.TrainDirection = args.Player.Vehicle.TrainDirection == TrainDirection.Clockwise ? TrainDirection.CounterClockwise : TrainDirection.Clockwise;
            this.chatBox.OutputTo(args.Player, $"Train direction: {args.Player.Vehicle.TrainDirection}");
        };

        this.commandService.AddCommand("togglesirens").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            args.Player.Vehicle.IsSirenActive = !args.Player.Vehicle.IsSirenActive;
            this.chatBox.OutputTo(args.Player, $"Siren state: {args.Player.Vehicle.IsSirenActive}.");
        };

        this.commandService.AddCommand("customsirens").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle == null)
                return;

            var sirens = new VehicleSirenSet()
            {
                SirenType = VehicleSirenType.Dual
            };
            sirens.AddSiren(new VehicleSiren()
            {
                Id = 0,
                Color = Color.AliceBlue,
                Is360 = true,
                UsesLineOfSightCheck = false,
                Position = new Vector3(1, 0, 1),
                SirenMinAlpha = 150
            });
            sirens.AddSiren(new VehicleSiren()
            {
                Id = 1,
                Color = Color.Crimson,
                Is360 = true,
                UsesLineOfSightCheck = false,
                Position = new Vector3(-1, 0, 1),
                SirenMinAlpha = 150
            });
            sirens.AddSiren(new VehicleSiren()
            {
                Id = 2,
                Color = Color.DarkBlue,
                Is360 = true,
                UsesLineOfSightCheck = false,
                Position = new Vector3(1, -1, 0),
                SirenMinAlpha = 150
            });
            sirens.AddSiren(new VehicleSiren()
            {
                Id = 3,
                Color = Color.IndianRed,
                Is360 = true,
                UsesLineOfSightCheck = false,
                Position = new Vector3(-1, -1, 0),
                SirenMinAlpha = 150
            });

            args.Player.Vehicle.Sirens = sirens;
            this.chatBox.OutputTo(args.Player, $"Sirens applied.");
        };

        this.commandService.AddCommand("removenearby").Triggered += (source, args) =>
        {
            if (!args.Arguments.Any())
                return;

            if (!ushort.TryParse(args.Arguments.First(), out var model))
                return;

            this.gameWorld.RemoveWorldModel(model, args.Player.Position, 100);
        };

        this.commandService.AddCommand("removeeverywhere").Triggered += (source, args) =>
        {
            if (!args.Arguments.Any())
                return;

            if (!ushort.TryParse(args.Arguments.First(), out var model))
                return;

            this.gameWorld.RemoveWorldModel(model, args.Player.Position, 12000);
        };

        this.commandService.AddCommand("restorenearby").Triggered += (source, args) =>
        {
            if (!args.Arguments.Any())
                return;

            if (!ushort.TryParse(args.Arguments.First(), out var model))
                return;

            this.gameWorld.RestoreWorldModel(model, args.Player.Position, 100);
        };

        this.commandService.AddCommand("restoreeverywhere").Triggered += (source, args) =>
        {
            if (!args.Arguments.Any())
                return;

            if (!ushort.TryParse(args.Arguments.First(), out var model))
                return;

            this.gameWorld.RestoreWorldModel(model, args.Player.Position, 12000);
        };

        this.commandService.AddCommand("removeall").Triggered += (source, args) =>
        {
            for (ushort i = 550; i < 20000; i++)
                this.gameWorld.RemoveWorldModel(i, Vector3.Zero, 12000);
        };

        this.commandService.AddCommand("restoreall").Triggered += (source, args) =>
        {
            this.gameWorld.RestoreAllWorldModels();
        };

        this.commandService.AddCommand("blipidzero").Triggered += (source, args) =>
        {
            var blip = new Blip(Vector3.Zero, BlipIcon.Truth);
            blip.CreateFor(args.Player);
        };

        this.commandService.AddCommand("gp").Triggered += (source, args) =>
        {
            this.chatBox.Output($"{args.Player.Position}");
        };

        this.commandService.AddCommand("createelementsforme").Triggered += (source, args) =>
        {
            uint id = 10_000;
            var create = (Element element) =>
            {
                element.Id = (ElementId)(id++);
                element.AssociateWith(args.Player);
            };

            var origin = new Vector3(-13.200195f, 26.257812f, 3.1171875f);
            args.Player.Position = origin;
            create(new Ped(PedModel.Ballas1, origin));
            create(new Vehicle(404, origin));
            create(new WorldObject(1337, origin));
            create(new Pickup(origin, 1274));

            var marker = new Marker(origin, MarkerType.Arrow);
            marker.Color = Color.Pink;
            create(marker);
            var collisionSphere = new CollisionSphere(origin, 3);
            collisionSphere.ElementEntered += e =>
            {
                this.chatBox.Output("entered element created for me");
            };
            create(collisionSphere);
            create(new Blip(origin, BlipIcon.Truth));
        };

        this.commandService.AddCommand("setvehicledata").Triggered += (source, args) =>
        {
            var vehicle = args.Player.Vehicle;
            if (vehicle == null || args.Arguments.Length < 2)
                return;

            var key = args.Arguments[0];
            var value = args.Arguments[1];
            this.chatBox.OutputTo(args.Player, $"Setting vehicle data {key} to {value}");
            vehicle.SetData(key, value, DataSyncType.Broadcast);
        };

        this.commandService.AddCommand("getvehicledata").Triggered += (source, args) =>
        {
            var vehicle = args.Player.Vehicle;
            if (vehicle == null || args.Arguments.Length < 1)
                return;

            var key = args.Arguments[0];
            var value = vehicle.GetData(key);
            this.chatBox.OutputTo(args.Player, $"Vehicle data {key} = {value}");
        };

        this.commandService.AddCommand("hot").Triggered += (source, args) =>
        {
            // command for testing, use hot reload to write code and apply during a running debug session
            var x = this;
        };

        this.commandService.AddCommand("clothes").Triggered += (source, args) =>
        {
            var CJ0 = new Ped(0, new Vector3(0, -14, 3)).AssociateWith(this.server);
            var CJ1 = new Ped(0, new Vector3(2, -14, 3)).AssociateWith(this.server);
            var CJ2 = new Ped(0, new Vector3(4, -14, 3)).AssociateWith(this.server);
            var CJ3 = new Ped(0, new Vector3(6, -14, 3)).AssociateWith(this.server);
            var CJ4 = new Ped(0, new Vector3(8, -14, 3)).AssociateWith(this.server);
            var CJ5 = new Ped(0, new Vector3(10, -14, 3)).AssociateWith(this.server);
            var CJ6 = new Ped(0, new Vector3(12, -14, 3)).AssociateWith(this.server);
            var CJ7 = new Ped(0, new Vector3(14, -14, 3)).AssociateWith(this.server);
            var CJ8 = new Ped(0, new Vector3(16, -14, 3)).AssociateWith(this.server);
            var CJ9 = new Ped(0, new Vector3(18, -14, 3)).AssociateWith(this.server);
            var CJ10 = new Ped(0, new Vector3(20, -14, 3)).AssociateWith(this.server);
            var CJ11 = new Ped(0, new Vector3(22, -14, 3)).AssociateWith(this.server);
            var CJ12 = new Ped(0, new Vector3(24, -14, 3)).AssociateWith(this.server);
            var CJ13 = new Ped(0, new Vector3(26, -14, 3)).AssociateWith(this.server);
            var CJ14 = new Ped(0, new Vector3(28, -14, 3)).AssociateWith(this.server);
            var CJ15 = new Ped(0, new Vector3(30, -14, 3)).AssociateWith(this.server);
            var CJ16 = new Ped(0, new Vector3(32, -14, 3)).AssociateWith(this.server);
            var CJ17 = new Ped(0, new Vector3(34, -14, 3)).AssociateWith(this.server);
            Task.Run(async () =>
            {
                int i = 0;
                while (true)
                {
                    await Task.Delay(1000);
                    CJ0.Clothing.Shirt = (byte)(i % ClothingConstants.ShirtsCount);
                    CJ1.Clothing.Head = (byte)(i % ClothingConstants.HeadsCount);
                    CJ2.Clothing.Trousers = (byte)(i % ClothingConstants.TrousersCount);
                    CJ3.Clothing.Shoes = (byte)(i % ClothingConstants.ShoesCount);
                    CJ4.Clothing.TattoosLeftUpperArm = (byte)(i % ClothingConstants.TattoosLeftUpperArmCount);
                    CJ5.Clothing.TattoosLeftLowerArm = (byte)(i % ClothingConstants.TattoosLeftLowerArmCount);
                    CJ6.Clothing.TattoosRightUpperArm = (byte)(i % ClothingConstants.TattoosRightUpperArmCount);
                    CJ7.Clothing.TattoosRightLowerArm = (byte)(i % ClothingConstants.TattoosRightLowerArmCount);
                    CJ8.Clothing.TattoosBack = (byte)(i % ClothingConstants.TattoosBackCount);
                    CJ9.Clothing.TattoosLeftChest = (byte)(i % ClothingConstants.TattoosLeftChestCount);
                    CJ10.Clothing.TattoosRightChest = (byte)(i % ClothingConstants.TattoosRightChestCount);
                    CJ11.Clothing.TattoosStomach = (byte)(i % ClothingConstants.TattoosStomachCount);
                    CJ12.Clothing.TattoosLowerBack = (byte)(i % ClothingConstants.TattoosLowerBackCount);
                    CJ13.Clothing.Necklace = (byte)(i % ClothingConstants.NecklaceCount);
                    CJ14.Clothing.Watch = (byte)(i % ClothingConstants.WatchesCount);
                    CJ15.Clothing.Glasses = (byte)(i % ClothingConstants.GlassesCount);
                    CJ16.Clothing.Hat = (byte)(i % ClothingConstants.HatsCount);
                    CJ17.Clothing.Extra = (byte)(i % ClothingConstants.ExtraCount);
                    i++;
                }
            });
        };

        this.commandService.AddCommand("upgrade").Triggered += (source, args) =>
        {
            this.Flash!.Upgrades.Spoiler = VehicleUpgradeSpoiler.Alien;
            this.Stratum!.Upgrades.Spoiler = VehicleUpgradeSpoiler.Alien;
            this.Sultan!.Upgrades.Spoiler = VehicleUpgradeSpoiler.Alien;
            this.Elegy!.Upgrades.Spoiler = VehicleUpgradeSpoiler.Alien;
            this.Jester!.Upgrades.Spoiler = VehicleUpgradeSpoiler.Alien;
            this.Uranus!.Upgrades.Spoiler = VehicleUpgradeSpoiler.Alien;
            this.Flash!.Upgrades.Wheels = VehicleUpgradeWheel.Cutter;
            this.Stratum!.Upgrades.Wheels = VehicleUpgradeWheel.Cutter;
            this.Sultan!.Upgrades.Wheels = VehicleUpgradeWheel.Cutter;
            this.Elegy!.Upgrades.Wheels = VehicleUpgradeWheel.Cutter;
            this.Jester!.Upgrades.Wheels = VehicleUpgradeWheel.Cutter;
            this.Uranus!.Upgrades.Wheels = VehicleUpgradeWheel.Cutter;

            this.Club!.Upgrades.Hood = VehicleUpgradeHood.LeftSquare;
            this.Club!.Upgrades.Vent = VehicleUpgradeVent.FuryScoop;
            this.Club!.Upgrades.Sideskirt = VehicleUpgradeSideskirt.Left;
            this.Club!.Upgrades.Lamps = VehicleUpgradeLamp.SquareFog;
            this.Slamvan!.Upgrades.FrontBullbar = VehicleUpgradeFrontBullbar.Slamin;
            this.Slamvan!.Upgrades.RearBullbar = VehicleUpgradeRearBullbar.Chrome;
            this.Remmington!.Upgrades.Misc = VehicleUpgradeMisc.BullbarChromeBars;
            this.Sultan!.Upgrades.Roof = VehicleUpgradeRoof.AlienRoofVent;
            this.Sultan!.Upgrades.Nitro = VehicleUpgradeNitro.x10;
            this.Sultan!.Upgrades.HasHydraulics = true;
            this.Sultan!.Upgrades.HasStereo = true;
            this.Sultan!.Upgrades.Exhaust = VehicleUpgradeExhaust.Alien;
            this.Sultan!.Upgrades.FrontBumper = VehicleUpgradeFrontBumper.Alien;
            this.Sultan!.Upgrades.RearBumper = VehicleUpgradeRearBumper.Alien;

            this.chatBox.OutputTo(args.Player, "Upgrades applied", Color.YellowGreen);
        };

        this.commandService.AddCommand("nomorebumpers").Triggered += (source, args) =>
        {
            this.Sultan!.Upgrades.FrontBumper = VehicleUpgradeFrontBumper.None;
            this.Sultan!.Upgrades.RearBumper = VehicleUpgradeRearBumper.None;

            this.chatBox.OutputTo(args.Player, "Bumpers removed", Color.YellowGreen);
        };

        this.commandService.AddCommand("xflow").Triggered += (source, args) =>
        {
            this.Sultan!.Upgrades.FrontBumper = VehicleUpgradeFrontBumper.XFlow;
            this.Sultan!.Upgrades.RearBumper = VehicleUpgradeRearBumper.XFlow;

            this.chatBox.OutputTo(args.Player, "X-Flow Bumpers installed", Color.YellowGreen);
        };

        this.commandService.AddCommand("paintjob").Triggered += (source, args) =>
        {
            if (args.Player.Vehicle != null && args.Arguments.Any() && byte.TryParse(args.Arguments.First(), out var paintjob))
                args.Player.Vehicle.PaintJob = paintjob;
        };

        this.commandService.AddCommand("ammoInClip").Triggered += (source, args) =>
        {
            if (!args.Arguments.Any() || !short.TryParse(args.Arguments.First(), out var ammoInClip))
                return;

            if (args.Player.CurrentWeapon == null)
                return;

            var weapon = args.Player.CurrentWeapon.Type;
            var config = this.weaponConfigurationService.GetWeaponConfiguration(weapon);
            config.MaximumClipAmmo = ammoInClip;
            this.weaponConfigurationService.SetWeaponConfigurationFor(weapon, config, args.Player);
        };

        this.commandService.AddCommand("areCollisionsEnabled").Triggered += (source, args) =>
        {
            var testobj = new WorldObject(1337, new Vector3(0, 5, 3));
            testobj.AssociateWith(this.server);
            testobj.IsCallPropagationEnabled = false;
            var testobj2 = new WorldObject(1337, new Vector3(0, 7, 3));
            testobj2.AssociateWith(this.server);
            testobj2.AreCollisionsEnabled = true;
        };

        var table = new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            ["x"] = 5.5f,
            ["y"] = "ążćźółń",
            ["z"] = new LuaValue(new Dictionary<LuaValue, LuaValue>()
            {
                ["x"] = 5.5f,
                ["y"] = "ążćźółń",
                ["z"] = new LuaValue(new Dictionary<LuaValue, LuaValue>()
                {
                    ["x"] = 5.5f,
                    ["y"] = "ążćźółń",
                    ["z"] = new LuaValue(new Dictionary<LuaValue, LuaValue>() { }),
                    ["w"] = false,
                    ["player"] = (uint)123,
                    ["vector2array"] = LuaValue.ArrayFromVector(new Vector2(1, 3)),
                }),
                ["w"] = false,
                ["player"] = (uint)123,
                ["vector2array"] = LuaValue.ArrayFromVector(new Vector2(1, 3)),
                ["vector2"] = new Vector2(1, 3),
                ["vector3"] = new Vector3(1, 3, 3),
            }),
            ["w"] = false,
            ["player"] = (uint)123,
            ["vector2array"] = LuaValue.ArrayFromVector(new Vector2(1, 3)),
        });

        var integer = new LuaValue(123);
        var doublev = new LuaValue(123.45d);
        var floatv = new LuaValue(123.45f);
        var stringv = new LuaValue("i'm string");
        var debugView = table.DebugView;

        var sequentialTableValue = LuaValue.IsSequentialTableValue(new Dictionary<LuaValue, LuaValue>
        {
            [3] = true,
            [1] = true,
            [2] = true,
        });

        var nonSequentialTableValue1 = LuaValue.IsSequentialTableValue(new Dictionary<LuaValue, LuaValue>
        {
            [1] = true,
            [2] = true,
            [3] = true,
            [5] = true,
        });

        var nonSequentialTableValue2 = LuaValue.IsSequentialTableValue(new Dictionary<LuaValue, LuaValue>
        {
            [2] = true,
            [3] = true,
            [4] = true,
        });
    }

    private void OnPlayerJoin(CustomPlayer player)
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
        player.SetTransferBoxVisible(false);
        player.WantedLevel = 4;
        //player.ForceMapVisible(true);
        //player.ToggleAllControls(false, true, true);

        player.Kicked += (player, args) =>
        {
            this.logger.LogWarning("{playerName} has been kicked, reason: {reason}", player.Name, args.Reason);
        };

        player.ScreenshotTaken += HandlePlayerScreenshot;

        player.Weapons.Add(new Weapon(WeaponId.Ak47, 500));
        player.Weapons.Add(new Weapon(WeaponId.Tec9, 500));
        player.Weapons.Add(new Weapon(WeaponId.Sniper, 500));
        player.Weapons.Add(new Weapon(WeaponId.Deagle, 500));
        player.Weapons.Add(new Weapon(WeaponId.Golfclub, 1));
        player.Weapons.Add(new Weapon(WeaponId.Satchel, 25));
        player.Weapons.Remove(WeaponId.Tec9);
        player.Weapons.Remove(WeaponId.Sniper);
        player.Weapons.First(weapon => weapon.Type == WeaponId.Deagle).Ammo -= 200;
        player.Weapons.First(weapon => weapon.Type == WeaponId.Ak47).Ammo = 750;
        player.Weapons.First(weapon => weapon.Type == WeaponId.Ak47).AmmoInClip = 25;

        this.testResource?.StartFor(player);
        this.secondTestResource?.StartFor(player);
        this.thirdTestResource?.StartFor(player);

        this.HandlePlayerSubscriptions(player);

        player.AcInfoReceived += (o, args) =>
        {
            this.logger.LogInformation("ACInfo for {playerName} detectedACList:{acList} d3d9Size: {D3D9Size} d3d9SHA256: {D3D9SHA256}", player.Name, string.Join(",", args.DetectedACList), args.D3D9Size, args.D3D9SHA256);
        };

        player.DiagnosticInfoReceived += (o, args) =>
        {
            this.logger.LogInformation("DIAGNOSTIC: {playerName} #{level} {message}", player.Name, args.Level, args.Message);
        };

        player.ModInfoReceived += (o, args) =>
        {
            this.logger.LogInformation("Player: {playerName} ModInfo:", player.Name);
            foreach (var item in args.ModInfoItems)
            {
                this.logger.LogInformation("\t{name} - md5: {md5}", item.Name, item.LongMd5);
            }
        };

        player.NetworkStatusReceived += (o, args) =>
        {
            switch (args.PlayerNetworkStatus)
            {
                case Packets.Enums.PlayerNetworkStatusType.InterruptionBegan:
                    this.logger.LogInformation("(packets from {name}) interruption began {ticks} ticks ago", o.Name, args.Ticks);
                    break;
                case Packets.Enums.PlayerNetworkStatusType.InterruptionEnd:
                    this.logger.LogInformation("(packets from {name}) interruption began {ticks} ticks ago and has just ended", o.Name, args.Ticks);
                    break;
            }
        };

        player.TeamChanged += (thePlayer, args) =>
        {
            this.logger.LogDebug("{playerName} Joined {teamName} team!", thePlayer.Name, thePlayer.Team?.TeamName);
        };

        player.TargetChanged += (thePlayer, args) =>
        {
            if (args.NewValue != null && args.NewValue is Vehicle vehicle)
            {
                if (vehicle.Model == (ushort)VehicleModel.Rhino)
                    this.logger.LogDebug("{playerName} Changed target rhino", thePlayer.Name);
            }
        };

        player.Team = this.slipeDevsTeam;

        bool jetpackBindEnabled = true;
        player.SetBind("j", KeyState.Down);
        player.SetBind("h", KeyState.Down);
        player.BindExecuted += (Player sender, PlayerBindExecutedEventArgs e) =>
        {
            if (e.Key == "j")
            {
                player.HasJetpack = !player.HasJetpack;
                if (player.HasJetpack)
                    this.logger.LogInformation("{name} put on a jetpack!", sender.Name);
                else
                    this.logger.LogInformation("{name} pulled off his jetpack!", sender.Name);
            } else if (e.Key == "h")
            {
                jetpackBindEnabled = !jetpackBindEnabled;
                player.SetBind("j", jetpackBindEnabled ? KeyState.Down : KeyState.None);
                if (jetpackBindEnabled)
                    this.logger.LogInformation("Bind enabled");
                else
                    this.logger.LogInformation("Bind disabled");
            }
        };
    }

    private void HandlePlayerSubscriptions(Player player)
    {
        var otherPlayers = this.elementCollection.GetByType<Player>().Where(x => x != player);
        foreach (var otherPlayer in otherPlayers)
        {
            otherPlayer.SubscribeTo(player);
            player.SubscribeTo(otherPlayer);
        }


        player.CommandEntered += (o, args) =>
        {
            Player? otherPlayer;
            switch (args.Command)
            {
                case "sub":
                    otherPlayer = this.elementCollection
                        .GetByType<Player>()
                        .SingleOrDefault(x => x.Name == args.Arguments[0]);

                    if (otherPlayer != null)
                        player.SubscribeTo(otherPlayer);
                    break;
                case "unsub":
                    otherPlayer = this.elementCollection
                        .GetByType<Player>()
                        .SingleOrDefault(x => x.Name == args.Arguments[0]);

                    if (otherPlayer != null)
                        player.UnsubscribeFrom(otherPlayer);
                    break;
            }
        };

        player.IsOnFireChanged += (o, args) =>
        {
            if (o is Player player)
            {
                chatBox.OutputTo(player, $"In on fire: {args.NewValue}");
            }
        };

        player.IsInWaterChanged += (o, args) =>
        {
            if (o is Player player)
            {
                chatBox.OutputTo(player, $"In water: {args.NewValue}");
            }
        };
    }

    private void HandlePlayerScreenshot(object? o, ScreenshotEventArgs e)
    {
        if (e.Stream != null)
        {
            using FileStream file = new FileStream($"screenshot_${e.Tag}.jpg", FileMode.Create, FileAccess.Write);
            e.Stream.CopyTo(file);
        } else
        {
            Player? player = (Player?)o;
            this.logger.LogWarning("Failed to take a screenshot ({tag}) of player: {playerName}, reason: {errorMessage}", e.Tag, player?.Name, e.ErrorMessage);
        }
    }

    private void HandleSampleEvent(LuaEvent luaEvent)
    {
        var sampleValue = new SampleLuaValue();
        sampleValue.Parse(luaEvent.Parameters.First());

        this.logger.LogInformation("{event}", JsonConvert.SerializeObject(sampleValue));
    }

    private void TriggerTestEvent(Player player)
    {
        var table = new LuaValue(new Dictionary<LuaValue, LuaValue>()
        {
            ["x"] = 5.5f,
            ["y"] = "ążćźółń",
            ["z"] = new LuaValue(new Dictionary<LuaValue, LuaValue>() { }),
            ["w"] = false,
            ["player"] = player.Id,
            ["vector2array"] = LuaValue.ArrayFromVector(new Vector2(1,3)),
        });
        table.TableValue?.Add("self", table);

        this.luaService.TriggerEventFor(player, "Slipe.Test.ClientEvent", this.root, "String value", true, 23, table);
    }
}
