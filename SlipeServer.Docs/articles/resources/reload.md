# Reload
The reload resource is a port of the default reload resource, allowing for players to reload their weapons by pressing the R key

In order to add it to your server first install [the NuGet package](https://www.nuget.org/packages/SlipeServer.resources.reload/). And then add the following to your server builder:

```cs
using SlipeServer.Resources.Resource;

// snip

this.server = MtaServer.Create(
    (builder) =>
    {
        builder.AddReloadResource();
    }
);
```

