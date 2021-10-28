using Microsoft.Extensions.Logging;
using RenderWareIo;
using SlipeServer.Physics.Builders;
using SlipeServer.Physics.Enum;
using SlipeServer.Physics.Worlds;
using System;
using System.IO;

namespace SlipeServer.Physics.Services
{
    public partial class PhysicsService
    {
        private readonly ILogger logger;

        public PhysicsService(ILogger logger)
        {
            this.logger = logger;
        }

        public PhysicsWorld CreateEmptyPhysicsWorld()
        {
            return new PhysicsWorld();
        }

        public PhysicsWorld CreatePhysicsWorldFromGtaDirectory(string directory, string datFile = "gta.dat", PhysicsModelLoadMode loadMode = PhysicsModelLoadMode.LowDetail)
        {
            string datFilepath = Path.Join(directory, $"data/{datFile}");
            string imgFilepath = Path.Join(directory, "models/gta3.img");

            return CreatePhysicsWorldFromDat(directory, new DatFile(datFilepath), new ImgFile(imgFilepath), loadMode);
        }

        public PhysicsWorld CreatePhysicsWorldFromDat(string root, string datFilepath, string imgFilepath, PhysicsModelLoadMode loadMode = PhysicsModelLoadMode.LowDetail)
        {
            return CreatePhysicsWorldFromDat(root, new DatFile(datFilepath), new ImgFile(imgFilepath), loadMode);
        }

        public PhysicsWorld CreatePhysicsWorldFromDat(string root, DatFile datFile, ImgFile imgFile, PhysicsModelLoadMode loadMode = PhysicsModelLoadMode.LowDetail)
        {
            return CreateWorld(builder =>
            {
                builder.AddImg(imgFile.Img);

                foreach (var ide in datFile.Dat.Ides)
                {
                    var path = Path.Join(root, ide).TrimEnd('\r');
                    if (File.Exists(path))
                        builder.AddIde(Path.GetFileNameWithoutExtension(path), new IdeFile(path).Ide, loadMode);
                    else
                        this.logger.LogWarning($"Unable to find .ide file {path}");
                }

                foreach (var ipl in datFile.Dat.Ipls)
                {
                    var path = Path.Join(root, ipl).TrimEnd('\r');
                    if (File.Exists(path))
                        builder.AddIpl(new IplFile(path).Ipl, loadMode);
                    else
                        this.logger.LogWarning($"Unable to find .ipl file {path}");
                }

                foreach (var ipl in imgFile.Img.IplFiles)
                {
                    builder.AddIpl(new BinaryIplFile(imgFile.Img.DataEntries[ipl].Data).BinaryIpl, loadMode);
                }
            });
        }

        public PhysicsWorld CreateWorld(Action<PhysicsWorldBuilder> builderAction)
        {
            var builder = new PhysicsWorldBuilder();

            builderAction(builder);

            return builder.Build();
        }
    }
}
