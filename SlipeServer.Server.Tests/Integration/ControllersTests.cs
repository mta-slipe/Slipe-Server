using SlipeServer.Server.Services;
using SlipeServer.Server.TestTools;
using Xunit;
using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using System;
using System.Reflection;
using SlipeServer.Server.ElementCollections;
using System.Threading.Tasks;
using System.Collections.Generic;
using SlipeServer.Server.Elements;
using System.Linq;
using SlipeServer.LuaControllers.Commands;
using SlipeServer.Server.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions.Execution;

namespace SlipeServer.Server.Tests.Integration;

public class SampleClass
{
    public int Number { get; set; }
}

public enum SampleEnum
{
    EnumValue1 = 1,
    EnumValue2 = 2,
    EnumValue3 = 3,
}

public class LuaControllersExampleLogic
{
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

public class ControllersTestsHelper : List<object>
{
    public int InvokeCalls { get; set; }
    public int InvokeAsyncCalls { get; set; }
    public TaskCompletionSource? TaskCompletionSource { get; set; }

    public void ClearAll()
    {
        this.InvokeCalls = 0;
        this.InvokeAsyncCalls = 0;
        TaskCompletionSource?.TrySetCanceled();
        TaskCompletionSource = null;
        Clear();
    }
}

public class NoAccessAttribute : Attribute;

[CommandController]
public class TestCommandController : BaseCommandController<TestingPlayer>
{
    private readonly ControllersTestsHelper controllersTestsHelper;

    public TestCommandController(ControllersTestsHelper controllersTestsHelper)
    {
        this.controllersTestsHelper = controllersTestsHelper;
    }

    protected override void Invoke(Action next)
    {
        if (this.Context.MethodInfo.GetCustomAttribute<NoAccessAttribute>() == null)
        {
            this.controllersTestsHelper.InvokeCalls++;
            next();
        }
    }

    protected override async Task InvokeAsync(Func<Task> next)
    {
        if (this.Context.MethodInfo.GetCustomAttribute<NoAccessAttribute>() == null)
        {
            this.controllersTestsHelper.InvokeAsyncCalls++;
            await next();
        }
    }

    public void Sample()
    {
        this.controllersTestsHelper.Add("Sample");
    }

    public void StringArgument(string arg)
    {
        this.controllersTestsHelper.Add("StringArgument");
        this.controllersTestsHelper.Add(arg);
    }

    public void NumberArgument(int arg)
    {
        this.controllersTestsHelper.Add("NumberArgument");
        this.controllersTestsHelper.Add(arg);
    }
    
    public void EnumArgument(SampleEnum sampleEnum)
    {
        this.controllersTestsHelper.Add("EnumArgument");
        this.controllersTestsHelper.Add(sampleEnum);
    }

    public void VariadicArguments(IEnumerable<string> words)
    {
        this.controllersTestsHelper.Add("VariadicArguments");
        this.controllersTestsHelper.Add(string.Join(' ', words));
    }
    
    public void Exception()
    {
        throw new Exception("oops");
    }

    [NoAccess]
    public void NoAccess()
    {
        this.controllersTestsHelper.Add("NoAccess");
    }

    public void CustomType(SampleClass sampleClass)
    {
        this.controllersTestsHelper.Add("SampleClass");
        this.controllersTestsHelper.Add(sampleClass.Number);
    }

    [Command("commandA")]
    [Command("commandB")]
    public void CommandAlias()
    {
        this.controllersTestsHelper.Add("CommandAlias");
    }

    [NoCommand]
    public void NoCommand()
    {
        this.controllersTestsHelper.Add("NoCommand");
    }

    public async Task Async()
    {
        this.controllersTestsHelper.Add("Pre");
        await Task.Yield();
        this.controllersTestsHelper.Add("Post");
        this.controllersTestsHelper.TaskCompletionSource!.TrySetResult();
    }
    
    public async Task CancelCommand()
    {
        try
        {
            await Task.Delay(-1, this.Context.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            this.controllersTestsHelper.Add("Cancelled");
        }
        finally
        {
            this.controllersTestsHelper.TaskCompletionSource!.TrySetResult();
        }
    }
}

public class LuaControllersFixture
{
    public TestingServer Server { get; }

