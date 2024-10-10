﻿using Microsoft.Extensions.Logging;
using SlipeServer.Server.Constants;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for responding to local server list queries
/// If the request comes from a debug client, and the server's configuration contains a debug port, the debug port will be sent to the client.
/// </summary>
public class LocalServerAnnouncementBehaviour
{
    private readonly Configuration configuration;
    private readonly ILogger logger;

    public LocalServerAnnouncementBehaviour(Configuration configuration, ILogger logger)
    {
        this.configuration = configuration;
        this.logger = logger;

        StartListening(NetworkConstants.LanPort);
    }

    private void OnUdpData(IAsyncResult result)
    {
        if (result.AsyncState is UdpClient socket)
        {
            try
            {
                IPEndPoint? source = new IPEndPoint(0, 0);
                byte[] incomingData = socket.EndReceive(result, ref source);
                string message = Encoding.UTF8.GetString(incomingData);
                this.logger.LogTrace("Local server broadcast received from {address} \"{message}\"", source?.Address, message);

                var port = this.configuration.Port + 123;
                if (message.TrimEnd().EndsWith("n"))
                    port = (this.configuration.DebugPort ?? this.configuration.Port) + 123;

                byte[] data = Encoding.UTF8.GetBytes($"MTA-SERVER {port}");

                socket.Send(data, data.Length, source);
                socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            }
            catch (Exception e)
            {
                this.logger.LogError("{exceptionMessage}", e.Message);
            }
        }
    }

    private void StartListening(ushort port)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

        UdpClient socket = new UdpClient();
        socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.Client.Bind(localEndPoint);
        socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
    }
}
