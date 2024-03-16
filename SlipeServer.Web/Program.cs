using SlipeServer.ConfigurationProviders;
using SlipeServer.Example;
using SlipeServer.Example.Elements;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ILogger>(x => x.GetRequiredService<ILogger<Program>>());

var configurationProvider = args.Length > 0 ? ConfigurationLoader.GetConfigurationProvider(args[0]) : null;

var serverConfiguration = configurationProvider?.GetConfiguration() ?? new Configuration()
{
    IsVoiceEnabled = true,
#if DEBUG
    DebugPort = 50667
#endif
};

builder.ConfigureMtaServer<CustomPlayer>(serverBuilder =>
{
    serverBuilder.ConfigureExampleServer(serverConfiguration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/players", (IElementCollection elementCollection) =>
{
    return elementCollection.GetByType<Player>().Select(x => x.Name);
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
