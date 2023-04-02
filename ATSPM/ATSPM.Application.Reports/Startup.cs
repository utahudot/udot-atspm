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
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ATSPM.Application.Reports
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            //Contexts
            //services.AddATSPMDbContext(h);
            services.AddDbContext<ConfigContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConfigConnectionString")));
            services.AddDbContext<EventLogContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConfigConnectionString")));
            services.AddDbContext<SpeedContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConfigConnectionString")));

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
            services.AddScoped<LeftTurnVolumeAnalysisService>();
            services.AddScoped<PerdueCoordinationDiagramService>();
            services.AddScoped<PreemptServiceService>();
            services.AddScoped<PreemptServiceRequestService>();
            services.AddScoped<SplitFailPhaseService>();

            //Common Services
            services.AddScoped<PlanService>();
            services.AddScoped<SignalPhaseService>();
            services.AddScoped<PlanSplitMonitorService>();
            services.AddScoped<CycleService>();
            services.AddScoped<SpeedDetectorService>();
            services.AddScoped<PedPhaseService>();
            services.AddScoped<PlansBaseService>();
            services.AddScoped<AnalysisPhaseCollectionService>();
            services.AddScoped<AnalysisPhaseService>();
            services.AddScoped<PreemptDetailService>();

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
        }
    }
}
