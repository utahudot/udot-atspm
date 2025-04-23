using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DeviceEmulator;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Serve XML logs from "/logs/{DeviceId}"
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "data")),
    RequestPath = "/logs"
});

app.Run();
