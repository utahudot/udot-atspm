using ATSPM.Application.Repositories;
using ATSPM.DataApi.Controllers;
using ATSPM.DataApi.EntityDataModel;
using ATSPM.Infrasturcture.Extensions;
using ATSPM.Infrasturcture.Repositories;
using Microsoft.AspNetCore.OData;
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
.AddOData(opt => opt.AddRouteComponents("data", new DataEdm().GetEntityDataModel())
.Select()
.Count()
.Expand()
.OrderBy()
.Filter()
.SkipToken()
.SetMaxTop(500));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Host.ConfigureServices((h, s) =>
{
    s.AddATSPMDbContext(h);

    s.AddScoped<IApproachRepository, ApproachEFRepository>();
    s.AddScoped<IControllerTypeRepository, ControllerTypeEFRepository>();
    s.AddScoped<ISignalRepository, SignalEFRepository>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("../swagger/v1/swagger.json", "ATSPM.Data v1"));
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

//app.MapControllers();

app.Run();
