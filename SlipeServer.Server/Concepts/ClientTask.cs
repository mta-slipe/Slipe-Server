using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Exceptions;
using SlipeServer.Server.Resources;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Server.Concepts;

public static class ResourceExtensions
{
    public static Resource AddClientTaskHelper(this Resource resource)
    {
        resource.NoClientScripts["clientTaskHelper.lua"] = System.Text.UTF8Encoding.UTF8.GetBytes(ClientTask.luaHelperCode);

        return resource;
    }
}

public sealed class ClientErrorException : Exception
{
    public ClientErrorException(string? message) : base(message)
    {

    }
}

public sealed class ClientTask : LuaValue, IDisposable
{
    public const string luaHelperCode = """
        ClientTask = {
          Resolve = function(clientTask, ...)
            if(clientTask._completed)then
                error("ClientTask already completed");
            end
            clientTask._completed = true;
            triggerServerEvent("clientTask_"..clientTask._id, localPlayer, "success", ...)
          end,
          Reject = function(clientTask, ...)
            if(clientTask._completed)then
                error("ClientTask already completed");
            end
            clientTask._completed = true;
            triggerServerEvent("clientTask_"..clientTask._id, localPlayer, "error", ...)
          end,
        }
        """;

    private readonly TaskCompletionSource taskCompletionSource;
    private readonly string eventName;
    public MtaServer MtaServer { get; }
    public Player Player { get; }
    public string Id { get; }

    internal ClientTask(MtaServer mtaServer, Player player, string id, CancellationToken cancellationToken) : base(new LuaTable
    {
        ["_id"] = id
    })
    {
        this.taskCompletionSource = new TaskCompletionSource();
        this.MtaServer = mtaServer;
        this.Player = player;
        this.Id = id;
        this.eventName = $"clientTask_{this.Id}";
        this.MtaServer.LuaEventTriggered += HandleLuaEventTriggered;
        this.Player.Disconnected += HandleDisconnected;
        cancellationToken.Register(() =>
        {
            this.taskCompletionSource.TrySetCanceled();
        });
    }

    private void HandleDisconnected(Player sender, Elements.Events.PlayerQuitEventArgs e)
    {
        this.taskCompletionSource.TrySetException(new PlayerDisconnectedException(sender));
        Dispose();
    }

    private void HandleLuaEventTriggered(Events.LuaEvent luaEvent)
    {
        if (luaEvent.Name != this.eventName || luaEvent.Player != this.Player)
            return;

        try
        {
            var result = luaEvent.Parameters[0].StringValue;

            if (result == "success")
            {
                this.taskCompletionSource.TrySetResult();
            }
            else if (result == "error")
            {
                if(luaEvent.Parameters.Length >= 2)
                {
                    this.taskCompletionSource.TrySetException(new ClientErrorException(luaEvent.Parameters[1].StringValue));
                } else
                {
                    this.taskCompletionSource.TrySetException(new ClientErrorException(null));
                }
            }
            else
            {
                this.taskCompletionSource.TrySetException(new InvalidOperationException());
            }
        }
        catch (Exception ex)
        {
            this.taskCompletionSource.TrySetException(ex);
        }
        finally
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        this.MtaServer.LuaEventTriggered -= HandleLuaEventTriggered;
        this.Player.Disconnected -= HandleDisconnected;
        this.taskCompletionSource.TrySetException(new ObjectDisposedException(nameof(ClientTask)));
    }

    public TaskAwaiter GetAwaiter()
    {
        return this.taskCompletionSource.Task.GetAwaiter();
    }
}
