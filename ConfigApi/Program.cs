using ATSPM.Application.Repositories;
using ATSPM.ConfigApi.EntityDataModel;
using ATSPM.Data;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Google.Api;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
})
.AddXmlDataContractSerializerFormatters()

//https://github.com/microsoft/OpenAPI.NET.OData
.AddOData(opt => opt.AddRouteComponents("config", new ConfigEdm().GetEntityDataModel())
.Select()
.Count()
.Expand()
.OrderBy()
.Filter()
.SkipToken()
.SetMaxTop(5000));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Host.ConfigureServices((h, s) =>
{
s.AddDbContext<ConfigContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).EnableSensitiveDataLogging(h.HostingEnvironment.IsDevelopment()));
s.AddScoped<IApproachRepository, ApproachEFRepository>();
s.AddScoped<IAreaRepository, AreaEFRepository>();
s.AddScoped<IControllerTypeRepository, ControllerTypeEFRepository>();
s.AddScoped<ISignalRepository, SignalEFRepository>();
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("../swagger/v1/swagger.json", "ATSPM.Config v1"));
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
