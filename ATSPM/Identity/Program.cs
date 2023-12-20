using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Migrations.Identity;
using Identity.Business.Accounts;
using Identity.Business.Agency;
using Identity.Business.EmailSender;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port)); // Listen for HTTP on port defined by PORT environment variable
});

builder.Host.ConfigureServices((host, services) =>
{
    services.AddNpgAtspmDbContext(host);
    services.AddIdentity<ApplicationUser, IdentityRole>() // Use AddDefaultIdentity if you don't need roles
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

    services.AddIdentityServer()
    .AddOperationalStore<IdentityOperationalContext>(options =>
    {
        options.ConfigureDbContext = builder =>
            builder.UseNpgsql(
                host.Configuration.GetConnectionString(nameof(IdentityContext)),
                sql => sql.MigrationsAssembly(typeof(ServiceExtensions).Assembly.GetName().Name));
    })
     .AddConfigurationStore<IdentityConfigurationContext>(options =>
     {
         options.ConfigureDbContext = builder =>
             builder.UseNpgsql(
                 host.Configuration.GetConnectionString(nameof(IdentityContext)),
                 sql => sql.MigrationsAssembly(typeof(ServiceExtensions).Assembly.GetName().Name));
     })
        //For production
        //(options =>
        //{
        //    options.AccessTokenLifetime = TimeSpan.FromHours(1);   // Default: 1 hour
        //    options.IdentityTokenLifetime = TimeSpan.FromMinutes(20);  // Default: 20 minutes
        //    options.RefreshTokenExpiration = TokenExpiration.Absolute;
        //    options.RefreshTokenUsage = TokenUsage.OneTimeOnly;
        //    options.RefreshTokenLifetime = TimeSpan.FromDays(30);  // Default: 15 days
        //                                                           // ... other options ...
        //})
        //.AddConfigurationStore<IdentityConfigurationContext>(options =>
        //{
        //    options.ConfigureDbContext = b => b.UseSqlServer(host.Configuration.GetConnectionString(nameof(IdentityContext)));
        //})
        //.AddOperationalStore<IdentityOperationalContext>(options =>
        //{
        //    options.ConfigureDbContext = b => b.UseSqlServer(host.Configuration.GetConnectionString(nameof(IdentityContext)));
        //})
        // other configurations, like adding a signing credential...
        .AddAspNetIdentity<ApplicationUser>()
        .AddDeveloperSigningCredential();

    services.AddScoped<IAgencyService, AgencyService>();
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<IEmailService, EmailService>();

    //services.AddAuthentication("Bearer")
    //   .AddJwtBearer("Bearer", options =>
    //   {
    //       options.Authority = "https://localhost:5001"; // assuming your IdentityServer is running on this URL
    //       options.RequireHttpsMetadata = false; // set to true in production!

    //       options.Audience = "Identity"; // replace with your API resource name
    //   });
    services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("Cookie")
    .AddJwtBearer("JwtBearerIdentityApi", options =>
    {
        options.Authority = "https://localhost:44346"; // IdentityServer URL
        options.Audience = "identityApi"; // API resource name
    })
    .AddJwtBearer("JwtBearerIdentity", options =>
    {
        options.Authority = "https://localhost:44357";
        options.Audience = "Identity";
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:44357";
        options.ClientId = "PostmanTest";
        options.ResponseType = "code";
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.SaveTokens = true;
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = "768772401068-fho9ccfocp70quikb2p167jj0u9jvqjh.apps.googleusercontent.com";
        options.ClientSecret = "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";
    });

    //This is for the production certificate
    //var certificate = new X509Store(StoreName.My, StoreLocation.LocalMachine)
    //.Certificates
    //.Find(X509FindType.FindByThumbprint, "ATSPM_CERTIFICATE_THUMBPRINT", validOnly: true)
    //.OfType<X509Certificate2>()
    //.Single();

    //IdentityClient Settings
    services.AddAuthorization(options =>
    {
        options.AddPolicy("ViewUsers", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("Admin:ViewUsers", "Admin:ViewUsers");
        });
        options.AddPolicy("EditUsers", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("Admin:ViewUsers", "Admin:EditUsers");
        });
        options.AddPolicy("DeleteUsers", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("Admin:ViewUsers", "Admin:DeleteUsers");
        });
        options.AddPolicy("ViewRoles", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("Admin:ViewRoles", "Admin:ViewRoles");
        });
        options.AddPolicy("EditRoles", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("Admin:EditRoles", "Admin:EditRoles");
        });
        options.AddPolicy("DeleteRoles", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("Admin:DeleteRoles", "Admin:DeleteRoles");
        });
        options.AddPolicy("CreateRoles", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("Admin:CreateRoles", "Admin:CreateRoles");
        });
        options.AddPolicy("RequireValidToken", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", "identityApi");
        });
    });


    services.AddControllers();

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
        // Add other Swagger configuration as needed
    });

    services.ConfigureApplicationCookie(options =>
    {
        // ... other options ...

        options.Cookie.SameSite = SameSiteMode.Lax;
        //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

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
});

var app = builder.Build();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    // Create a new service scope
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var configContext = services.GetRequiredService<IdentityConfigurationContext>();

            // Get UserManager and RoleManager instances
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Run the seed method for configuration data
            ConfigurationSeedData.Seed(configContext);

            // Run the seed method for users and roles
            await ConfigurationSeedData.SeedUsersAndRoles(userManager, roleManager);
        }
        catch (Exception ex)
        {
            // Log the error if there's any
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the configuration data.");
        }
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1"));
    }
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityServer();

app.MapControllers();


app.Run();