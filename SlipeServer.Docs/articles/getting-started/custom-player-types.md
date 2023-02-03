# Custom Player Types

Slipe Server allows you to define your own classes for elements. Most elements are generally created by the consumer of the SlipeServer.Server library, the Player class however is instantiated by the server when a player connects.  

You might want to create your own player class in order to keep track of additional data on it, or add additional methods. In order to do so the `MtaServer` class has a generic static method to createa server that uses a custom `Player` class, [`Create<TPlayer>()`](/api/server/SlipeServer.Server.MtaServer.html#SlipeServer_Server_MtaServer_Create__1_Action_SlipeServer_Server_ServerBuilders_ServerBuilder__).   

In order to use this you first need to make a new class that inherits from the `Player` class
```cs
public class CustomPlayer : Player { }
```

You can replace your `MtaServer.Create()` call in your Program.cs with:
```cs
var server = MtaServer.Create<CustomPlayer>(builder =>
{
    // builder actions here
});
```

This will then create an instance of your `CustomPlayer` class when a player connects.

## Dependency injection
You can also use a custom player class, and inject services into it using the dependency injection container. You do this by using the [`MtaServer.CreateWithDiSupport<TPlayer>`](/api/server/SlipeServer.Server.MtaServer.html#SlipeServer_Server_MtaServer_CreateWithDiSupport__1_Action_SlipeServer_Server_ServerBuilders_ServerBuilder__) method.
```cs
var server = MtaServer.CreateWithDiSupport<CustomPlayer>(builder =>
{
    // builder actions here
});
```

You can then use constructor injection in your player class.
```cs
public class CustomPlayer : Player
{
    public CustomPlayer(ExplosionService explosionService)
    {
        this.Wasted += (_, _) 
            => explosionService.CreateExplosion(this.position, ExplosionType.TankGrenade);
    }

}
```