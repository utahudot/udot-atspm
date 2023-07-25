using ATSPM.Application.Repositories;
using ATSPM.DataApi.Formatters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
    options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));

    //options.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
    //https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-5.0#special-case-formatters
    options.OutputFormatters.Add(new EventLogCsvFormatter());
    options.OutputFormatters.RemoveType<StringOutputFormatter>();
})
.AddXmlDataContractSerializerFormatters();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(a =>
{
    a.AssumeDefaultVersionWhenUnspecified = true;
    a.DefaultApiVersion = new ApiVersion(1, 0);
    a.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(a =>
{
    a.GroupNameFormat = "'v'VV";
});

var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();

builder.Services.AddSwaggerGen(c =>
{
    foreach (var desc in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        Console.WriteLine($"v{desc.GroupName} - {desc.ApiVersion}");
        
        c.SwaggerDoc(desc.GroupName, new()
        {
            Title = "Controller Event Log API",
            Version = desc.ApiVersion.ToString(),
            Description = "API to interact with controller event log data",
            Contact = new()
            {
                Name = "udotdevelopment",
                Email = "udotdevelopment@gmail.com",
                Url = new Uri("https://udottraffic.utah.gov/atspm/")
            }
        });
    }

    c.DocInclusionPredicate((d, a) =>
    {
        var actionApiVersionModel = a.ActionDescriptor.GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);
        if (actionApiVersionModel == null)
        {
            return true;
        }

        if (actionApiVersionModel.DeclaredApiVersions.Any())
        {
            return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v}" == d);
        }

        return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v}" == d);
    });

    c.OperationFilter<TimestampFormatHeader>();
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"), true);
});

builder.Host.ConfigureServices((h, s) =>
{
    s.AddATSPMDbContext(h);
    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var desc in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"../swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

//FileExtensionContentTypeProvider

//public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
//{
//    readonly IApiVersionDescriptionProvider provider;

//    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) =>
//      this.provider = provider;

//    public void Configure(SwaggerGenOptions options)
//    {
//        foreach (var desc in provider.ApiVersionDescriptions)
//        {
//            Console.WriteLine($"v{desc.GroupName} - {desc.ApiVersion}");

//            options.SwaggerDoc(desc.GroupName, new()
//            {
//                Title = "Controller Event Log API",
//                Version = desc.ApiVersion.ToString(),
//                Description = "API to interact with controller event log data",
//                Contact = new()
//                {
//                    Name = "udotdevelopment",
//                    Email = "udotdevelopment@gmail.com",
//                    Url = new Uri("https://udottraffic.utah.gov/atspm/")
//                }
//            });
//        }
//    }
//}
