# Per Player Elements
Since you have more control over how packets are sent with Slipe Server, you have the ability to create elements only for certain players. This can be used to lower the load on clients, or even use the same element id for multiple different elements, as long as they are sent to different clients.  

This is done using the `.CreateFor()` method on the `Element` class (or on `IEnumerable<Element>`).  
In order to use this method you need to **not** call the `.AssociateWith(server)` method on the element, since that will create the element for all players. Due to this the element however will not be assigned an ID. Because of this you will need to manually assign an ID.

This also means this ID you assign will need to not be used by the server when using `.AssociateWith`, in order to do this you will need to replace the default element id generator, with one that uses a specific range. 

There are some default implementations to do this relatively easily. To start, replace the default id generator with `RangedCollectionBasedElementIdGenerator`.

```cs
uint rangeStart = 1;
uint rangeStop = 100000;

var server = MtaServer.Create(builder =>
{
    builder.ConfigureServices(services =>
    {
        services.AddSingleton<IElementIdGenerator, RangedCollectionBasedElementIdGenerator>(x =>
            new RangedCollectionBasedElementIdGenerator(x.GetRequiredService<IElementCollection>(), rangeStart, rangeStop)
        );
    });
});
```

This will make sure the server only uses the range `1`-`100000` for assigning ids to server-associated elements. This means the rest of the range (`100001` to `131071`) can be used for per-player elements.  

Once this is done we can now create our element for a single player, as long as we assign it an id that is not known to the player yet.  
```cs
commandService.AddCommand("createelementforme").Triggered += (source, args) =>
{
    var bin = new WorldObject(1337, new Vector3(0, 0, 3));
    bin.Id = 100001;
    bin.CreateFor(args.Player);
};
```

In order to facilitate handling ids for these elements you can also create an instance of the `RangedElementIdGenerator`. You can create one with your range that is free for per-player elements, and use the ids returned by this for your elements.

(There is also `DestroyFor` counterpart to the `CreateFor` method)

## Other per-player actions