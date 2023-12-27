
using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.AppoachDelay;
using ATSPM.ReportApi.Business.ApproachSpeed;
using ATSPM.ReportApi.Business.ApproachVolume;
using ATSPM.ReportApi.Business.ArrivalOnRed;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.GreenTimeUtilization;
using ATSPM.ReportApi.Business.LeftTurnGapAnalysis;
using ATSPM.ReportApi.Business.LeftTurnGapReport;
using ATSPM.ReportApi.Business.PedDelay;
using ATSPM.ReportApi.Business.PhaseTermination;
using ATSPM.ReportApi.Business.PreempDetail;
using ATSPM.ReportApi.Business.PreemptService;
using ATSPM.ReportApi.Business.PreemptServiceRequest;
using ATSPM.ReportApi.Business.PurdueCoordinationDiagram;
using ATSPM.ReportApi.Business.SplitFail;
using ATSPM.ReportApi.Business.SplitMonitor;
using ATSPM.ReportApi.Business.TimeSpaceDiagram;
using ATSPM.ReportApi.Business.TimingAndActuation;
using ATSPM.ReportApi.Business.TurningMovementCounts;
using ATSPM.ReportApi.Business.WaitTime;
using ATSPM.ReportApi.Business.Watchdog;
using ATSPM.ReportApi.Business.YellowRedActivations;
using ATSPM.ReportApi.ReportServices;
using AutoFixture;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

