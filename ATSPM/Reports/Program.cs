using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.Business.ApproachSpeed;
using ATSPM.Application.Reports.Business.ApproachVolume;
using ATSPM.Application.Reports.Business.ArrivalOnRed;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.LeftTurnGapAnalysis;
using ATSPM.Application.Reports.Business.LeftTurnGapReport;
using ATSPM.Application.Reports.Business.PedDelay;
using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Application.Reports.Business.PreempDetail;
using ATSPM.Application.Reports.Business.PreemptService;
using ATSPM.Application.Reports.Business.SplitFail;
using ATSPM.Application.Reports.Business.SplitMonitor;
using ATSPM.Application.Reports.Business.TimingAndActuation;
using ATSPM.Application.Reports.Business.TurningMovementCounts;
using ATSPM.Application.Reports.Business.WaitTime;
using ATSPM.Application.Reports.Business.YellowRedActivations;
using ATSPM.Application.Repositories;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Reports.Business.Common;
using Reports.Business.PurdueCoordinationDiagram;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureServices((h, services) =>
{
    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true; // Enable compression for HTTPS requests
        options.Providers.Add<GzipCompressionProvider>(); // Enable GZIP compression
        options.Providers.Add<BrotliCompressionProvider>();
        //options.Providers.Add<DeflateCompressionProvider>(); // Enable Deflate compression
    });

    services.AddLogging();
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
    });
    //Contexts
    services.AddATSPMDbContext(h);



    //Repositories
    services.AddScoped<ISignalRepository, SignalEFRepository>();
    services.AddScoped<IApproachRepository, ApproachEFRepository>();
    services.AddScoped<IDetectorRepository, DetectorEFRepository>();
    services.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
    services.AddScoped<ISpeedEventRepository, SpeedEventEFRepository>();
    services.AddScoped<IDetectionTypeRepository, DetectionTypeEFRepository>();
    services.AddScoped<IPhasePedAggregationRepository, PhasePedAggregationEFRepository>();
    services.AddScoped<IApproachCycleAggregationRepository, ApproachCycleAggregationEFRepository>();
    services.AddScoped<IPhaseTerminationAggregationRepository, PhaseTerminationAggregationEFRepository>();
    services.AddScoped<IDetectorEventCountAggregationRepository, DetectorEventCountAggregationEFRepository>();
    services.AddScoped<IPhaseLeftTurnGapAggregationRepository, PhaseLeftTurnGapAggregationEFRepository>();
    services.AddScoped<IApproachSplitFailAggregationRepository, ApproachSplitFailAggregationEFRepository>();

    //Chart Services
    services.AddScoped<ApproachDelayService>();
    services.AddScoped<ApproachSpeedService>();
    services.AddScoped<ApproachVolumeService>();
    services.AddScoped<ArrivalOnRedService>();
    services.AddScoped<LeftTurnGapAnalysisService>();
    services.AddScoped<LeftTurnReportPreCheckService>();
    services.AddScoped<LeftTurnVolumeAnalysisService>();
    services.AddScoped<PedDelayService>();
    services.AddScoped<GreenTimeUtilizationService>();
    services.AddScoped<PreemptServiceService>();
    services.AddScoped<PreemptServiceRequestService>();
    services.AddScoped<PurdueCoordinationDiagramService>();
    services.AddScoped<SplitFailPhaseService>();
    services.AddScoped<SplitMonitorService>();
    services.AddScoped<TimingAndActuationsForPhaseService>();
    services.AddScoped<TurningMovementCountsService>();
    services.AddScoped<WaitTimeService>();
    services.AddScoped<YellowRedActivationsService>();

    //Common Services
    services.AddScoped<PlanService>();
    services.AddScoped<SignalPhaseService>();
    services.AddScoped<CycleService>();
    services.AddScoped<PedPhaseService>();
    services.AddScoped<AnalysisPhaseCollectionService>();
    services.AddScoped<AnalysisPhaseService>();
    services.AddScoped<PreemptDetailService>();
    services.AddScoped<PhaseService>();

    //services.AddScoped<IDetectorRepository, DetectorEFRepository>();
    //services.AddScoped<IPhasePedAggregationRepository, PhasePedAggregationRepository>();
    //services.AddScoped<IApproachCycleAggregationRepository, ApproachCycleAggregationRepository>();
    //services.AddScoped<IPhaseTerminationAggregationRepository, PhaseTerminationAggregationRepository>();
    //services.AddScoped<IDetectorEventCountAggregationRepository, DetectorEventCountAggregationRepository>();
    //services.AddScoped<IPhaseLeftTurnGapAggregationRepository, PhaseLeftTurnGapAggregationRepository>();
    //services.AddScoped<IApproachSplitFailAggregationRepository, ApproachSplitFailAggregationRepository>();

    services.AddControllers();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "ATSPM.Application.Reports", Version = "v1" });
    });
});

var app = builder.Build();
app.UseResponseCompression(); // Enable compression middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAll");
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("../swagger/v1/swagger.json", "ATSPM.Application.Reports v1"));

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();

//namespace ATSPM.Application.Reports
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
