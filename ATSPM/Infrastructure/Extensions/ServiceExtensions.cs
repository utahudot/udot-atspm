using ATSPM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ATSPM.Infrasturcture.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddATSPMDbContext(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddDbContext<ConfigContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            services.AddDbContext<AggregationContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(AggregationContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            services.AddDbContext<EventLogContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            services.AddDbContext<SpeedContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(SpeedContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            return services;
        }
    }
}
