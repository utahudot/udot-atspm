#region license
// Copyright 2025 Utah Departement of Transportation
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
    /// Specifies database provider and connection string
    /// </summary>
    public class DatabaseOption
    {
        /// <summary>
        /// Provider Type
        /// <list type="bullet">
        /// <item><see cref="SqlServerProvider.ProviderName"/></item>
        /// <item><see cref="PostgreSQLProvider.ProviderName"/></item>
        /// <item><see cref="SqlLiteProvider.ProviderName"/></item>
        /// <item><see cref="MySqlProvider.ProviderName"/></item>
        /// <item><see cref="OracleProvider.ProviderName"/></item>
        /// </list>
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Database connection string for given <see cref="Provider"/>
        /// </summary>
        public string ConnectionString { get; set; }
    }

    /// <summary>
    /// Extensions for <see cref="Microsoft.Extensions.Hosting"/> environment
    /// </summary>
    public static class ServiceExtensions
    {
        internal static DbContextOptionsBuilder GetDbProviderInfo<T>(this DbContextOptionsBuilder builder, HostBuilderContext host)
        {
            var opt = host.Configuration.GetSection($"ConnectionStrings:{typeof(T).Name}").Get<DatabaseOption>();

            switch (opt.Provider)
            {
                case SqlServerProvider.ProviderName:
                    {
                        return builder.UseSqlServer(opt.ConnectionString, opt => opt.MigrationsAssembly(SqlServerProvider.Migration));
                    }

                case PostgreSQLProvider.ProviderName:
                    {
                        return builder.UseNpgsql(opt.ConnectionString, opt => opt.MigrationsAssembly(PostgreSQLProvider.Migration));
                    }

                case SqlLiteProvider.ProviderName:
                    {
                        return builder.UseSqlite(opt.ConnectionString, opt => opt.MigrationsAssembly(SqlLiteProvider.Migration));
                    }

                case MySqlProvider.ProviderName:
                    {
                        return builder.UseMySql(ServerVersion.AutoDetect(opt.ConnectionString), opt => opt.MigrationsAssembly(SqlLiteProvider.Migration));
                    }

                case OracleProvider.ProviderName:
                    {
                        return builder.UseOracle(opt.ConnectionString, opt => opt.MigrationsAssembly(OracleProvider.Migration));
                    }

                default:
                    {
                        return builder.UseSqlServer(opt.ConnectionString, opt => opt.MigrationsAssembly(SqlServerProvider.Migration));
                    }
            }
        }

        /// <summary>
        /// Adds database context based on connection string and assigns database provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="host"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder DbDefaults<T>(this DbContextOptionsBuilder builder, HostBuilderContext host, QueryTrackingBehavior tracking = QueryTrackingBehavior.TrackAll) where T : DbContext
        {
            builder.GetDbProviderInfo<T>(host).UseQueryTrackingBehavior(tracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment());

            return builder;
        }

        /// <summary>
        /// Adds database contexts based connection strings and assigns database provider
        /// <seealso href="https://learn.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmDbContext(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService<JwtUserSession>, JwtCurrentUserService>();
            services.AddScoped<AuditPropertiesInterceptor>();

            services.AddDbContext<ConfigContext>((s, db) => db.DbDefaults<ConfigContext>(host).AddInterceptors(s.GetService<AuditPropertiesInterceptor>()));
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
            services.AddScoped<IVersionHistoryRepository, VersionHistoryEFRepository>();
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