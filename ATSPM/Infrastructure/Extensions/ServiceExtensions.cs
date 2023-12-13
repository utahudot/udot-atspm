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
using System;
using ATSPM.Infrastructure.SqlDatabaseProvider;
using ATSPM.Infrastructure.PostgreSQLDatabaseProvider;

namespace ATSPM.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        internal static DbContextOptionsBuilder GetDbProviderInfo(this DbContextOptionsBuilder builder, HostBuilderContext host)
        {
            switch (host.Configuration.GetConnectionString("Provider"))
            {
                case SqlServerProvider.ProviderName:
                    {
                        return builder.UseSqlServer(host.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(SqlServerProvider.Migration));

                        break;
                    }

                case PostgreSQLProvider.ProviderName:
                    {
                        return builder.UseSqlServer(host.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(PostgreSQLProvider.Migration));

                        break;
                    }

                default:
                    {
                        return builder.UseSqlServer(host.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(SqlServerProvider.Migration));

                        break;
                    }
            }
        }

        /// <summary>
        /// Adds database contexts based connection strings and assigns database provider
        /// <seealso cref="https://learn.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmDbContext(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddDbContext<ConfigContext>(db => db.GetDbProviderInfo(host).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<AggregationContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(AggregationContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<EventLogContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<SpeedContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(SpeedContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<LegacyEventLogContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(LegacyEventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));


            //services.AddDbContext<ConfigContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            //services.AddDbContext<AggregationContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(AggregationContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            //services.AddDbContext<EventLogContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            //services.AddDbContext<SpeedContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(SpeedContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            //services.AddDbContext<LegacyEventLogContext>(db => db.UseSqlServer(host.Configuration.GetConnectionString(nameof(LegacyEventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            return services;
        }

        //HACK: this is only temporary
        public static IServiceCollection AddNpgAtspmDbContext(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddDbContext<ConfigContext>(db => db.UseNpgsql(host.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<AggregationContext>(db => db.UseNpgsql(host.Configuration.GetConnectionString(nameof(AggregationContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<EventLogContext>(db => db.UseNpgsql(host.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<SpeedContext>(db => db.UseNpgsql(host.Configuration.GetConnectionString(nameof(SpeedContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<IdentityContext>(db => db.UseNpgsql(host.Configuration.GetConnectionString(nameof(IdentityContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<IdentityConfigurationContext>(db => db.UseNpgsql(host.Configuration.GetConnectionString(nameof(IdentityContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<IdentityOperationalContext>(db => db.UseNpgsql(host.Configuration.GetConnectionString(nameof(IdentityContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            return services;
        }

        public static IServiceCollection ConfigureLocationControllerDownloaders(this IServiceCollection services, HostBuilderContext host)
        {
            services.Configure<LocationControllerDownloaderConfiguration>(nameof(ASC3LocationControllerDownloader), host.Configuration.GetSection($"{nameof(LocationControllerDownloaderConfiguration)}:{nameof(ASC3LocationControllerDownloader)}"));
            services.Configure<LocationControllerDownloaderConfiguration>(nameof(CobaltLocationControllerDownloader), host.Configuration.GetSection($"{nameof(LocationControllerDownloaderConfiguration)}:{nameof(CobaltLocationControllerDownloader)}"));
            services.Configure<LocationControllerDownloaderConfiguration>(nameof(MaxTimeLocationControllerDownloader), host.Configuration.GetSection($"{nameof(LocationControllerDownloaderConfiguration)}:{nameof(MaxTimeLocationControllerDownloader)}"));
            services.Configure<LocationControllerDownloaderConfiguration>(nameof(EOSLocationControllerDownloader), host.Configuration.GetSection($"{nameof(LocationControllerDownloaderConfiguration)}:{nameof(EOSLocationControllerDownloader)}"));
            services.Configure<LocationControllerDownloaderConfiguration>(nameof(NewCobaltLocationControllerDownloader), host.Configuration.GetSection($"{nameof(LocationControllerDownloaderConfiguration)}:{nameof(NewCobaltLocationControllerDownloader)}"));

            return services;
        }

        public static IServiceCollection ConfigureLocationControllerDecoders(this IServiceCollection services, HostBuilderContext host)
        {
            services.Configure<LocationControllerDecoderConfiguration>(nameof(ASCLocationControllerDecoder), host.Configuration.GetSection($"{nameof(LocationControllerDecoderConfiguration)}:{nameof(ASCLocationControllerDecoder)}"));
            services.Configure<LocationControllerDecoderConfiguration>(nameof(MaxTimeLocationControllerDecoder), host.Configuration.GetSection($"{nameof(LocationControllerDecoderConfiguration)}:{nameof(MaxTimeLocationControllerDecoder)}"));

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
            services.AddScoped<IRouteLocationsRepository, RouteLocationEFRepository>();
            services.AddScoped<ISettingsRepository, SettingsEFRepository>();
            services.AddScoped<ILocationRepository, LocationEFRepository>();
            services.AddScoped<IVersionHistoryRepository, VersionHistoryEFRepository>();

            return services;
        }
    }
}