//// Configure Kestrel to listen on the port defined by the PORT environment variable
//var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(int.Parse(port)); // Listen for HTTP on port defined by PORT environment variable
//});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Host.ConfigureServices((h, s) =>
{
    s.AddControllers(o =>
    {
        o.ReturnHttpNotAcceptable = true;
        o.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
        o.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
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
        o.IncludeXmlComments(filePath);
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
    s.AddAtspmEFRepositories();
    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();

    //mocked report services
    //s.AddScoped(f => GenerateMoqReportServiceA<ApproachDelayOptions, ApproachDelayResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<ApproachSpeedOptions, ApproachSpeedResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<ApproachVolumeOptions, ApproachVolumeResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<ArrivalOnRedOptions, ArrivalOnRedResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<GreenTimeUtilizationOptions, GreenTimeUtilizationResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<LeftTurnGapAnalysisOptions, LeftTurnGapAnalysisResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<PedDelayOptions, PedDelayResult>());
    //s.AddScoped(f => GenerateMoqReportServiceB<PreemptDetailOptions, PreemptDetailResult>());
    //s.AddScoped(f => GenerateMoqReportServiceB<PreemptServiceOptions, PreemptServiceResult>());
    //s.AddScoped(f => GenerateMoqReportServiceB<PreemptServiceRequestOptions, PreemptServiceRequestResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<PurdueCoordinationDiagramOptions, PurdueCoordinationDiagramResult>());
    //s.AddScoped(f => GenerateMoqReportServiceB<PurduePhaseTerminationOptions, PhaseTerminationResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<SplitFailOptions, SplitFailsResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<SplitMonitorOptions, SplitMonitorResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<TimeSpaceDiagramOptions, TimeSpaceDiagramResults>());
    //s.AddScoped(f => GenerateMoqReportServiceA<TimingAndActuationsOptions, TimingAndActuationsForPhaseResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<TurningMovementCountsOptions, TurningMovementCountsResult>());
    //s.AddScoped(f => GenerateMoqReportServiceA<YellowRedActivationsOptions, YellowRedActivationsResult>());

    //s.AddScoped<TestDataUtility>();

    //report services
    s.AddScoped<IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>, ApproachDelayReportService>();
    s.AddScoped<IReportService<ApproachSpeedOptions, IEnumerable<ApproachSpeedResult>>, ApproachSpeedReportService>();
    s.AddScoped<IReportService<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>>, ApproachVolumeReportService>();
    s.AddScoped<IReportService<ArrivalOnRedOptions, IEnumerable<ArrivalOnRedResult>>, ArrivalOnRedReportService>();
    s.AddScoped<IReportService<GreenTimeUtilizationOptions, IEnumerable<GreenTimeUtilizationResult>>, GreenTimeUtilizationReportService>();
    s.AddScoped<IReportService<LeftTurnGapAnalysisOptions, IEnumerable<LeftTurnGapAnalysisResult>>, LeftTurnGapAnalysisReportService>();
    s.AddScoped<IReportService<PedDelayOptions, IEnumerable<PedDelayResult>>, PedDelayReportService>();
    s.AddScoped<IReportService<PreemptDetailOptions, PreemptDetailResult>, PreemptDetailReportService>();
    s.AddScoped<IReportService<PreemptServiceOptions, PreemptServiceResult>, PreemptServiceReportService>();
    s.AddScoped<IReportService<PreemptServiceRequestOptions, PreemptServiceRequestResult>, PreemptRequestReportService>();
    s.AddScoped<IReportService<PurdueCoordinationDiagramOptions, IEnumerable<PurdueCoordinationDiagramResult>>, PurdueCoordinationDiagramReportService>();
    s.AddScoped<IReportService<PurduePhaseTerminationOptions, PhaseTerminationResult>, PurduePhaseTerminationReportService>();
    s.AddScoped<IReportService<SplitFailOptions, IEnumerable<SplitFailsResult>>, SplitFailReportService>();
    s.AddScoped<IReportService<SplitMonitorOptions, IEnumerable<SplitMonitorResult>>, SplitMonitorReportService>();
    s.AddScoped<IReportService<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResults>>, TimeSpaceDiagramReportService>();
    s.AddScoped<IReportService<TimingAndActuationsOptions, IEnumerable<TimingAndActuationsForPhaseResult>>, TimingAndActuactionReportService>();
    s.AddScoped<IReportService<TurningMovementCountsOptions, IEnumerable<TurningMovementCountsResult>>, TurningMovementCountReportService>();
    s.AddScoped<IReportService<YellowRedActivationsOptions, IEnumerable<YellowRedActivationsResult>>, YellowRedActivationsReportService>();
    s.AddScoped<IReportService<WaitTimeOptions, IEnumerable<WaitTimeResult>>, WaitTimeReportService>();
    s.AddScoped<IReportService<WatchDogOptions, WatchDogResult>, WatchDogReportService>();

    //Chart Services
    s.AddScoped<ApproachDelayService>();
    s.AddScoped<ApproachSpeedService>();
    s.AddScoped<ApproachVolumeService>();
    s.AddScoped<ArrivalOnRedService>();
    s.AddScoped<LeftTurnGapAnalysisService>();
    s.AddScoped<LeftTurnReportPreCheckService>();
    s.AddScoped<LeftTurnVolumeAnalysisService>();
    s.AddScoped<PedDelayService>();
    s.AddScoped<GreenTimeUtilizationService>();
    s.AddScoped<PreemptServiceService>();
    s.AddScoped<PreemptServiceRequestService>();
    s.AddScoped<PurdueCoordinationDiagramService>();
    s.AddScoped<SplitFailPhaseService>();
    s.AddScoped<SplitMonitorService>();
    s.AddScoped<TimingAndActuationsForPhaseService>();
    s.AddScoped<TurningMovementCountsService>();
    s.AddScoped<WaitTimeService>();
    s.AddScoped<YellowRedActivationsService>();

    //Common Services
    s.AddScoped<PlanService>();
    s.AddScoped<LocationPhaseService>();
    s.AddScoped<CycleService>();
    s.AddScoped<PedPhaseService>();
    s.AddScoped<AnalysisPhaseCollectionService>();
    s.AddScoped<AnalysisPhaseService>();
    s.AddScoped<PreemptDetailService>();
    s.AddScoped<PhaseService>();

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







IReportService<Tin, IEnumerable<Tout>> GenerateMoqReportServiceA<Tin, Tout>()
{
    var moq = new Mock<IReportService<Tin, IEnumerable<Tout>>>();
    moq.Setup(s => s.ExecuteAsync(It.IsAny<Tin>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Fixture().CreateMany<Tout>(10));
    return moq.Object;
}

IReportService<Tin, Tout> GenerateMoqReportServiceB<Tin, Tout>()
{
    var moq = new Mock<IReportService<Tin, Tout>>();
    moq.Setup(s => s.ExecuteAsync(It.IsAny<Tin>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Fixture().Create<Tout>());
    return moq.Object;
}

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
