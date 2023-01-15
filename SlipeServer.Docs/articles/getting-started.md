# Getting started

This article will describe how to get started with your first Slipe Server project.

## Preqequisites
- Visual Studio (or another editor if you prefer)
- dotnet 7.0

## Creating your project
We're going to create and run our first Slipe Server project, in order to do so:
  
- 1. Create a new c# Console Project using dotnet 7.0
- 2. Install [our SlipeServer.Server NuGet package](https://www.nuget.org/packages/SlipeServer.Server)
- 3. Add the following code to your `Program.cs`:
    ```cs
    using System.Threading.Tasks;
    using SlipeServer.Server;
    using SlipeServer.Server.ServerBuilders;

    var server = MtaServer.Create(builder =>
    {
        builder.AddDefaults();
    });

    server.Start();
    await Task.Delay(-1);
    ```
- 4. Run the project
- 5. Go to your local server tab in your browser
- 6. Connect to the server

If everything worked as intended, you are now connected to an MTA server, you will however just see a black screen. In the next article: [your first logic](/articles/your-first-logic.html) we will add some logic to the server so you actually spawn somewhere.
