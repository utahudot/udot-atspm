#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - %Namespace%/Program.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Asp.Versioning;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Utah.Udot.Atspm.Business.AppoachDelay;
using Utah.Udot.Atspm.Business.ApproachSpeed;
using Utah.Udot.Atspm.Business.ApproachVolume;
using Utah.Udot.Atspm.Business.ArrivalOnRed;
using Utah.Udot.Atspm.Business.GreenTimeUtilization;
using Utah.Udot.Atspm.Business.LeftTurnGapAnalysis;
using Utah.Udot.Atspm.Business.LeftTurnGapReport;
using Utah.Udot.Atspm.Business.LinkPivot;
using Utah.Udot.Atspm.Business.PedDelay;
using Utah.Udot.Atspm.Business.PhaseTermination;
using Utah.Udot.Atspm.Business.PreempDetail;
using Utah.Udot.Atspm.Business.PreemptService;
using Utah.Udot.Atspm.Business.PreemptServiceRequest;
using Utah.Udot.Atspm.Business.PurdueCoordinationDiagram;
using Utah.Udot.Atspm.Business.RampMetering;
using Utah.Udot.Atspm.Business.SplitFail;
using Utah.Udot.Atspm.Business.SplitMonitor;
using Utah.Udot.Atspm.Business.TimeSpaceDiagram;
using Utah.Udot.Atspm.Business.TimingAndActuation;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Business.WaitTime;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Business.YellowRedActivations;
using Utah.Udot.Atspm.ReportApi.DataAggregation;
using Utah.Udot.Atspm.ReportApi.ReportServices;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using Utah.Udot.ATSPM.ReportApi.ReportServices;

