using Force.Crc32;
using Microsoft.Extensions.Logging;
using MtaServer.Packets.Structs;
using MtaServer.Server.Elements.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MtaServer.Server.ResourceServing
{
    public class BasicHttpServer: IResourceServer
    {
        private readonly HttpListener httpListener;
        private readonly string rootDirectory;
        private readonly ILogger logger;
        private bool running;

        public BasicHttpServer(Configuration configuration, ILogger logger)
        {
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add($"http://{configuration.HttpHost}:{configuration.HttpPort}/");

            this.running = false;
            this.rootDirectory = configuration.ResourceDirectory;
            this.logger = logger;
        }

        public void Start()
        {
            if (this.running)
            {
                return;
            }

            this.running = true;
            this.httpListener.Start();

            Task.Run(async () =>
            {
                while (running)
                {
                    var context = await this.httpListener.GetContextAsync();
                    Console.WriteLine(context.Request.Url.AbsoluteUri);

                    _ = Task.Run(async () =>
                    {
                        var path = Path.Join(this.rootDirectory, context.Request.Url.LocalPath);

                        if (File.Exists(path))
                        {
                            var content = await File.ReadAllBytesAsync(path);

                            await context.Response.OutputStream.WriteAsync(content, 0, content.Length);
                            context.Response.StatusCode = 200;
                        } else
                        {
                            context.Response.StatusCode = 404;
                            this.logger.LogWarning($"404 encountered while trying to download {context.Request.Url.LocalPath}", null);
                        }

                        context.Response.Close();
                    });
                }

                this.httpListener.Stop();
            });
        }

        public void Stop()
        {
            this.running = false;
        }

        public IEnumerable<ResourceFile> GetResourceFiles(string resource)
        {
            List<ResourceFile> resourceFiles = new List<ResourceFile>();

            using (var md5 = MD5.Create())
            {
                foreach (var file in Directory.GetFiles(Path.Join(this.rootDirectory, resource)))
                {
                    byte[] content = File.ReadAllBytes(file);
                    var hash = md5.ComputeHash(content);
                    var stringHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    var checksum = Crc32Algorithm.Compute(content);

                    string fileName = Path.GetRelativePath(Path.Join(this.rootDirectory, resource), file);
                    resourceFiles.Add(new ResourceFile()
                    {
                        Name = fileName,
                        AproximateSize = 28,
                        IsAutoDownload = false,
                        CheckSum = checksum,
                        FileType = (byte)(fileName.EndsWith(".lua") ? ResourceFileType.ClientScript : ResourceFileType.ClientFile),
                        Md5 = hash
                    });
                }
            }

            return resourceFiles.ToArray();
        }

        public IEnumerable<ResourceFile> GetResourceFiles()
        {
            IEnumerable<ResourceFile> resourceFiles = new ResourceFile[0];
            foreach (var directory in Directory.GetDirectories(this.rootDirectory))
            {
                resourceFiles = resourceFiles.Concat(GetResourceFiles(directory));
            }

            return resourceFiles;
        }
    }
}
