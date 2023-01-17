# More Elements

This article will describe how to create and interact with more elements than just players

## Creating an element
For the purposes of this article we're going to create a new logic, and add it to the configuration. We will call this the `AdditionalElementLogic`.

```cs
public class AdditionalElementLogic {}
```
```cs
var server = MtaServer.Create(builder =>
{
    // snip
    builder.AddLogic<AdditionalElementLogic>();
});
```

An element can relatively simply be created by just using a `new` statement with the specific element class.  
```cs
public class AdditionalElementLogic
{
    private readonly Vehicle vehicle;

    public void AdditionalElementLogic(MtaServer mtaServer)
    {
        this.vehicle = new Vehicle(530, new Vector3(20, 5, 3));
    }
}
```

### Association
Running the above code will seemingly do nothing. When connecting to the server you will not see the vehicle in question. The reasoning for this is that vehicle is in no way associated to the server.  

Because Slipe Server runs as a library, this means that we can't make any assumptions about how exactly it's used. It's very possible for example to have multiple servers running within a single process, or even no running server at all.  
Due to this when you create an element, you have to explicitly tell it what server it belongs to, in order for it to appear for players that are connected to that server. You can do this with the `.AssociateWith(MtaServer server)` method.

```cs
public class AdditionalElementLogic
{
    private readonly Vehicle vehicle;

    public void AdditionalElementLogic(MtaServer mtaServer)
    {
        this.vehicle = new Vehicle(530, new Vector3(20, 5, 3));
        this.vehicle.AssociateWith(mtaServer);
    }
}
```

Now when you run this and connect to the server, you will actually see this vehicle, and be able to interact with it like normal.  
Just like with the `Player` class, you can explore this class' properties, methods and events.

Other element classes are used in very similar ways, these other classes are:
- `Blip`
- `DummyElement`
- `Element`
- `Marker`
- `Ped`
- `Pickup`
- `Player`
- `RadarArea`
- `RootElement`
- `Team`
- `Vehicle`
- `Water`
- `WeaponObject`
- `WorldObject`

### Next?
When you're ready to proceed and, we recommend: [Injectable services](/articles/getting-started/injectable-services.html)