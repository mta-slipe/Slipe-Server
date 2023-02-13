# Per Player Elements
Since you have more control over how packets are sent with Slipe Server, you have the ability to create elements only for certain players. This can be used to lower the load on clients, or even use the same element id for multiple different elements, as long as they are sent to different clients.  

This is done using the `.AssociateWith(Player player)` method onverload the `Element` class that accepts a `Player` as parameter, as opposed to an `MtaServer` (or on `IEnumerable<Element>`).  
Calling this method with the player as parameter will only make that element known to that player. It's important to note that in this case no ID will be generated for the element, so you need to manually assign it an ID.  

This also means this ID you assign will need to **not** be used by the server when using `.AssociateWith(MtaServer server)`, in order to do this you will need to replace the default element id generator, with one that uses a specific range. 

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

(There is also `RemoveFrom` counterpart to the `AssociateWith` method)

## Other per-player actions
There might be other cases where you want certain changes to only affect a subset of clients. For example you might want to change the colour of a vehicle for just one player. Normally you would do this with client-sided scripts, but Slipe Server can do this server sided.

Do note that in the case of per-player-elements any changes to the element will already only be relayed to players who are "aware" of this element's existence.  

In order to do this we use the `ClientPacketScope` class. You can read more about that on the [dedicated article](/articles/advanced-features/client-packet-scope.html).