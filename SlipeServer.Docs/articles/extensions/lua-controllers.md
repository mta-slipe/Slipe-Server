# Lua Controllers
Lua Controllers is a [NuGet package](https://www.nuget.org/packages/SlipeServer.LuaControllers/) that allows for an easier way to handle client -> server Lua events.

This is done in a way that's not dissimilar to ASP.NET MVC Controllers (hence the name).

## Getting started
First you use the `AddLuaControllers()` method on the server builder to enable support for Lua controllers.

```cs
var server = MtaServer.Create(builder =>
{
    //snip

    builder.AddLuaControllers();
});
```

Next create your controller class, this class has to inherit from `BaseLuaController`

```cs
public class SampleController : BaseLuaController
{
    public SampleController(MtaServer server)
    {
    }
}
```

Just like with most of Slipe Server, you can use dependency injection in your controllers.

Once you've created your controller any method you add will be called when an even is triggered with the method name as event name.

```cs
public class SampleController : BaseLuaController
{
    private readonly ILogger logger;

    public SampleController(ILogger logger)
    {
        this.logger = logger;
    }

    public void Foo() // this method is called when the "Foo" event is triggered.
    {

    }
}
```

If you wish for a method to not be called by Lua events you can add the `NoLuaEvent` attribute
```cs
[NoLuaEvent]
public void WillnotBeCalledWhenEventIsTriggered() {}
```

### Renaming

You can also use the `LuaEvent` attribute, this will allow you to change the event name that will call a method.
```cs
[LuaEvent("Banana")]
public void Foo() { }
```

You can also add a prefix for all methods in a controller by using the `LuaController` attribute.
```cs
[LuaController("SlipeServer.Controllers.")]
public class SampleController : BaseLuaController
{
    [LuaEvent("Banana")]
    public void Foo() { }
}
```

In this above example the `"SlipeServer.Controllers.Banana"` event will call the `Foo` method.

### Event handling context
In controller methods you might want access to some information about the event, you can do this using the `Context` property on the controller.  
This property is a [`LuaEventContext`](/api/optionals/lua-controllers/SlipeServer.LuaControllers.Contexts.LuaEventContext.html), which contains the event name, event source, and the player that triggered the event.

### Different player types
You may have configured your server to use a different player class. Controllers have support for this by inheriting from the generic `BaseLuaController<TPlayer>`. This mostly works the exact same, but the `Player` property on the context will be of the generic player type. This saves you some needless casting when handling events.
For example:

```cs
[LuaController("Slipe.TeamDeathMatch.")]
public class SampleController : BaseLuaController<SamplePlayer>
{    
    public void Foo()
    {
        this.Context.Player.SampleMethod();
    }
}
```

### Parameters
You can also add parameters to your controller methods, these will be mapped from the lua values that the event was triggered with.  

Parameters are mapped using the [`FromLuaValueMapper`](/api/server/SlipeServer.Server.Mappers.FromLuaValueMapper.html). You can configure this mapper to support any arbitrary type using the [`ServerBuilder` extensions](/api/server/SlipeServer.Server.Mappers.LuaMapperBuilderExtensions.html).  
By default however this is configured to be able to map most types, either by simple casts, or using reflection.

You can have methods with relatively simple parameters
```cs
public class AuthenticationController : BaseLuaController<TdmPlayer>
{
    private readonly AccountService accountService;

    public AuthenticationController(
        AccountService accountService)
    {
        this.accountService = accountService;
    }

    public async void Login(string username, string password)
    {
        await this.accountService.LoginAsync(this.Context.Player, username, password);
    }
}
```

Or methods with more complex parameters, such as elements, vectors, enums
```cs
public class FreeroamController : BaseLuaController<FreeroamPlayer>
{
    [LuaEvent("setVehicleOverrideLights")]
    public void SetOverrideLights(Vehicle vehicle, int lights)
    {
        vehicle.OverrideLights = (VehicleOverrideLights)lights;
    }

    [LuaEvent("setPedStat")]
    public void SetPedStat(FreeroamPlayer player, PedStat stat, float value)
    {
        player.SetStat(stat, value);
    }
}
```