# Client Packet Scope
In some cases you might want certain changes to only affect a subset of clients.  
For example you might want to change the color of a vehicle for just one player. Normally you would do this with client-sided scripts, but Slipe Server can do this server sided.

In order to do this you make use of the `ClientPacketScope` class. This class is used similarly to how database transactions (`TransactionScope`) work.

You open a packet scope for a subset of players, and call any method or change any property that would result in a packet being sent. Any packets that are sent between opening and closing the packet scope, will only be sent to the player(s) that are in the packet scope.

```cs
this.commandService.AddCommand("mytaxicolor").Triggered += (source, args) =>
{
    using (var scope = new ClientPacketScope(args.Player))
    {
        this.Taxi.Colors.Primary = args.Arguments.Length > 0 ?
            Color.FromKnownColor(Enum.Parse<KnownColor>(args.Arguments[0], true)) :
            Color.Pink;
    }
};
```

This will work for any type of packet that is sent to the client, regardless of the source. So make sure you only perform actions within this scope if you're certain you only want certain clients to be aware of it.

Do note that in the above example, according to the server the vehicle color will be the one specified by the user, even if only one user can see this. If any user reconnects they will be sent the current state of the vehicle.

Consider the following example:

- The server has 3 online players
  - Player A
  - Player B
  - Player C
- Player A uses `/mytaxicolor orange`
- Player A sees the vehicle as being orange
- Players B and C see the vehicle with the original color
- A new player, Player D connects
- Player D sees the vehicle as being orange (since according to the server it is orange)