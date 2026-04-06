using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Console.LuaDefinitions;
using SlipeServer.Lua;
using SlipeServer.Scripting;
using SlipeServer.Server;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Logic;

public class LuaTestLogic
{
    private readonly IMtaServer server;
    private readonly IRootElement root;
    private readonly IScriptEventRuntime eventRuntime;
    private readonly LuaService luaService;
    private readonly ICommandService commandService;
    private readonly ILogger logger;
    private readonly IResourceProvider resourceProvider;

    public LuaTestLogic(
        IMtaServer server,
        IRootElement root,
        IScriptEventRuntime eventRuntime, 
        LuaService luaService,
        ICommandService commandService,
        ILogger logger,
        IResourceProvider resourceProvider
        )
    {
        this.server = server;
        this.root = root;
        this.eventRuntime = eventRuntime;
        this.luaService = luaService;
        this.commandService = commandService;
        this.logger = logger;
        this.resourceProvider = resourceProvider;

        commandService.AddCommand("lua").Triggered += (source, args) => Init();
        commandService.AddCommand("luatest").Triggered += (source, args) => InitTest();
        commandService.AddCommand("luagate").Triggered += (source, args) => InitGate();
    }

    private void Init()
    {
        this.eventRuntime.LoadDefaultEvents();

        this.luaService.LoadDefaultDefinitions();

        this.luaService.LoadDefinitions<CustomMathDefinition>();
        this.luaService.LoadDefinitions<TestDefinition>();
    }

    private void InitTest()
    {
        using var testLua = File.OpenRead("test.lua");
        using var reader = new StreamReader(testLua);
        try
        {
            this.luaService.LoadScript("test.lua", reader.ReadToEnd());
        }
        catch (InterpreterException ex)
        {
            this.logger.LogInformation("Failed to load script\n\t{message}", ex.DecoratedMessage);
        }
    }

    private void InitGate()
    {
        var gateResource = new Resource(this.server, this.root, "GateTest", "GateTest")
        {
            NetId = this.resourceProvider.ReserveNetId()
        };

        gateResource.Start();

        this.luaService.LoadScript("gate.lua", """
            local object = createObject(321, 10, 0, 5)
            local colshape = createColSphere(10, 0, 5, 5)
            
            addEventHandler("onColShapeHit", colshape, function(hitElement)
                if getElementType(hitElement) == "player" then
                    moveObject(object, 1000, 10, 0, 10)
                end

                debugBreak()
            end)
            
            addEventHandler("onColShapeLeave", colshape, function(leftElement)
                if getElementType(leftElement) == "player" then
                    moveObject(object, 1000, 10, 0, 5)
                end
            
                debugBreak()
            end)
            """, gateResource);


        Command? command = null;
        void TerminateGate()
        {
            this.commandService.RemoveCommand(command);
            this.luaService.UnloadScriptsFor(gateResource);
        }

        command = this.commandService.AddCommand("stopgate");
        command.Triggered += (source, args) => TerminateGate();
    }
}
