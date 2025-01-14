using SlipeServer.Server.Services;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;
using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using System;
using System.Reflection;
using SlipeServer.Server.ElementCollections;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using SlipeServer.Server.Elements;
using System.Linq;
using SlipeServer.LuaControllers.Commands;
using SlipeServer.Server.Enums;

namespace SlipeServer.Server.Tests.Integration;

public class SampleClass
{
    public int Number { get; set; }
}

public class LuaControllersExampleLogic
{
    private readonly IElementCollection elementCollection;
    private readonly ChatBox chatBox;

    public LuaControllersExampleLogic(LuaControllerArgumentsMapper mapper, IElementCollection elementCollection, ChatBox chatBox)
    {
        mapper.DefineMap<SampleClass>(arg =>
        {
            return new SampleClass
            {
                Number = int.Parse(arg)
            };
        });
        mapper.DefineMap<Player>(arg =>
        {
            return elementCollection.GetByType<Player>().Where(x => x.Name.Contains(arg)).FirstOrDefault();
        });

        mapper.ArgumentErrorOccurred += HandleArgumentErrorOccurred;
        this.elementCollection = elementCollection;
        this.chatBox = chatBox;
    }

    private void HandleArgumentErrorOccurred(Player player, Exception exception)
    {
        if (exception is ArgumentOutOfRangeException)
            this.chatBox.OutputTo(player, "Too many or too few arguments");
        else if (exception is LuaControllerArgumentException ex)
        {
            this.chatBox.OutputTo(player, $"Error while executing command, argument at index {ex.Index + 1} expected {ex.ParameterInfo.ParameterType}, got '{ex.Argument}'");
        }
    }
}

public class ControllersTestsHelper
{
    public Action<string>? Callback;
}

public class ControllersTests
{

    internal class NoAccessAttribute : Attribute;

    [CommandController]
    public class TestCommandController : BaseCommandController<TestingPlayer>
    {
        private readonly ChatBox chatBox;
        private readonly IElementCollection elementCollection;
        private readonly ControllersTestsHelper controllersTestsHelper;

        public TestCommandController(ChatBox chatBox, IElementCollection elementCollection, ControllersTestsHelper controllersTestsHelper)
        {
            this.chatBox = chatBox;
            this.elementCollection = elementCollection;
            this.controllersTestsHelper = controllersTestsHelper;
        }

        protected override void Invoke(Action next)
        {
            try
            {
                if (this.Context.MethodInfo.GetCustomAttribute<NoAccessAttribute>() != null)
                {
                    this.chatBox.OutputTo(this.Context.Player, $"You can not access command {this.Context.Command}");
                } else
                {
                    next();
                }
            }
            catch (Exception ex)
            {
                this.chatBox.OutputTo(this.Context.Player, $"Failed to execute command {this.Context.Command}");
            }
        }

        protected override async Task InvokeAsync(Func<Task> next)
        {
            var stopwatch = Stopwatch.StartNew();
            await next();
            Console.WriteLine("Executed async command in: {0}ms", stopwatch.ElapsedMilliseconds);
        }

        public void Chat(IEnumerable<string> words)
        {
            this.chatBox.OutputTo(this.Context.Player, string.Join(' ', words));
        }

        [NoAccess]
        public void NoAccess()
        {
            this.chatBox.OutputTo(this.Context.Player, "You have accessed command with NoAccess attribute!");
        }

        public void Oops()
        {
            throw new Exception("oops");
        }

        public void Ping()
        {
            this.chatBox.OutputTo(this.Context.Player, $"Your ping is {this.Context.Player.Client.Ping}.");
        }

        public void SampleClass(SampleClass sampleClass)
        {
            this.chatBox.OutputTo(this.Context.Player, $"sampleClass: {sampleClass.Number}");
        }

        public void FindPlayer(Player player)
        {
            this.chatBox.OutputTo(this.Context.Player, $"player: {player}");
        }

        public async Task Async()
        {
            this.chatBox.OutputTo(this.Context.Player, "Executing command...");
            await Task.Delay(1000);
            this.chatBox.OutputTo(this.Context.Player, "Command executed!");
        }

        public async Task AsyncLong()
        {
            this.chatBox.OutputTo(this.Context.Player, $"Simulating long execution...");
            try
            {
                await Task.Delay(10_000, this.Context.CancellationToken);
                this.chatBox.OutputTo(this.Context.Player, "Long command executed!");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Failed to execute long async command :(");
            }
        }

        [Command("tp")]
        [Command("teleport")]
        public void Teleport(float x, float y, float z)
        {
            this.Context.Player.Position = new(x, y, z);
        }

        public void SpawnAt(ushort model, float x, float y, float z)
        {
            this.Context.Player.Spawn(new System.Numerics.Vector3(x, y, z), 0, model, 0, 0);
        }

        public void GiveWeapon(WeaponId weapon, ushort ammoCount = 100)
        {
            this.Context.Player.AddWeapon(weapon, ammoCount, true);
        }

        [NoCommand]
        public void NoCommand()
        {
            this.chatBox.OutputTo(this.Context.Player, $"This should not run.");
        }
    }

    [Fact]
    public void LuaControllerCommandsTest()
    {
        var mtaServer = new TestingServer((Configuration?)null, x =>
        {
            x.AddLuaControllers();
            x.AddLuaControllers();
            x.AddLogic<LuaControllersExampleLogic>();
        });

        var player = mtaServer.AddFakePlayer();
    }

}
