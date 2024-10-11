using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SlipeServer.Server.TestTools;

public class MasterListServer
{
    public string Ip { get; set; }
    public ushort Port { get; set; }
}

public class MtaSaMasterServerDelegateHandler : DelegatingHandler
{
    private readonly List<MasterListServer> _servers = [];

    public string DetectedIp { get; } = "123.123.123.123";
    public IEnumerable<MasterListServer> Servers => this._servers;

    public event Action<MasterListServer>? ServerAdded;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri != null && request.RequestUri.ToString().Contains("master.mtasa.com/ase/add.php") && request.Method == HttpMethod.Post)
        {
            var queryParams = HttpUtility.ParseQueryString(request.RequestUri.Query);

            var gParam = ushort.Parse(queryParams["g"]); // port
            var aParam = queryParams["a"]; // ase port
            var hParam = queryParams["h"]; // http port
            var vParam = queryParams["v"]; // version
            var xParam = queryParams["x"]; // ip
            var ipParam = queryParams["ip"]; // ip

            if (!this._servers.Any(x => x.Port == gParam))
            {
                var server = new MasterListServer
                {
                    Ip = this.DetectedIp,
                    Port = gParam
                };
                this._servers.Add(server);
                ServerAdded?.Invoke(server);
            }

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"&interval=720&remote_addr={this.DetectedIp}&using_addr={this.DetectedIp}&ok_message=(Auto detected IP:{this.DetectedIp})", System.Text.Encoding.UTF8, "application/json")
            };

            return await Task.FromResult(fakeResponse);
        }

        var response = await base.SendAsync(request, cancellationToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
