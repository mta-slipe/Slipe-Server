using BepuPhysics.Collidables;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using SlipeServer.Physics.Entities;
using SlipeServer.Physics.Enum;
using SlipeServer.Physics.Services;
using SlipeServer.Physics.Worlds;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Services;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SlipeServer.Console.Logic;

public class PhysicsTestLogic
{
    private readonly MtaServer server;
    private readonly IElementCollection elementCollection;
    private readonly PhysicsService physicsService;
    private readonly CommandService commandService;
    private readonly ILogger logger;

    private PhysicsWorld? physicsWorld;
    private StaticPhysicsElement? ufoInnMesh1;
    private StaticPhysicsElement? ufoInnMesh2;
    private StaticPhysicsElement? army;
    private CompoundPhysicsMesh cylinder;
    private CompoundPhysicsMesh drum;
    private ConvexPhysicsMesh ball;

    public PhysicsTestLogic(
        MtaServer server,
        IElementCollection elementCollection,
        PhysicsService physicsService,
        CommandService commandService,
        ILogger logger)
    {
        this.server = server;
        this.elementCollection = elementCollection;
        this.physicsService = physicsService;
        this.commandService = commandService;
        this.logger = logger;

        this.commandService.AddCommand("physicsreset").Triggered += (source, args) => Reset();
        this.commandService.AddCommand("physics").Triggered += (source, args) => InitEmpty();
        this.commandService.AddCommand("physicshighres").Triggered += (source, args) => InitFullDff();
        this.commandService.AddCommand("physicslowres").Triggered += (source, args) => InitFullCol();
    }

    private void Reset()
    {
        this.physicsWorld?.Stop();
        this.physicsWorld?.Dispose();
        this.physicsWorld = null;
    }

    private void InitEmpty()
    {
        this.physicsWorld = this.physicsService.CreateEmptyPhysicsWorld(new Vector3(0, 0, -1f));
        this.Init();
    }

    private void InitFullDff()
    {
        string? gtaDirectory = GetGtasaDirectory();
        this.physicsWorld = this.physicsService.CreatePhysicsWorldFromGtaDirectory(gtaDirectory ?? "gtasa", "gta.dat", PhysicsModelLoadMode.Dff, builderAction: (builder) =>
        {
            builder.SetGravity(Vector3.UnitZ * -1.0f);
        });
        this.Init();
    }

    private void InitFullCol()
    {
        string? gtaDirectory = GetGtasaDirectory();
        this.physicsWorld = this.physicsService.CreatePhysicsWorldFromGtaDirectory(gtaDirectory ?? "gtasa", "gta.dat", PhysicsModelLoadMode.Col, builderAction: (builder) =>
        {
            builder.SetGravity(Vector3.UnitZ * -1.0f);
        });
        this.Init();
    }