    public LuaControllersFixture()
    {
        this.Server = new TestingServer((Configuration?)null, x =>
        {
            x.AddLuaControllers();
            x.AddLogic<LuaControllersExampleLogic>();
            x.ConfigureServices(services =>
            {
                services.AddSingleton<ControllersTestsHelper>();
            });
        });
    }
}

public class ControllersTests : IClassFixture<LuaControllersFixture>
{
    private readonly LuaControllersFixture luaControllersFixture;
    private readonly TestingServer server;
    private readonly ControllersTestsHelper helper;
    public ControllersTests(LuaControllersFixture luaControllersFixture)
    {
        this.luaControllersFixture = luaControllersFixture;
        this.server = this.luaControllersFixture.Server;
        this.helper = this.server.GetRequiredService<ControllersTestsHelper>();
        this.helper.ClearAll();
    }

    [Fact]
    public void NonExistingCommand()
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("non existing command", []);

        using var _ = new AssertionScope();
        this.helper.InvokeCalls.Should().Be(0);
        this.helper.Should().BeEmpty();
    }
    
    [InlineData("Sample")]
    [InlineData("sample")]
    [InlineData("SAMPLE")]
    [Theory]
    public void CommandShouldBeCaseInsensitive(string command)
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand(command, []);

        using var _ = new AssertionScope();
        this.helper.InvokeCalls.Should().Be(1);
        this.helper.Should().BeEquivalentTo(["Sample"]);
    }

    [Fact]
    public void StringArgument()
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("StringArgument", ["foo"]);

        this.helper.Should().BeEquivalentTo(["StringArgument", "foo"]);
    }

    [Fact]
    public void NumberArgument()
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("NumberArgument", ["123"]);

        this.helper.Should().BeEquivalentTo(new object[] { "NumberArgument", 123 });
    }

    [InlineData("EnumValue2")]
    [InlineData("2")]
    [Theory]
    public void EnumArgument(string argument)
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("EnumArgument", [argument]);

        this.helper.Should().BeEquivalentTo(new object[] { "EnumArgument", SampleEnum.EnumValue2 });
    }

    [Fact]
    public void VariadicNumberOfArguments()
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("VariadicArguments", ["a", "b", "c"]);

        this.helper.Should().BeEquivalentTo(["VariadicArguments", "a b c"]);
    }

    [Fact]
    public void MalfunctionCommand()
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("Exception", []);

        using var _ = new AssertionScope();
        this.helper.InvokeCalls.Should().Be(1);
        this.helper.Should().BeEmpty();
    }

    [Fact]
    public void NoAccessCommand()
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("NoAccess", []);

        using var _ = new AssertionScope();
        this.helper.InvokeCalls.Should().Be(0);
        this.helper.Should().BeEmpty();
    }

    [Fact]
    public void CustomMapperShouldWork()
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("CustomType", ["123"]);

        this.helper.Should().BeEquivalentTo(new object[] { "SampleClass", 123 });
    }

    [InlineData("commandA")]
    [InlineData("commandB")]
    [Theory]
    public void CommandAliasShouldWork(string command)
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand(command, []);

        this.helper.Should().BeEquivalentTo(["CommandAlias"]);
    }

    [Fact]
    public void NoCommandShouldNotBeCallable()
    {
        var player = this.server.AddFakePlayer();

        player.TriggerCommand("NoCommand", []);

        this.helper.Should().BeEmpty();
    }

    [Fact]
    public async Task AsyncCommandShouldWork()
    {
        var player = this.server.AddFakePlayer();

        this.helper.TaskCompletionSource = new();
        player.TriggerCommand("Async", []);
        await this.helper.TaskCompletionSource.Task;

        using var _ = new AssertionScope();
        this.helper.InvokeAsyncCalls.Should().Be(1);
        this.helper.Should().BeEquivalentTo(["Pre", "Post"]);
    }

    [Fact]
    public async Task CancelCommandWhenPlayerDisconnect()
    {
        var player = this.server.AddFakePlayer();

        this.helper.TaskCompletionSource = new();
        player.TriggerCommand("CancelCommand", []);

        player.TriggerDisconnected(QuitReason.Quit);
        await this.helper.TaskCompletionSource.Task;

        using var _ = new AssertionScope();
        this.helper.InvokeAsyncCalls.Should().Be(1);
        this.helper.Should().BeEquivalentTo(["Cancelled"]);
    }
}
