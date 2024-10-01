using Asp.Versioning;
using ATSPM.Infrastructure.Services.SpeedManagementServices.CongestionTracking;
using Google.Cloud.BigQuery.V2;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SpeedManagementApi.Processors;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;
using Utah.Udot.Atspm.Business.SpeedManagement.DataQuality;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.CongestionTracking;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.EffectivenessOfStrategies;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedCompliance;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverTime;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedVariability;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedViolations;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.ViolationsAndExtremeViolations;
using Utah.Udot.Atspm.DataApi.Configuration;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;
using Utah.Udot.Atspm.Services;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.DataQuality;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.EffectivenessOrStrategies;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedCompliance;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedOverDistance;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedOverTime;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedVariability;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedViolations;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.ViolationsAndExtremeViolations;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

//// Configure Kestrel to listen on the port defined by the PORT environment variable
//var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(int.Parse(port)); // Listen for HTTP on port defined by PORT environment variable
//});
builder.Host.ConfigureServices((h, s) =>
{
    s.AddControllers(o =>
    {
        o.ReturnHttpNotAcceptable = true;
        o.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
        o.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
    })
    .AddXmlDataContractSerializerFormatters();
    s.AddProblemDetails();

    s.AddResponseCompression(o =>
    {
        o.EnableForHttps = true; // Enable compression for HTTPS requests
                                 //o.Providers.Add<GzipCompressionProvider>(); // Enable GZIP compression
                                 //o.Providers.Add<BrotliCompressionProvider>();
        o.MimeTypes = new[] { "application/json", "application/xml" };
    });
    //https://github.com/dotnet/aspnet-api-versioning/wiki/OData-Versioned-Metadata
    s.AddApiVersioning(o =>
    {
        o.ReportApiVersions = true;
        //o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.Policies.Sunset(0.9)
    .Effective(DateTimeOffset.Now.AddDays(60))
    .Link("policy.html")
    .Title("Versioning Policy")
    .Type("text/html");
    }).AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

    s.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    s.AddSwaggerGen(o =>
    {
        // add a custom operation filter which sets default values
        o.OperationFilter<SwaggerDefaultValues>();

        var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        // integrate xml comments
        //o.IncludeXmlComments(filePath);
    });
    var allowedHosts = builder.Configuration.GetSection("AllowedHosts").Get<string>();
    s.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
    builder =>
    {
        builder.WithOrigins(allowedHosts.Split(','))
           .AllowAnyMethod()
           .AllowAnyHeader();
    });
    });

    s.AddAtspmDbContext(h);
    s.AddAtspmEFEventLogRepositories();
    s.AddAtspmEFConfigRepositories();
    s.AddAtspmEFAggregationRepositories();

    s.AddAtspmAuthentication(h);
    s.AddAtspmAuthorization();

    //s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
    s.AddSingleton(provider =>
    {
        var projectId = builder.Configuration["BigQuery:ProjectId"];
        return BigQueryClient.Create(projectId);
    });
    s.AddScoped<IHourlySpeedRepository, HourlySpeedBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:HourlySpeedTableId"];
        var logger = provider.GetRequiredService<ILogger<HourlySpeedBQRepository>>();
        return new HourlySpeedBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<ISegmentRepository, SegmentBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:RouteTableId"];
        var projectId = builder.Configuration["BigQuery:ProjectId"];
        var logger = provider.GetRequiredService<ILogger<SegmentBQRepository>>();
        return new SegmentBQRepository(client, datasetId, tableId, projectId, logger);
    });
    s.AddScoped<IImpactRepository, ImpactBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:ImpactTableId"];
        var logger = provider.GetRequiredService<ILogger<ImpactBQRepository>>();
        return new ImpactBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<IImpactTypeRepository, ImpactTypeBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:ImpactTypeTableId"];
        var logger = provider.GetRequiredService<ILogger<ImpactTypeBQRepository>>();
        return new ImpactTypeBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<IImpactImpactTypeRepository, ImpactImpactTypeBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:ImpactImpactTypeTableId"];
        var logger = provider.GetRequiredService<ILogger<ImpactImpactTypeBQRepository>>();
        return new ImpactImpactTypeBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<ISegmentImpactRepository, SegmentImpactBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:SegmentImpactTableId"];
        var logger = provider.GetRequiredService<ILogger<SegmentImpactBQRepository>>();
        return new SegmentImpactBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<IMonthlyAggregationRepository, MonthlyAggregationBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:MonthlyAggregationTableId"];
        var logger = provider.GetRequiredService<ILogger<MonthlyAggregationBQRepository>>();
        return new MonthlyAggregationBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<ICityRepository, CityBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:CityTableId"];
        var logger = provider.GetRequiredService<ILogger<CityBQRepository>>();
        return new CityBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<ICountyRepository, CountyBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:CountyTableId"];
        var logger = provider.GetRequiredService<ILogger<CountyBQRepository>>();
        return new CountyBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<IRegionRepository, RegionBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:RegionTableId"];
        var logger = provider.GetRequiredService<ILogger<RegionBQRepository>>();
        return new RegionBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<IAccessCategoryRepository, AccessCategoryBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:AccessCategoryTableId"];
        var logger = provider.GetRequiredService<ILogger<AccessCategoryBQRepository>>();
        return new AccessCategoryBQRepository(client, datasetId, tableId, logger);
    });
    s.AddScoped<IFunctionalTypeRepository, FunctionalTypeBQRepository>(provider =>
    {
        var client = provider.GetRequiredService<BigQueryClient>();
        var datasetId = builder.Configuration["BigQuery:DatasetId"];
        var tableId = builder.Configuration["BigQuery:FunctionalTypeTableId"];
        var logger = provider.GetRequiredService<ILogger<FunctionalTypeBQRepository>>();
        return new FunctionalTypeBQRepository(client, datasetId, tableId, logger);
    });

    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", builder.Configuration["GoogleApplicationCredentials"]);

    s.AddScoped<RouteSpeedService>();
    s.AddScoped<SegmentService>();
    s.AddScoped<ImpactService>();
    s.AddScoped<ImpactTypeService>();
    s.AddScoped<MonthlyAggregationService>();
    s.AddScoped<HourlySpeedService>();
    s.AddScoped<AggregateMonthlyEventsProcessor>();
    s.AddScoped<DeleteOldEventsProcessor>();
    s.AddScoped<SpeedOverDistanceService>();
    s.AddScoped<IReportService<CongestionTrackingOptions, CongestionTrackingDto>, CongestionTrackingService>();
    s.AddScoped<IReportService<SpeedOverTimeOptions, SpeedOverTimeDto>, SpeedOverTimeService>();
    s.AddScoped<IReportService<SpeedOverDistanceOptions, List<SpeedOverDistanceDto>>, SpeedOverDistanceService>();
    s.AddScoped<IReportService<SpeedOverDistanceOptions, List<SpeedOverDistanceDto>>, SpeedOverDistanceService>();
    s.AddScoped<IReportService<SpeedComplianceOptions, List<SpeedComplianceDto>>, SpeedComplianceService>();
    s.AddScoped<IReportService<SpeedViolationsOptions, List<SpeedViolationsDto>>, SpeedViolationsService>();
    s.AddScoped<IReportService<EffectivenessOfStrategiesOptions, List<EffectivenessOfStrategiesDto>>, EffectivenessOfStrategiesService>();
    s.AddScoped<IReportService<DataQualityOptions, List<DataQualitySource>>, DataQualityService>();
    s.AddScoped<IReportService<ViolationsAndExtremeViolationsOptions, List<ViolationsAndExtremeViolationsDto>>, ViolationsAndExtremeViolationsService>();
    s.AddScoped<IReportService<SpeedVariabilityOptions, SpeedVariabilityDto>, SpeedVariabilityService>();
    s.AddScoped<SpeedOverDistanceService>();
    //report services


    //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-7.0
    s.AddHttpLogging(l =>
    {
        l.LoggingFields = HttpLoggingFields.All;
        //l.RequestHeaders.Add("My-Request-Header");
        //l.ResponseHeaders.Add("My-Response-Header");
        //l.MediaTypeOptions.AddText("application/json");
        l.RequestBodyLogLimit = 4096;
        l.ResponseBodyLogLimit = 4096;
    });
});

