using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DeviceEmulator;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Serve XML logs from "/logs/{DeviceId}"
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider("/files"), //  match docker volume
    RequestPath = "/logs"
});

app.Run();
