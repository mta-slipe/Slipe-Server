using SlipeServer.Physics.Services;
using SlipeServer.Server.Services;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;

namespace SlipeServer.Console.Logic
{
    public class PhysicsTestLogic
    {
        private readonly PhysicsService physicsService;

        public PhysicsTestLogic(PhysicsService physicsService, CommandService commandService)
        {
            this.physicsService = physicsService;

            Init();
            TryRay();
        }

        private void Init()
        {
            var img = this.physicsService.LoadImg(@"D:\SteamLibrary\steamapps\common\Grand Theft Auto San Andreas\models\gta3.img");
            var ufoInnMesh = this.physicsService.CreateMesh(img, "des_ufoinn.dff");
            var ufoInnRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), 0.5f * MathF.PI);
            var ufoInn = this.physicsService.AddStatic(ufoInnMesh, Vector3.Zero, ufoInnRotation);

            var armyMesh = this.physicsService.CreateMesh(img, "army.dff");
            var armyRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -0.5f * MathF.PI);
            var army = this.physicsService.AddStatic(armyMesh, new Vector3(4, -22.5f, -2), armyRotation);
        }

        private void HandleRayCommand(object? sender, Server.Events.CommandTriggeredEventArgs e)
        {
            TryRay();
        }

        private void TryRay()
        {
            var width = 800;
            var height = 800;
            var depth = 50f;
            var pixelSize = 0.025f;

            var direction = new Vector3(0, 1, 0);

            Bitmap output = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var from = new Vector3(x * pixelSize - 0.5f * width * pixelSize, -25, y * pixelSize - 0.5f * width * pixelSize);
                    var hit = this.physicsService.RayCast(from, direction, depth);
                    if (hit.HasValue)
                    {
                        var distance = 255 - (byte)(hit.Value.distance / depth * 255);
                        output.SetPixel(x, height - y, Color.FromArgb(255, distance, distance, distance));
                    }
                }
            }

            output.Save("rayresult.png", ImageFormat.Png);
        }
    }
}
