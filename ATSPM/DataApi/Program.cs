using ATSPM.Application.Repositories;
using ATSPM.DataApi.Formatters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Formatters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
    //options.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
    //https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-5.0#special-case-formatters
    options.OutputFormatters.Add(new EventLogCsvFormatter());
    options.OutputFormatters.RemoveType<StringOutputFormatter>();
})
.AddXmlDataContractSerializerFormatters();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c => c.OperationFilter<CustomHeaderSwaggerAttribute>());

builder.Host.ConfigureServices((h, s) =>
{
    s.AddATSPMDbContext(h);

    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();

    s.AddScoped<IAreaRepository, AreaEFRepository>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("../swagger/v1/swagger.json", "ATSPM.Data v1"));

}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