var app = builder.Build();


app.UseResponseCompression();


app.UseCors("CorsPolicy");
if (app.Environment.IsDevelopment())
{
    //app.Services.PrintHostInformation();
    app.UseDeveloperExceptionPage();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    var descriptions = app.DescribeApiVersions();

    // build a swagger endpoint for each discovered API version
    foreach (var description in descriptions)
    {
        var url = $"/swagger/{description.GroupName}/swagger.json";
        var name = description.GroupName.ToUpperInvariant();
        o.SwaggerEndpoint(url, name);
    }
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();








/// <summary>
/// Represents the OpenAPI/Swashbuckle operation filter used to document information provided, but not used.
/// </summary>
/// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
/// Once they are fixed and published, this class can be removed.</remarks>
public class SwaggerDefaultValues : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b7cf75e7905050305b115dd96640ddd6e74c7ac9/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGenerator.cs#L383-L387
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        if (operation.Parameters == null)
        {
            return;
        }

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            parameter.Description ??= description.ModelMetadata?.Description;

            if (parameter.Schema.Default == null &&
                 description.DefaultValue != null &&
                 description.DefaultValue is not DBNull &&
                 description.ModelMetadata is ModelMetadata modelMetadata)
            {
                // REF: https://github.com/Microsoft/aspnet-api-versioning/issues/429#issuecomment-605402330
                var json = System.Text.Json.JsonSerializer.Serialize(description.DefaultValue, modelMetadata.ModelType);
                parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
            }

            parameter.Required |= description.IsRequired;
        }
    }
}
