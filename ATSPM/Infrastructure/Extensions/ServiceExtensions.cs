using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ATSPM.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAtspmDbContext(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddDbContext<ConfigContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<AggregationContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(AggregationContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<EventLogContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<SpeedContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(SpeedContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<LegacyEventLogContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(LegacyEventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            return services;
        }

        public static IServiceCollection ConfigureSignalControllerDownloaders(this IServiceCollection services, HostBuilderContext host)
        {
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(ASC3SignalControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(ASC3SignalControllerDownloader)}"));
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltSignalControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(CobaltSignalControllerDownloader)}"));
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeSignalControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(MaxTimeSignalControllerDownloader)}"));
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(EOSSignalControllerDownloader)}"));
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltSignalControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(NewCobaltSignalControllerDownloader)}"));

            return services;
        }

        public static IServiceCollection ConfigureSignalControllerDecoders(this IServiceCollection services, HostBuilderContext host)
        {
            services.Configure<SignalControllerDecoderConfiguration>(nameof(ASCSignalControllerDecoder), host.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(ASCSignalControllerDecoder)}"));
            services.Configure<SignalControllerDecoderConfiguration>(nameof(MaxTimeSignalControllerDecoder), host.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(MaxTimeSignalControllerDecoder)}"));

            return services;
        }

        public static IServiceCollection AddAtspmEFRepositories(this IServiceCollection services)
        {
            services.AddScoped<IApproachRepository, ApproachEFRepository>();
            services.AddScoped<ISpeedEventRepository, SpeedEventEFRepository>();
            services.AddScoped<IAreaRepository, AreaEFRepository>();
            services.AddScoped<IControllerTypeRepository, ControllerTypeEFRepository>();
            services.AddScoped<IDetectionTypeRepository, DetectionTypeEFRepository>();
            services.AddScoped<IDetectorCommentRepository, DetectorCommentEFRepository>();
            services.AddScoped<IDetectorRepository, DetectorEFRepository>();
            services.AddScoped<IDirectionTypeRepository, DirectionTypeEFRepository>();
            services.AddScoped<IExternalLinksRepository, ExternalLinsEFRepository>();
            services.AddScoped<IFaqRepository, FaqEFRepository>();
            services.AddScoped<IJurisdictionRepository, JurisdictionEFRepository>();
            services.AddScoped<IMeasureCommentRepository, MeasureCommentEFRepository>();
            services.AddScoped<IMeasureOptionsRepository, MeasureOptionsEFRepository>();
            services.AddScoped<IMeasureTypeRepository, MeasureTypeEFRepository>();
            services.AddScoped<IMenuItemReposiotry, MenuItemEFRepository>();
            services.AddScoped<IRegionsRepository, RegionEFRepository>();
            services.AddScoped<IRouteRepository, RouteEFRepository>();
            services.AddScoped<IRouteSignalsRepository, RouteSignalEFRepository>();
            services.AddScoped<ISettingsRepository, SettingsEFRepository>();
            services.AddScoped<ISignalRepository, SignalEFRepository>();

            return services;
        }
    }
}
