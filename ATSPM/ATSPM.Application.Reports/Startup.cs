using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddScoped<IChartDataService, ApproachDelayService>();
            //services.AddDbContext<MOEContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:SPM"]));
            services.AddScoped<ISignalRepository, SignalEFRepository>();
            services.AddScoped<IApproachRepository, ApproachEFRepository>();
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
