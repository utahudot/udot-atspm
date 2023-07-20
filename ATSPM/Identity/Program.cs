using ATSPM.Data;
using ATSPM.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
static async Task Main(string[] args)
{
        
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging((hostContext, logging) =>
        {
            // Configure logging options here
            logging.SetMinimumLevel(LogLevel.None);

            // TODO: Add a GoogleLogger section
            // LoggingServiceOptions googleOptions = hostContext.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();

            // TODO: Remove this to an extension method
            // DOTNET_ENVIRONMENT=Development, GOOGLE_APPLICATION_CREDENTIALS=M:\My Drive\ut-udot-atspm-dev-023438451801.json
            // if (hostContext.Configuration.GetValue<bool>("UseGoogleLogger"))
            // {
            //     logging.AddGoogle(new LoggingServiceOptions
            //     {
            //         ProjectId = "1022556126938",
            //         // ProjectId = "869261868126",
            //         ServiceName = AppDomain.CurrentDomain.FriendlyName,
            //         Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
            //         Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
            //     });
            // }
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddDbContext<IdentityContext>(db =>
                db.UseSqlServer(
                    hostContext.Configuration.GetConnectionString(nameof(IdentityContext)),
                    options => options.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableSensitiveDataLogging(hostContext.HostingEnvironment.IsDevelopment()));

            // Add other services here

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(hostContext.Configuration.GetSection("AzureAd"));

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
                // Add other Swagger configuration as needed
            });
        });

}


//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<IdentityContext>(db =>
//    db.UseSqlServer(
//        builder.Configuration.GetConnectionString(nameof(IdentityContext)),
//        options => options.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName))
//    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
//    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

//builder.Services.AddControllers();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
//    // Add other Swagger configuration as needed
//});

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    app.UseSwagger();
//    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1"));
//}

//app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();
