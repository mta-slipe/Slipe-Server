# Your First Logic

This article will describe how to start implementing gameplay logic to your server.

## Prerequisites
- Complete the getting-started tutorial

### Dependency injection
Slipe Server makes use of dependency injection, allowing for loosely coupled and well maintainable code.  
If you are unaware of what dependency injection is, I would recommend reading up on it. There is loads of good sources available online. But you don't neccesarily need full understanding of it for the purposes of getting started with your first logic.


## Creating your logic class
The first thing we do is we create a new class for our logic, this can be named anything but for the purpose of this tutorial let's name it `SpawningLogic`

```cs
public class SpawningLogic {}
```

In order for this logic to be used by the server, we go back to our program.cs as created in [getting started](/articles/getting-started.html), and add the following line: `builder.AddLogic<SpawningLogic>()` to our server setup. The end result should look something like this:
```cs
var server = MtaServer.Create(builder =>
{
    builder.AddDefaults();
    builder.AddLogic<SpawningLogic>();
});
```

This will make it so an instance of our `SpawningLogic` class is created, and any services we need can be injected into it.

We inject these servers into our `SpawningLogic` by defining (additional) constructor parameters. Some examples of services that can be injected are: 
- `MtaServer`  
  MTA server itself
- `RootElement`  
  The MTA root element
- `IElementCollection`  
  A collection of all elements on the server
- `ILogger`  
  To log to the server console

We will inject the `MtaServer` class, and the `ILogger` interface, and store these as private members for later use.  

We also registeran event handler for the `PlayerJoined` event on the `MtaServer` instance, this will trigger your method as soon any player has successfully joined your server.

```cs
public class SpawningLogic
{
    private readonly MtaServer mtaServer;
    private readonly ILogger logger;

    public SpawningLogic(MtaServer mtaServer, ILogger logger)
    {
        this.mtaServer = mtaServer;
        this.logger = logger;
        
        this.server.PlayerJoined += OnPlayerJoin;
    }

    private void OnPlayerJoin(Player player) { }
}
```

With this we have an entry point where we can start running code whenever the player joins, for now we will just spawn the player, and fade his camera.  
We also will use the logger that we have stored, in order to log to the server console that the player has now joined.

```cs
private void OnPlayerJoin(Player player)
{
    player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
    player.Camera.Fade(CameraFade.In);

    this.logger.LogInformation("{name} has joined the game", player.Name);
}
```

When you run this code, and connect ot the server, your player should now be spawned, and be able to see the world around you.

## Next?

At this point feel free to look at the different properties, methods and events that are available on the `Player` class.   

When you're ready to proceed and create some other elements, read the next article: [More Elements](/articles/getting-started/more-elements.html)