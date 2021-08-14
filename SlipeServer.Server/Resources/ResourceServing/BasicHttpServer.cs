using Force.Crc32;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Resources.ResourceServing
{
    public class BasicHttpServer: IResourceServer
    {
        private readonly HttpListener httpListener;
        private readonly string rootDirectory;
        private readonly Configuration configuration;
        private readonly ILogger logger;
        private readonly string httpAddress;
        private bool isRunning;

        public BasicHttpServer(Configuration configuration, ILogger logger)
        {
            this.httpAddress = $"http://{configuration.HttpHost}:{configuration.HttpPort}/";
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(this.httpAddress);

            this.isRunning = false;
            this.rootDirectory = configuration.ResourceDirectory;
            this.configuration = configuration;
            this.logger = logger;
        }

        public void Start()
        {
            if (this.isRunning)
            {
                return;
            }

            this.isRunning = true;
            try
            {
                this.httpListener.Start();
            }
            catch(HttpListenerException exception)
            {
                if (exception.Message == "Access is denied." && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string command = $@"netsh http add urlacl url=http://{this.configuration.HttpHost}:{this.configuration.HttpPort}/ sddl=D:(A;;GX;;;S-1-1-0)";
                    throw new Exception($"Could not start http server on address {this.httpAddress}\n{exception.Message}\nYou might need to run the following command in an administrator command prompt: \n{command}", exception);
                } else
                {
                    throw new Exception($"Could not start http server on address {this.httpAddress}\n{exception.Message}", exception);
                }
            }

            Task.Run(async () =>
            {
                while (this.isRunning)
                {
                    var context = await this.httpListener.GetContextAsync();

                    _ = Task.Run(async () =>
                    {
                        var path = Path.Join(this.rootDirectory, context.Request.Url?.LocalPath);

                        if (File.Exists(path))
                        {
                            var content = await File.ReadAllBytesAsync(path);

                            await context.Response.OutputStream.WriteAsync(content.AsMemory(0, content.Length));
                            context.Response.StatusCode = 200;
                        } else
                        {
                            context.Response.StatusCode = 404;
                            this.logger.LogWarning($"404 encountered while trying to download {context.Request.Url?.LocalPath}", null);
                        }

                        context.Response.Close();
                    });
                }

                this.httpListener.Stop();
            });
        }

        public void Stop()
        {
            this.isRunning = false;
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
                    var checksum = Crc32Algorithm.Compute(content);

                    string fileName = Path.GetRelativePath(Path.Join(this.rootDirectory, resource), file);
                    resourceFiles.Add(new ResourceFile()
                    {
                        Name = fileName,
                        AproximateSize = content.Length,
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
            IEnumerable<ResourceFile> resourceFiles = Array.Empty<ResourceFile>();
            foreach (var directory in Directory.GetDirectories(this.rootDirectory))
            {
                resourceFiles = resourceFiles.Concat(GetResourceFiles(directory));
            }

            return resourceFiles;
        }
    }
}