//gitactions: I 

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ApplyVolumeConfiguration()
    .ConfigureServices((h, s) =>
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
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;

        //Sunset policies
        o.Policies.Sunset(0.1).Effective(DateTimeOffset.Now.AddDays(60)).Link("").Title("These are only available during development").Type("text/html");

    }).AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;

        //configure query options(which cannot otherwise be configured by OData conventions)
        //o.QueryOptions.Controller<JurisdictionController>()
        //                    .Action(c => c.Get(default))
        //                        .Allow(AllowedQueryOptions.Skip | AllowedQueryOptions.Count)
        //                        .AllowTop(100);
    });

    s.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    s.AddSwaggerGen(o =>
    {
        o.IncludeXmlComments(typeof(Program));
        o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
        o.EnableAnnotations();
        o.AddJwtAuthorization();
    });

    var allowedHosts = builder.Configuration.GetSection("AllowedHosts").Get<string>() ?? "*";
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

    s.AddAtspmDbContext(h);
    s.AddAtspmEFEventLogRepositories();
    s.AddAtspmEFConfigRepositories();
    s.AddAtspmEFAggregationRepositories();

    //report services
    s.AddScoped<IReportService<AggregationOptions, IEnumerable<AggregationResult>>, AggregationReportService>();
    s.AddScoped<IReportService<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>, ApproachDelayReportService>();
    s.AddScoped<IReportService<ApproachSpeedOptions, IEnumerable<ApproachSpeedResult>>, ApproachSpeedReportService>();
    s.AddScoped<IReportService<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>>, ApproachVolumeReportService>();
    s.AddScoped<IReportService<ArrivalOnRedOptions, IEnumerable<ArrivalOnRedResult>>, ArrivalOnRedReportService>();
    s.AddScoped<IReportService<GapDurationOptions, GapDurationResult>, LeftTurnGapDurationService>();
    s.AddScoped<IReportService<GreenTimeUtilizationOptions, IEnumerable<GreenTimeUtilizationResult>>, GreenTimeUtilizationReportService>();
    s.AddScoped<IReportService<LeftTurnGapAnalysisOptions, IEnumerable<LeftTurnGapAnalysisResult>>, LeftTurnGapAnalysisReportService>();
    s.AddScoped<IReportService<LeftTurnGapDataCheckOptions, LeftTurnGapDataCheckResult>, LeftTurnGapReportDataCheckService>();
    s.AddScoped<IReportService<LeftTurnSplitFailOptions, LeftTurnSplitFailResult>, LeftTurnSplitFailService>();
    s.AddScoped<IReportService<LeftTurnGapReportOptions, IEnumerable<LeftTurnGapReportResult>>, LeftTurnGapReportService>();
    s.AddScoped<IReportService<LinkPivotOptions, LinkPivotResult>, LinkPivotReportService>();
    s.AddScoped<LinkPivotReportService>();
    s.AddScoped<IReportService<VolumeOptions, VolumeResult>, LeftTurnVolumeService>();
    s.AddScoped<IReportService<PedActuationOptions, PedActuationResult>, LeftTurnPedActuationService>();
    s.AddScoped<IReportService<PedDelayOptions, IEnumerable<PedDelayResult>>, PedDelayReportService>();
    s.AddScoped<IReportService<PeakHourOptions, PeakHourResult>, LeftTurnPeakHourService>();
    s.AddScoped<IReportService<PreemptDetailOptions, PreemptDetailResult>, PreemptDetailReportService>();
    s.AddScoped<IReportService<PreemptServiceOptions, PreemptServiceResult>, PreemptServiceReportService>();
    s.AddScoped<IReportService<PreemptServiceRequestOptions, PreemptServiceRequestResult>, PreemptRequestReportService>();
    s.AddScoped<IReportService<PurdueCoordinationDiagramOptions, IEnumerable<PurdueCoordinationDiagramResult>>, PurdueCoordinationDiagramReportService>();
    s.AddScoped<IReportService<PurduePhaseTerminationOptions, PhaseTerminationResult>, PurduePhaseTerminationReportService>();
    s.AddScoped<IReportService<RampMeteringOptions, RampMeteringResult>, RampMeteringReportService>();
    s.AddScoped<IReportService<SplitFailOptions, IEnumerable<SplitFailsResult>>, SplitFailReportService>();
    s.AddScoped<IReportService<SplitMonitorOptions, IEnumerable<SplitMonitorResult>>, SplitMonitorReportService>();
    s.AddScoped<IReportService<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResultForPhase>>, TimeSpaceDiagramReportService>();
    s.AddScoped<IReportService<TimeSpaceDiagramAverageOptions, IEnumerable<TimeSpaceDiagramAverageResult>>, TimeSpaceDiagramAverageReportService>();
    s.AddScoped<IReportService<TimingAndActuationsOptions, IEnumerable<TimingAndActuationsForPhaseResult>>, TimingAndActuactionReportService>();
    s.AddScoped<IReportService<TurningMovementCountsOptions, TurningMovementCountsResult>, TurningMovementCountReportService>();
    s.AddScoped<IReportService<YellowRedActivationsOptions, IEnumerable<YellowRedActivationsResult>>, YellowRedActivationsReportService>();
    s.AddScoped<IReportService<WaitTimeOptions, IEnumerable<WaitTimeResult>>, WaitTimeReportService>();
    s.AddScoped<IReportService<WatchDogOptions, WatchDogResult>, WatchDogReportService>();
    s.AddScoped<WatchDogDashboardReportService>();

    //AggregationResult Services
    s.AddScoped<AggregationReportService>();
    s.AddScoped<ApproachDelayService>();
    s.AddScoped<ApproachSpeedService>();
    s.AddScoped<ApproachVolumeService>();
    s.AddScoped<ArrivalOnRedService>();
    s.AddScoped<LeftTurnGapAnalysisService>();
    s.AddScoped<LeftTurnGapReportDataCheckService>();
    s.AddScoped<LeftTurnSplitFailService>();
    s.AddScoped<LeftTurnPedActuationService>();
    s.AddScoped<LeftTurnGapDurationService>();
    s.AddScoped<LeftTurnVolumeService>();
    s.AddScoped<LeftTurnGapReportService>();

    //s.AddScoped<LeftTurnVolumeAnalysisService>();
    s.AddScoped<PedDelayService>();
    s.AddScoped<GreenTimeUtilizationService>();
    s.AddScoped<LeftTurnPeakHourService>();
    s.AddScoped<PreemptServiceService>();
    s.AddScoped<PreemptServiceRequestService>();
    s.AddScoped<PurdueCoordinationDiagramService>();
    s.AddScoped<SplitFailPhaseService>();
    s.AddScoped<SplitMonitorService>();
    s.AddScoped<TimeSpaceDiagramForPhaseService>();
    s.AddScoped<TimeSpaceAverageService>();
    s.AddScoped<TimingAndActuationsForPhaseService>();
    s.AddScoped<TurningMovementCountsService>();
    s.AddScoped<WaitTimeService>();
    s.AddScoped<YellowRedActivationsService>();
    s.AddScoped<WatchDogReportService>();

    //Aggregation Services
    s.AddScoped<DetectorVolumeAggregationOptions>();
    s.AddScoped<ApproachSpeedAggregationOptions>();
    s.AddScoped<ApproachPcdAggregationOptions>();
    s.AddScoped<PhaseCycleAggregationOptions>();
    s.AddScoped<ApproachSplitFailAggregationOptions>();
    s.AddScoped<ApproachYellowRedActivationsAggregationOptions>();
    s.AddScoped<PreemptionAggregationOptions>();
    s.AddScoped<PriorityAggregationOptions>();
    s.AddScoped<SignalEventCountAggregationOptions>();
    s.AddScoped<PhaseTerminationAggregationOptions>();
    s.AddScoped<PhasePedAggregationOptions>();
    s.AddScoped<PhaseLeftTurnGapAggregationOptions>();
    s.AddScoped<PhaseSplitMonitorAggregationOptions>();

    //Common Services
    s.AddScoped<PlanService>();
    s.AddScoped<PedActuationService>();
    s.AddScoped<LocationPhaseService>();
    s.AddScoped<CycleService>();
    s.AddScoped<PedPhaseService>();
    s.AddScoped<GapDurationService>();
    s.AddScoped<AnalysisPhaseCollectionService>();
    s.AddScoped<AnalysisPhaseService>();
    s.AddScoped<PreemptDetailService>();
    s.AddScoped<PhaseService>();
    s.AddScoped<SplitFailService>();
    s.AddScoped<LeftTurnReportService>();
    s.AddScoped<VolumeService>();
    s.AddScoped<LinkPivotService>();
    s.AddScoped<LinkPivotPairService>();
    s.AddScoped<LinkPivotPcdService>();
    s.AddScoped<WatchDogIgnoreEventService>();
    s.AddScoped<RampMeteringService>();

    s.AddPathBaseFilter(h);

    s.AddAtspmIdentity(h);
});

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.Services.PrintHostInformation();
    app.UseDeveloperExceptionPage();
}

app.UseResponseCompression();
app.UseCors("CorsPolicy");
app.UseHttpLogging();
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    var descriptions = app.DescribeApiVersions();

    // build a swagger endpoint for each discovered API version
    foreach (var description in descriptions)
    {
        var url = $"{app.Configuration["PathBaseSettings:ApplicationPathBase"]}/swagger/{description.GroupName}/swagger.json";
        var name = description.GroupName.ToUpperInvariant();
        o.SwaggerEndpoint(url, name);
    }
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
