using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Infrastructure.MySqlDatabaseProvider;
using ATSPM.Infrastructure.OracleDatabaseProvider;
using ATSPM.Infrastructure.PostgreSQLDatabaseProvider;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.SqlDatabaseProvider;
using ATSPM.Infrastructure.SqlLiteDatabaseProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ATSPM.Infrastructure.Extensions
{
    public class DatabaseOption
    {
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
    }


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
        /// Adds database contexts based connection strings and assigns database provider
        /// <seealso cref="https://learn.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmDbContext(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddDbContext<ConfigContext>(db => db.GetDbProviderInfo<ConfigContext>(host).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<AggregationContext>(db => db.GetDbProviderInfo<AggregationContext>(host).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<EventLogContext>(db => db.GetDbProviderInfo<EventLogContext>(host).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<SpeedContext>(db => db.GetDbProviderInfo<SpeedContext>(host).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));
            services.AddDbContext<IdentityContext>(db => db.GetDbProviderInfo<IdentityContext>(host).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            return services;
        }
        public static IServiceCollection AddIdentityDbContext(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddDbContext<IdentityContext>(db => db.GetDbProviderInfo<IdentityContext>(host).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment()));

            return services;
        }
        public static IServiceCollection AddAtspmAuthentication(this IServiceCollection services, HostBuilderContext host, WebApplicationBuilder builder)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   //remeber this was set to true if we need to revert it
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = builder.Configuration["Jwt:Issuer"],
                   ValidAudience = builder.Configuration["Jwt:Audience"],
                   IssuerSigningKey = new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
               };
           });

            return services;
        }
        public static IServiceCollection AddAtspmAuthorization(this IServiceCollection services, HostBuilderContext host)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanViewUsers", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Type == ClaimTypes.Role && c.Value == "User:View") ||
                            (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanEditUsers", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Type == ClaimTypes.Role && c.Value == "User:Edit") ||
                            (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanDeleteUsers", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Type == ClaimTypes.Role && c.Value == "User:Delete") ||
                            (c.Type == ClaimTypes.Role && c.Value == "Admin"))));


                options.AddPolicy("CanViewRoles", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "Role:View") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanEditRoles", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "Role:Edit") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanDeleteRoles", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "Role:Delete") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));


                options.AddPolicy("CanViewLocationConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "LocationConfiguration:View") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanEditLocationConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "LocationConfiguration:Edit") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanDeleteLocationConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "LocationConfiguration:Delete") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));


                options.AddPolicy("CanViewGeneralConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "GeneralConfiguration:View") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanEditGeneralConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "GeneralConfiguration:Edit") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanDeleteGeneralConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "GeneralConfiguration:Delete") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));


                options.AddPolicy("CanViewData", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "Data:View") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
                options.AddPolicy("CanEditData", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "Data:Edit") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));


                options.AddPolicy("CanViewWatchDog", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           (c.Type == ClaimTypes.Role && c.Value == "Watchdog:View") ||
                           (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
            });
            return services;
        }

        public static IServiceCollection ConfigureLocationControllerDownloaders(this IServiceCollection services, HostBuilderContext host)
        {
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(ASC3LocationControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(ASC3LocationControllerDownloader)}"));
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltLocationControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(CobaltLocationControllerDownloader)}"));
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeLocationControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(MaxTimeLocationControllerDownloader)}"));
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSLocationControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(EOSLocationControllerDownloader)}"));
            services.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltLocationControllerDownloader), host.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(NewCobaltLocationControllerDownloader)}"));

            return services;
        }

        public static IServiceCollection ConfigureLocationControllerDecoders(this IServiceCollection services, HostBuilderContext host)
        {
            services.Configure<SignalControllerDecoderConfiguration>(nameof(ASCLocationControllerDecoder), host.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(ASCLocationControllerDecoder)}"));
            services.Configure<SignalControllerDecoderConfiguration>(nameof(MaxTimeLocationControllerDecoder), host.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(MaxTimeLocationControllerDecoder)}"));

            return services;
        }

        public static IServiceCollection AddAtspmEFRepositories(this IServiceCollection services)
        {
            services.AddScoped<IApproachRepository, ApproachEFRepository>();
            services.AddScoped<ISpeedEventRepository, SpeedEventEFRepository>();
            services.AddScoped<IAreaRepository, AreaEFRepository>();
            services.AddScoped<IDetectionTypeRepository, DetectionTypeEFRepository>();
            services.AddScoped<IDetectorCommentRepository, DetectorCommentEFRepository>();
            services.AddScoped<IDetectorRepository, DetectorEFRepository>();
            services.AddScoped<IDeviceRepository, DeviceEFRepository>();
            services.AddScoped<IDeviceConfigurationRepository, DeviceConfigurationEFRepository>();
            services.AddScoped<IDirectionTypeRepository, DirectionTypeEFRepository>();
            services.AddScoped<IExternalLinksRepository, ExternalLinsEFRepository>();
            services.AddScoped<IFaqRepository, FaqEFRepository>();
            services.AddScoped<IJurisdictionRepository, JurisdictionEFRepository>();
            services.AddScoped<ILocationRepository, LocationEFRepository>();
            services.AddScoped<ILocationTypeRepository, LocationTypeEFRepository>();
            services.AddScoped<IMeasureCommentRepository, MeasureCommentEFRepository>();
            services.AddScoped<IMeasureOptionsRepository, MeasureOptionsEFRepository>();
            services.AddScoped<IMeasureTypeRepository, MeasureTypeEFRepository>();
            services.AddScoped<IMenuItemReposiotry, MenuItemEFRepository>();
            services.AddScoped<IProductRepository, ProductEFRepository>();
            services.AddScoped<IRegionsRepository, RegionEFRepository>();
            services.AddScoped<IRouteRepository, RouteEFRepository>();
            services.AddScoped<IRouteDistanceRepository, RouteDistanceEFRepository>();
            services.AddScoped<IRouteLocationsRepository, RouteLocationEFRepository>();
            services.AddScoped<ISettingsRepository, SettingsEFRepository>();
            services.AddScoped<IVersionHistoryRepository, VersionHistoryEFRepository>();
            services.AddScoped<IWatchDogLogEventRepository, WatchDogLogEventEFRepository>();

            return services;
        }
    }
}
