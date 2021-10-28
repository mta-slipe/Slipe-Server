using BepuPhysics.Collidables;
using Microsoft.Win32;
using SlipeServer.Physics.Entities;
using SlipeServer.Physics.Services;
using SlipeServer.Physics.Worlds;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SlipeServer.Console.Logic
{
    public class PhysicsTestLogic
    {
        private readonly MtaServer server;
        private readonly PhysicsWorld physicsWorld;

        private StaticPhysicsElement? ufoInn;
        private StaticPhysicsElement? army;
        private PhysicsMesh cylinder;

        public PhysicsTestLogic(MtaServer server, PhysicsService physicsService, CommandService commandService)
        {
            this.server = server;

            string? gtaDirectory = Environment.GetEnvironmentVariable("Slipe.GtaSAPath");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && gtaDirectory == null)
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Multi Theft Auto: San Andreas All\Common");
                gtaDirectory = key?.GetValue("GTA:SA Path")?.ToString() ;
            }

            //this.physicsWorld = physicsService.CreateEmptyPhysicsWorld();
            this.physicsWorld = physicsService.CreatePhysicsWorldFromGtaDirectory(gtaDirectory ?? "gtasa", "gta.dat");

            server.PlayerJoined += HandlePlayerJoin;
            commandService.AddCommand("ray").Triggered += HandleRayCommand;
            commandService.AddCommand("rayme").Triggered += HandleRayMeCommand;

            Init();
            GenerateRaycastedImage(new Vector3(50, 0, 3));
        }

        private void HandlePlayerJoin(Player player)
        {
            var playerElement = this.physicsWorld.AddStatic(this.cylinder, player.Position, player.Rotation.ToQuaternion());
            playerElement.CoupleWith(player, Vector3.Zero, new Vector3(0, 90, 0));

            player.Disconnected += (_, _) => this.physicsWorld.Destroy(playerElement);
        }

        private void Init()
        {
            var img = this.physicsWorld.LoadImg(@"D:\SteamLibrary\steamapps\common\Grand Theft Auto San Andreas\models\gta3 - Copy.img");
            //var ufoInnMesh = this.physicsWorld.CreateMesh(img, "des_ufoinn.dff");
            var ufoInnMesh = this.physicsWorld.CreateMesh(img, "countn2_20.col", "des_ufoinn");
            this.ufoInn = (StaticPhysicsElement)this.physicsWorld.AddStatic(ufoInnMesh, Vector3.Zero, Quaternion.Identity);

            var inn = new WorldObject(Server.Enums.ObjectModel.Desufoinn, new Vector3(50, 0, 4.5f))
            {
                Rotation = new Vector3(0, 0, 90)
            }.AssociateWith(this.server);
            this.ufoInn.CoupleWith(inn);

            var armyMesh = this.physicsWorld.CreateMesh(img, "army.dff");
            var armyRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -0.5f * MathF.PI);
            this.army = (StaticPhysicsElement)this.physicsWorld.AddStatic(armyMesh, new Vector3(54, -22.5f, 1), armyRotation);

            this.cylinder = this.physicsWorld.CreateCylinder(0.35f, 1.8f);
        }

        private void HandleRayCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
        {
            GenerateRaycastedImage(new Vector3(50, 0, 3));
        }

        private void HandleRayMeCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
        {
            GenerateRaycastedImage(e.Player.Position);
        }

        private void GenerateRaycastedImage(Vector3 position)
        {
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
            Bitmap output = new Bitmap(width, height);
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
            System.Console.WriteLine(time.TotalMilliseconds);

            output.Save("rayresult.png", ImageFormat.Png);
        }
    }
}
