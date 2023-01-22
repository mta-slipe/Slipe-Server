# Parachute
The parachute resource is a port of the default parachute resource, allowing for singleplayer-like handling of parachuting.

In order to add it to your server first install [the NuGet package](https://www.nuget.org/packages/SlipeServer.resources.parachute/). And then add the following to your server builder:

```cs
using SlipeServer.Resources.Parachute;

// snip

this.server = MtaServer.Create(
    (builder) =>
    {
        builder.AddParachuteResource();
    }
);
```

