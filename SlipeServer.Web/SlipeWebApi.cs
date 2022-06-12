using Microsoft.AspNetCore.Mvc.ApplicationParts;
using SlipeServer.Web.Controllers;

namespace SlipeServer.Web;

public class SlipeWebApi
{
    private readonly WebApplicationBuilder builder;
    public IServiceCollection Services => this.builder.Services;
    public WebApplication? App { get; private set; }
    public IServiceProvider? ServiceProvider => this.App?.Services;

    public SlipeWebApi()
    {
        this.builder = WebApplication.CreateBuilder();
    }

    public void InitialiseServices()
    {
        this.builder.Services.AddAutoMapper(typeof(SlipeWebApi));

        this.builder.Services.AddControllers().PartManager.ApplicationParts.Add(new AssemblyPart(typeof(PlayerController).Assembly));
        this.builder.Services.AddEndpointsApiExplorer();
        this.builder.Services.AddSwaggerGen();
    }

    public void Build()
    {
        this.App = this.builder.Build();

        if (this.App.Environment.IsDevelopment())
        {
            this.App.UseSwagger();
            this.App.UseSwaggerUI();
        }

        this.App.UseHttpsRedirection();
        this.App.UseAuthorization();
        this.App.MapControllers();
    }

    public void Run()
    {
        this.App?.Run();
    }
}
