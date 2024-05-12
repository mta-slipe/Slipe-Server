using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SlipeServer.Server.Resources.Serving;

/// <summary>
/// Simple HTTP server that hosts the content of the resources directory (as configured in the server configuration). 
/// </summary>
public class BasicHttpServer : IResourceServer
{
    private readonly HttpListener httpListener;
    private readonly string rootDirectory;
    private readonly Configuration configuration;
    private readonly ILogger logger;
    private readonly string httpAddress;
    private readonly Dictionary<string, byte[]> additionalFiles;

    private bool isRunning;

    public BasicHttpServer(MtaServer mtaServer, ILogger logger)
    {
        var configuration = mtaServer.Configuration;
        this.httpAddress = $"http://{configuration.HttpHost}:{configuration.HttpPort}/";
        this.httpListener = new HttpListener();
        this.httpListener.Prefixes.Add(this.httpAddress);
        this.additionalFiles = new();

        this.isRunning = false;
        this.rootDirectory = configuration.ResourceDirectory;
        this.configuration = configuration;
        this.logger = logger;
    }

    public void Start()
    {
        if (this.isRunning)
            return;

        this.isRunning = true;
        try
        {
            this.httpListener.Start();
        }
        catch (HttpListenerException exception)
        {
            if (exception.ErrorCode == 5 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
                _ = Task.Run(() => HandleRequest(context));
            }

            this.httpListener.Stop();
        });
    }

    public void Stop()
    {
        this.isRunning = false;
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        var path = Path.Join(this.rootDirectory, context.Request.Url?.LocalPath);

        if (this.additionalFiles.TryGetValue(path, out var value))
        {
            context.Response.OutputStream.Write(value, 0, value.Length);
            context.Response.StatusCode = 200;
        }
        else if (File.Exists(path))
        {
            using var file = File.Open(path, FileMode.Open, FileAccess.Read);
            await file.CopyToAsync(context.Response.OutputStream);
            context.Response.StatusCode = 200;
        } else
        {
            context.Response.StatusCode = 404;
            this.logger.LogWarning("404 encountered while trying to download {path}", context.Request.Url?.LocalPath);
        }

        context.Response.Close();
    }

    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        foreach (var file in resource.Files)
        {
            var content = files[file.Name];
            var path = Path.Join(this.rootDirectory, resource.Name, file.Name);
            this.additionalFiles[path.Replace("\\", "/")] = content;
        }
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        foreach (var file in resource.Files)
        {
            var path = Path.Join(this.rootDirectory, file.Name);
            File.Delete(path);
        }
    }
}
