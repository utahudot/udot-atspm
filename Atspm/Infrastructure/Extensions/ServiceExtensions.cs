#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Extensions/ServiceExtensions.cs
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

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Utility;
using Utah.Udot.Atspm.Infrastructure.Repositories;
using Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories;
using Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.MySqlDatabaseProvider;
using Utah.Udot.Atspm.OracleDatabaseProvider;
using Utah.Udot.Atspm.PostgreSQLDatabaseProvider;
using Utah.Udot.Atspm.Repositories;
using Utah.Udot.Atspm.SqlDatabaseProvider;
using Utah.Udot.Atspm.SqlLiteDatabaseProvider;
using Utah.Udot.NetStandardToolkit.Authentication;


namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Microsoft.Extensions.Hosting"/> environment
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Configures the <see cref="DbContextOptionsBuilder"/> with the appropriate database provider and connection string 
        /// based on the application's configuration settings.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="DbContext"/> being configured.</typeparam>
        /// <param name="builder">The builder used to configure the database context options.</param>
        /// <param name="host">The host builder context providing access to the configuration system.</param>
        /// <returns>
        /// The <see cref="DbContextOptionsBuilder"/> configured with the selected database provider. 
        /// If the provider type is unrecognized, it falls back to an in-memory database using the connection string as the identifier.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if the required configuration section for the context type is missing.</exception>
        internal static DbContextOptionsBuilder GetDbProviderInfo<T>(this DbContextOptionsBuilder builder, HostBuilderContext host) where T : DbContext
        {
            var settings = host.Configuration
                .GetSection($"DatabaseConfiguration:{typeof(T).Name}")
                .Get<DatabaseConfiguration>() ?? throw new InvalidOperationException($"Configuration section 'DatabaseConfiguration:{typeof(T).Name}' is missing.");

            var connectionString = settings.BuildConnectionString();

            return settings.DBType.ToLower() switch
            {
                "sqlserver" => builder.UseSqlServer(connectionString,
                    o => o.MigrationsAssembly(SqlServerProvider.Migration)),

                "postgresql" => builder.UseNpgsql(connectionString,
                    o => o.MigrationsAssembly(PostgreSQLProvider.Migration)),

                "mysql" => builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    o => o.MigrationsAssembly(MySqlProvider.Migration)),

                "oracle" => builder.UseOracle(connectionString,
                    o => o.MigrationsAssembly(OracleProvider.Migration)),

                "sqlite" => builder.UseSqlite(connectionString,
                    o => o.MigrationsAssembly(SqlLiteProvider.Migration)),

                _ => builder.UseInMemoryDatabase(connectionString)
            };
        }

        /// <summary>
        /// Configures default settings for a <see cref="DbContext"/>, including provider information, 
        /// query tracking behavior, and sensitive data logging based on the environment.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="DbContext"/> being configured.</typeparam>
        /// <param name="builder">The options builder to configure.</param>
        /// <param name="host">The host builder context providing configuration and environment information.</param>
        /// <param name="tracking">The query tracking behavior to apply. Defaults to <see cref="QueryTrackingBehavior.TrackAll"/>.</param>
        /// <returns>The configured <see cref="DbContextOptionsBuilder"/>.</returns>
        public static DbContextOptionsBuilder DbDefaults<T>(this DbContextOptionsBuilder builder, HostBuilderContext host, QueryTrackingBehavior tracking = QueryTrackingBehavior.TrackAll) where T : DbContext
        {
            return builder.GetDbProviderInfo<T>(host)
                .UseQueryTrackingBehavior(tracking)
                .EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment());
        }

        /// <summary>
        /// Registers and configures the ATSPM database contexts, identity services, 
        /// and health checks within the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="host">The host builder context providing access to configuration sections.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAtspmDbContext(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService<JwtUserSession>, JwtCurrentUserService>();
            services.AddScoped<AuditPropertiesInterceptor>();

            string[] contexts = { nameof(ConfigContext), nameof(AggregationContext), nameof(EventLogContext), nameof(IdentityContext) };
            foreach (var contextName in contexts)
            {
                services.Configure<DatabaseConfiguration>(contextName, host.Configuration.GetSection($"DatabaseConfiguration:{contextName}"));
            }

            services.AddDbContext<ConfigContext>((s, db) =>
            {
                db.DbDefaults<ConfigContext>(host);
                db.AddInterceptors(s.GetRequiredService<AuditPropertiesInterceptor>());
            });

            services.AddDbContext<AggregationContext>(db => db.DbDefaults<AggregationContext>(host, QueryTrackingBehavior.NoTracking));
            services.AddDbContext<EventLogContext>(db => db.DbDefaults<EventLogContext>(host, QueryTrackingBehavior.NoTracking));
            services.AddDbContext<IdentityContext>(db => db.DbDefaults<IdentityContext>(host, QueryTrackingBehavior.NoTracking));

            services.AddHealthChecks()
                .AddDbContextCheck<ConfigContext>()
                .AddDbContextCheck<AggregationContext>()
                .AddDbContextCheck<EventLogContext>()
                .AddDbContextCheck<IdentityContext>();

            return services;
        }

        /// <summary>
        /// Adds all repositories that belong to <see cref="ConfigContext"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmEFConfigRepositories(this IServiceCollection services)
        {
            services.AddScoped<IApproachRepository, ApproachEFRepository>();
            services.AddScoped<IAreaRepository, AreaEFRepository>();
            services.AddScoped<IUsageEntryRepository, UsageEntryEFRepository>();
            services.AddScoped<IDetectionTypeRepository, DetectionTypeEFRepository>();
            services.AddScoped<IDetectorCommentRepository, DetectorCommentEFRepository>();
            services.AddScoped<IDetectorRepository, DetectorEFRepository>();
            services.AddScoped<IDeviceConfigurationRepository, DeviceConfigurationEFRepository>();
            services.AddScoped<IDeviceRepository, DeviceEFRepository>();
            services.AddScoped<IDirectionTypeRepository, DirectionTypeEFRepository>();
            services.AddScoped<IFaqRepository, FaqEFRepository>();
            services.AddScoped<IJurisdictionRepository, JurisdictionEFRepository>();
            services.AddScoped<ILocationRepository, LocationEFRepository>();
            services.AddScoped<ILocationTypeRepository, LocationTypeEFRepository>();
            services.AddScoped<IMeasureCommentRepository, MeasureCommentEFRepository>();
            services.AddScoped<IMeasureOptionsRepository, MeasureOptionsEFRepository>();
            services.AddScoped<IMeasureOptionPresetRepository, MeasureOptionPresetEFRepository>();
            services.AddScoped<IMeasureTypeRepository, MeasureTypeEFRepository>();
            services.AddScoped<IMenuItemReposiotry, MenuItemEFRepository>();
            services.AddScoped<IProductRepository, ProductEFRepository>();
            services.AddScoped<IRegionsRepository, RegionEFRepository>();
            services.AddScoped<IRouteDistanceRepository, RouteDistanceEFRepository>();
            services.AddScoped<IRouteLocationsRepository, RouteLocationEFRepository>();
            services.AddScoped<IRouteRepository, RouteEFRepository>();
            services.AddScoped<IUserAreaRepository, UserAreaEFRepository>();
            services.AddScoped<IUserJurisdictionRepository, UserJurisdictionEFRepository>();
            services.AddScoped<IUserRegionRepository, UserRegionEFRepository>();
            services.AddScoped<IWatchDogEventLogRepository, WatchDogLogEventEFRepository>();
            services.AddScoped<IWatchDogIgnoreEventRepository, WatchDogIgnoreEventEFRepository>();

            return services;
        }

        /// <summary>
        /// Adds all repositories that belong to <see cref="EventLogContext"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmEFEventLogRepositories(this IServiceCollection services)
        {
            services.AddScoped<IEventLogRepository, EventLogEFRepository>();

            services.AddScoped<IIndianaEventLogRepository, IndianaEventLogEFRepository>();
            services.AddScoped<ISpeedEventLogRepository, SpeedEventLogEFRepository>();

            return services;
        }

        /// <summary>
        /// Adds all repositories that belong to <see cref="AggregationContext"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmEFAggregationRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAggregationRepository, AggregationEFRepository>();

            services.AddScoped<IApproachPcdAggregationRepository, ApproachPcdAggregationEFRepository>();
            services.AddScoped<IApproachSpeedAggregationRepository, ApproachSpeedAggregationEFRepository>();
            services.AddScoped<IApproachSplitFailAggregationRepository, ApproachSplitFailAggregationEFRepository>();
            services.AddScoped<IApproachYellowRedActivationAggregationRepository, ApproachYellowRedActivationAggregationEFRepository>();
            services.AddScoped<IDetectorEventCountAggregationRepository, DetectorEventCountAggregationEFRepository>();
            services.AddScoped<IPhaseCycleAggregationRepository, PhaseCycleAggregationEFRepository>();
            services.AddScoped<IPhaseLeftTurnGapAggregationRepository, PhaseLeftTurnGapAggregationEFRepository>();
            services.AddScoped<IPhasePedAggregationRepository, PhasePedAggregationEFRepository>();
            services.AddScoped<IPhaseSplitMonitorAggregationRepository, PhaseSplitMonitorAggregationEFRepository>();
            services.AddScoped<IPhaseTerminationAggregationRepository, PhaseTerminationAggregationEFRepository>();
            services.AddScoped<IPreemptionAggregationRepository, PreemptionAggregationEFRepository>();
            services.AddScoped<IPriorityAggregationRepository, PriorityAggregationEFRepository>();
            services.AddScoped<ISignalEventCountAggregationRepository, SignalEventCountAggregationEFRepository>();
            services.AddScoped<ISignalPlanAggregationRepository, SignalPlanAggregationEFRepository>();

            return services;
        }

        /// <summary>
        /// Adds all implementations of <see cref="IDownloaderClient"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDownloaderClients(this IServiceCollection services)
        {
            return services.RegisterServicesByInterface<IDownloaderClient>();
        }

        /// <summary>
        /// Adds all implementations of <see cref="IDeviceDownloader"/> which have corresponding <see cref="DeviceDownloaderConfiguration"/> definitions
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddDeviceDownloaders(this IServiceCollection services, HostBuilderContext host)
        {
            return services.RegisterServicesByInterfaceAndConfiguration<IDeviceDownloader, DeviceDownloaderConfiguration>(host, ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Adds all implementations of <see cref="IEventLogDecoder"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventLogDecoders(this IServiceCollection services)
        {
            return services.RegisterServicesByInterface<IEventLogDecoder>();
        }

        /// <summary>
        /// Adds all implementations of <see cref="IEventLogImporter"/> which have corresponding <see cref="EventLogImporterConfiguration"/> definitions
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventLogImporters(this IServiceCollection services, HostBuilderContext host)
        {
            return services.RegisterServicesByInterfaceAndConfiguration<IEventLogImporter, EventLogImporterConfiguration>(host, ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Adds all implementations of <see cref="IEmailService"/> which have corresponding <see cref="EmailConfiguration"/> definitions
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddEmailServices(this IServiceCollection services, HostBuilderContext host)
        {
            //return services.RegisterServicesByInterfaceAndConfiguration<IEmailService, EmailConfiguration>(host);

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(m => m.GetTypes().Where(w => w.GetInterfaces().Contains(typeof(IEmailService)))).ToList();
            foreach (var t in types)
            {
                if (host.Configuration.GetSection($"{nameof(EmailConfiguration)}:{t.Name}").Exists())
                {
                    services.Add(new ServiceDescriptor(typeof(IEmailService), t, ServiceLifetime.Transient));
                    services.Configure<EmailConfiguration>(t.Name, host.Configuration.GetSection($"{nameof(EmailConfiguration)}:{t.Name}"));
                }
            }

            return services;
        }
    }
}