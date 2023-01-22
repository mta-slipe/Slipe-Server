# Lua Resources

While Slipe Server allows you to achieve things you would typically need client side code for, you will still need client side code when using Slipe Server.

Much like in a typical MTA server, you will create Lua resources for this, these resources are however client side only. (Lua support for server side scripting is a WIP, but is not (yet?) working with the resource system).

## Providers
Slipe Server uses resource providers to tell the server which resources exist on your server. By default this uses the filesystem, and a `Resources` directory (configurable using the server [`Configuration`](/api/server/SlipeServer.Server.Configuration.html)).  
This default provider will create resources for any directory in the resource directory.  

If you, for whatever reason, wish to use a different way to get your resources (like for example using cloud storage to storage your resources), you can do this by creating your own implementation of `IResourceProvider`.

## Interpreters
Once the resource provider has identified your resource, it then needs to turn the resource files into an actual resource. This is done using resource interpreters.  

By default the server supports parsing resources from:
- meta.xml files
- entrypoint.slipe files (Slipe Lua)
- N/A  
  If none of the above options are found, it will simply add all `*.lua` files in the directory as client Lua files to the resource.

You can also create your own resource interpreters in case you want to load from a different format, you do this by creating a class that implements the `IResourceInterpreter` interface, and registring it in your server builder.

When a resource interpreter's `IsFallback` is `true`, it will use this resource interpreter if none of the other resource interpreters were able to interpret a resource.

```cs
public class CustomResourceInterpreter : IResourceInterpreter
{
    public bool IsFallback => false;
    
    TryInterpretResource(
        MtaServer mtaServer,
        RootElement rootElement,
        string name,
        string path,
        IResourceProvider resourceProvider,
        out Resource? resource
    )
    {
        resource = new Resource(mtaServer, rootElement, name, path);

        return true;
    }
}
```

```cs
var server = MtaServer.Create(builder =>
{
    builder.AddResourceInterpreter<CustomResourceInterpreter>();
};
```

## Additional resources
Slipe Server also supports adding resources that are not provided by the resource provider.  
You do this by using the [`AddAdditionalResource`](/api/server/SlipeServer.Server.MtaServer.html#methods) method on the `MtaServer` class.  

This exists for the purpose of making NuGet packages (or otherwise libraries) that allow you to easily add a resource. Examples of this can be found in our [various Resource packages](https://www.nuget.org/packages?q=SlipeServer.Resources). ([source](https://github.com/mta-slipe/slipe-server-resources))  


These allow you to do something like:
```cs
using SlipeServer.Resources.Parachute;

var server = MtaServer.Create(
    (builder) =>
    {
        builder.AddParachuteResource();
    }
);
```

## Resource servers
Clients will need to access the (Lua) files for client side resources, these are made available to the clients using a resource server.  
The default implementation for this is the [`BasicHttpServer`](/api/server/SlipeServer.Server.Resources.Serving.BasicHttpServer.html)  
This will serve resource files over HTTP, much like the internal HTTP server in the regular MTA server does.  

You can replace this with anything else you want (you might want to use external processes for example)

## Starting resources
Once you have your resource, either through the default resource provider, or a more complex setup, you can start the resource.  
Unlike default MTA servers, Slipe Server allows you to start resources for only a single player.

If you just want to start resources for a single user, you can use the `StartResource` method on the [`ResourceService`](https://server.mta-slipe.com/api/server/SlipeServer.Server.Resources.ResourceService.html) class.

If you want to do more complex things (like the mentioned starting for individual clients), you can inject the `IResourceProvider`, to get a reference to the [`Resource` class](/api/server/SlipeServer.Server.Resources.Resource.html) instance. 

Once you have this resource instance you can use the `StartFor()` or `StartForAsync()` methods.

The `StartForAsync` method is awaitable, if you await this it will continue execution once the resource has actually started on the client.