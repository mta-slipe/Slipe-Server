using Microsoft.Extensions.Logging;
using RenderWareIo;
using SlipeServer.Physics.Builders;
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

        public PhysicsWorld CreatePhysicsWorldFromGtaDirectory(string directory, string datFile = "gta.dat")
        {
            string datFilepath = Path.Join(directory, $"data/{datFile}");
            string imgFilepath = Path.Join(directory, "models/gta3.img");

            return CreatePhysicsWorldFromDat(directory, new DatFile(datFilepath), new ImgFile(imgFilepath));
        }

        public PhysicsWorld CreatePhysicsWorldFromDat(string root, string datFilepath, string imgFilepath)
        {
            return CreatePhysicsWorldFromDat(root, new DatFile(datFilepath), new ImgFile(imgFilepath));
        }

        public PhysicsWorld CreatePhysicsWorldFromDat(string root, DatFile datFile, ImgFile imgFile)
        {
            return CreateWorld(builder =>
            {
                builder.AddImg(imgFile.Img);

                foreach (var ide in datFile.Dat.Ides)
                {
                    var path = Path.Join(root, ide).TrimEnd('\r');
                    if (File.Exists(path))
                        builder.AddIde(new IdeFile(path).Ide);
                    else
                        this.logger.LogWarning($"Unable to find .ide file {path}");
                }

                foreach (var ipl in datFile.Dat.Ipls)
                {
                    var path = Path.Join(root, ipl).TrimEnd('\r');
                    if (File.Exists(path))
                        builder.AddIpl(new IplFile(path).Ipl);
                    else
                        this.logger.LogWarning($"Unable to find .ipl file {path}");
                }

                foreach (var ipl in imgFile.Img.IplFiles)
                {
                    builder.AddIpl(new BinaryIplFile(imgFile.Img.DataEntries[ipl].Data).BinaryIpl);
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