    private void Init()
    {
        if (this.physicsWorld == null)
            return;

        try
        {
            var img = this.physicsWorld.LoadImg(Path.Join(GetGtasaDirectory(), @"models\gta3.img"));
            var ufoInnMeshes = this.physicsWorld.CreateMesh(img, "countn2_20.col", "des_ufoinn");
            this.ufoInnMesh1 = (StaticPhysicsElement)this.physicsWorld.AddStatic(ufoInnMeshes.Item1!, Vector3.Zero, Quaternion.Identity);
            this.ufoInnMesh2 = (StaticPhysicsElement)this.physicsWorld.AddStatic(ufoInnMeshes.Item2!, Vector3.Zero, Quaternion.Identity);

            var inn = new WorldObject(Server.Enums.ObjectModel.Desufoinn, new Vector3(50, 0, 4.5f))
            {
                Rotation = new Vector3(0, 0, 90)
            }.AssociateWith(this.server);
            this.ufoInnMesh1.CoupleWith(inn);
            this.ufoInnMesh2.CoupleWith(inn);

            var armyMesh = this.physicsWorld.CreateMesh(img, "army.dff");
            var armyRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -0.5f * MathF.PI);
            this.army = (StaticPhysicsElement)this.physicsWorld.AddStatic(armyMesh, new Vector3(54, -22.5f, 1), armyRotation);

            this.cylinder = this.physicsWorld.CreateCylinder(0.35f, 1.8f);
            this.drum = this.physicsWorld.CreateCylinder(0.35f, 1.1f);
            this.ball = this.physicsWorld.CreateSphere(0.25f);

            this.commandService.AddCommand("ray").Triggered += HandleRayCommand;
            this.commandService.AddCommand("rayme").Triggered += HandleRayMeCommand;
            this.commandService.AddCommand("raymedown").Triggered += HandleRayMeDownCommand;
            this.commandService.AddCommand("ball").Triggered += HandleBallCommand;
            this.commandService.AddCommand("cylinder").Triggered += HandleCylinderCommand;
            this.commandService.AddCommand("cylinderstack").Triggered += HandleCylinderStackCommand;
            this.commandService.AddCommand("startsim").Triggered += HandleStartSimCommand;
            this.commandService.AddCommand("stopsim").Triggered += HandleStopSimCommand;
            this.commandService.AddCommand("heightmap").Triggered += (s, a) => GenerateRaycastedHeightMap();

            this.server.PlayerJoined += HandlePlayerJoin;
            foreach (var player in this.elementCollection.GetByType<Player>())
                HandlePlayerJoin(player);
        }
        catch (IOException)
        {
            this.logger.LogWarning("Failed to open gta3.img\nIf you're running MTA consider creating a copy of your GTA:SA directory and creating an environment variable named \"Slipe.GtaSAPath\" pointing to the path");
        }
    }

    private void HandlePlayerJoin(Player player)
    {
        if (this.physicsWorld == null)
            return;

        var playerElement = this.physicsWorld.AddKinematicBody(this.cylinder, player.Position, player.Rotation.ToQuaternion());
        playerElement.CoupleWith(player, Vector3.Zero, new Vector3(0, 0, 0));

        player.Disconnected += (_, _) => this.physicsWorld.Destroy(playerElement);
    }

    private void HandleRayCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        GenerateRaycastedImage(new Vector3(50, 0, 3));
    }

    private void HandleRayMeCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        GenerateRaycastedImage(e.Player.Position);
    }

    private void HandleRayMeDownCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        GenerateRaycastedHeightMap(e.Player.Position, new Vector2(25, 25), new Vector2(1024, 1024));
    }

    private void HandleBallCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        if (this.physicsWorld == null)
            return;

        var physicsBall = this.physicsWorld.AddDynamicBody(this.ball, e.Player.Position, Quaternion.Identity, 1);
        var ball = new WorldObject(2114, e.Player.Position + Vector3.UnitZ * 2).AssociateWith(this.server);
        physicsBall.CoupleWith(ball);
    }

    private void HandleCylinderCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        if (this.physicsWorld == null)
            return;

        var physicsCylinder = this.physicsWorld.AddDynamicBody(this.drum, e.Player.Position, Quaternion.Identity, 30, 0.35f);
        var cylinder = new WorldObject(2062, e.Player.Position + Vector3.UnitZ * 2).AssociateWith(this.server);
        physicsCylinder.CoupleWith(cylinder);
    }

    private void HandleCylinderStackCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        if (this.physicsWorld == null)
            return;

        for (int i = 0; i < 10; i++)
        {
            var physicsCylinder = this.physicsWorld.AddDynamicBody(this.drum, e.Player.Position + Vector3.UnitZ * i * 2.0f, Quaternion.Identity, 30, 0.35f);
            var cylinder = new WorldObject(2062, e.Player.Position + Vector3.UnitZ * (i * 2.0f + 2)).AssociateWith(this.server);
            physicsCylinder.CoupleWith(cylinder);
        }
    }

    private void HandleStartSimCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.physicsWorld?.Start(5);
    }

    private void HandleStopSimCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
    {
        this.physicsWorld?.Stop();
    }

    private void GenerateRaycastedImage(Vector3 position)
    {
        if (this.physicsWorld == null)
            return;

        static IEnumerable<Color> GetColors()
        {
            yield return Color.Red;
            yield return Color.Green;
            yield return Color.Blue;

            var random = new Random();
            while (true)
            {
                yield return Color.FromArgb(255, (byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
            }
        }

        var width = 1200;
        var height = 1200;
        var depth = 50f;
        var pixelSize = 0.025f;

        var direction = new Vector3(0, 1, 0);

        var colors = GetColors().GetEnumerator();
        var colorPerCollidable = new Dictionary<CollidableReference, Color>();

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using var output = new Bitmap(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var from = position + new Vector3(x * pixelSize - 0.5f * width * pixelSize, -25, y * pixelSize - 0.5f * width * pixelSize);
                var hit = this.physicsWorld.RayCast(from, direction, depth);
                if (hit.HasValue)
                {
                    var intensity = 255 - (byte)(hit.Value.distance / depth * 255);
                    if (!colorPerCollidable.ContainsKey(hit.Value.Collidable))
                    {
                        colors.MoveNext();
                        colorPerCollidable[hit.Value.Collidable] = colors.Current;
                    }
                    var color = Color.FromArgb(intensity, colorPerCollidable[hit.Value.Collidable]);
                    output.SetPixel(x, height - y - 1, color);
                }
            }
        }
        stopwatch.Stop();
        var time = stopwatch.Elapsed;
        this.logger.LogInformation("Raycast image generated in {time}ms", time.TotalMilliseconds);
        output.Save("rayresult.png", ImageFormat.Png);
    }

    private void GenerateRaycastedHeightMap()
    {
        var width = 6000;
        var height = 6000;
        var depth = 500f;

        var direction = new Vector3(0, 0, -1);

        using var output = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var from = new Vector3(x - width / 2, y - height / 2, 400);
                var hit = this.physicsWorld!.RayCast(from, direction, depth);
                if (hit.HasValue)
                {
                    var max = 256 * 3;
                    var intensity = hit.Value.distance / depth * max;
                    var color = Color.FromArgb(255, (int)Math.Clamp(intensity, 0, 255), (int)Math.Clamp(intensity - 256, 0, 255), (int)Math.Clamp(intensity - 512, 0, 255));
                    output.SetPixel(x, height - y - 1, color);
                }
            }
        }
        this.logger.LogInformation($"Heightmap generated");

        output.Save("heightmap.png", ImageFormat.Png);
    }

    private void GenerateRaycastedHeightMap(Vector3 center, Vector2 dimensions, Vector2 resolution)
    {
        var width = resolution.X;
        var height = resolution.Y;

        var min = center.Z - 5;
        var max = center.Z + 5;

        var depth = max - min;

        var direction = new Vector3(0, 0, -1);

        using var output = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var from = center + new Vector3(x / width * dimensions.X * 2 - dimensions.X, y / height * dimensions.Y * 2 - dimensions.Y, max);
                var hit = this.physicsWorld!.RayCast(from, direction, depth);
                if (hit.HasValue)
                {
                    var level = from + hit.Value.distance * direction;
                    var intensity = (level.Z - min) / (max - min) * 255 * 3;
                    var color = Color.FromArgb(255, (int)Math.Clamp(intensity, 0, 255), (int)Math.Clamp(intensity - 256, 0, 255), (int)Math.Clamp(intensity - 512, 0, 255));
                    output.SetPixel(x, (int)(height) - y - 1, color);
                }
            }
        }
        this.logger.LogInformation($"Heightmap generated");

        output.Save("localheightmap.png", ImageFormat.Png);
    }

    private string? GetGtasaDirectory()
    {
        string? gtaDirectory = Environment.GetEnvironmentVariable("Slipe.GtaSAPath");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && gtaDirectory == null)
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Multi Theft Auto: San Andreas All\Common");
            gtaDirectory = key?.GetValue("GTA:SA Path")?.ToString();
        }

        return gtaDirectory;
    }
}
