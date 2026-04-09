using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Structs;
using SlipeServer.Server;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Resources.Serving;
using System.Net;
using System.Runtime.InteropServices;

namespace SlipeServer.DropInReplacement.MixedResources;

public class DropInReplacementResourceServer : IResourceServer
{
    private readonly HttpListener httpListener;
    private readonly string rootDirectory;
    private readonly Configuration configuration;
    private readonly ILogger logger;
    private readonly IResourceProvider resourceProvider;
    private readonly string httpAddress;
    private readonly string listenerHost;
    private readonly Dictionary<string, byte[]> additionalFiles;

    private bool isRunning;

    public DropInReplacementResourceServer(Configuration configuration, ILogger logger, IResourceProvider resourceProvider)
    {
        // If the configured host is '*' or '0.0.0.0', HttpListener requires the '+' wildcard for prefixes
        this.listenerHost = configuration.HttpHost == "*" || configuration.HttpHost == "0.0.0.0" ? "+" : configuration.HttpHost;
        this.httpAddress = $"http://{this.listenerHost}:{configuration.HttpPort}/";
        this.httpListener = new HttpListener();
        this.httpListener.Prefixes.Add(this.httpAddress);
        this.additionalFiles = new();

        this.isRunning = false;
        this.rootDirectory = configuration.ResourceDirectory;
        this.configuration = configuration;
        this.logger = logger;
        this.resourceProvider = resourceProvider;
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
                string command = $@"netsh http add urlacl url=http://{this.listenerHost}:{this.configuration.HttpPort}/ sddl=D:(A;;GX;;;S-1-1-0)";
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
        var localPath = context.Request.Url?.LocalPath ?? string.Empty;
        var path = Path.Join(this.rootDirectory, localPath);

        var fullPath = Path.GetFullPath(Path.Combine(this.rootDirectory, localPath.TrimStart('/')));

        context.Response.AddHeader("Server", "SlipeServer BasicHttpServer");

        if (!fullPath.StartsWith(Path.GetFullPath(this.rootDirectory)))
        {
            context.Response.StatusCode = 403;
            this.logger.LogWarning("403 forbidden path traversal attempt: {path}", context.Request.Url?.LocalPath);
            context.Response.Close();
            return;
        }

        var splits = localPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var resourceName = splits.First();
        var remainingPath = string.Join('/', splits.Skip(1));


        if (this.additionalFiles.TryGetValue(path, out var value))
        {
            context.Response.OutputStream.Write(value, 0, value.Length);
            context.Response.StatusCode = 200;
        } else
        {
            var resource = this.resourceProvider.GetResource(resourceName);
            var file = resource?.Files
                .Where(x => x.Name == remainingPath)
                .SingleOrDefault(ShouldFileBeServed);

            if (resource != null && file != null)
            {
                var content = this.resourceProvider.GetFileContent(resourceName, remainingPath);
                using var writer = new BinaryWriter(context.Response.OutputStream);
                writer.Write(content);

                context.Response.StatusCode = 200;
            } else
            {
                if (localPath == "/mta_client_firewall_probe/")
                {
                    context.Response.StatusCode = 200;
                } else
                {
                    context.Response.StatusCode = 404;
                    this.logger.LogWarning("404 encountered while trying to download {path}", localPath);
                }
            }
        }

        context.Response.Close();
    }

    private bool ShouldFileBeServed(ResourceFile file)
    {
        switch ((ResourceFileType)file.FileType)
        {
            case ResourceFileType.ClientScript:
            case ResourceFileType.ClientConfig:
            case ResourceFileType.Html:
            case ResourceFileType.ClientFile:
                return true;

            case ResourceFileType.Map:
            case ResourceFileType.Script:
            case ResourceFileType.Config:
            case ResourceFileType.None:

            default:
                return false;
        }
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
