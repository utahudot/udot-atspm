using ATSPM.Identity.Business.Claims;
using ATSPM.Infrastructure.Extensions;
using Identity.Business.Accounts;
using Identity.Business.Agency;
using Identity.Business.EmailSender;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port)); // Listen for HTTP on port defined by PORT environment variable
});

builder.Host.ConfigureServices((host, services) =>
{
    services.AddAtspmDbContext(host);
    services.AddIdentity<ApplicationUser, IdentityRole>() // Use AddDefaultIdentity if you don't need roles
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

    services.AddScoped<IAgencyService, AgencyService>();
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<ClaimsService, ClaimsService>();

    services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    //.AddCookie("Cookie")
    .AddJwtBearer("JwtBearerIdentityApi", options =>
    {
        options.Authority = "https://localhost:44346"; // IdentityServer URL
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Disable audience validation
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("your_long_and_secure_key_here")),
        };
    });
    //.AddGoogle("Google", options =>
    //{
    //    options.ClientId = "***REMOVED***";
    //    options.ClientSecret = "***REMOVED***";
    //});


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
            policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin:ViewUsers");
        });
        //options.AddPolicy("EditUsers", policy =>
        //{
        //    policy.RequireAuthenticatedUser();
        //    policy.RequireClaim("Admin:ViewUsers", "Admin:EditUsers");
        //});
        //options.AddPolicy("DeleteUsers", policy =>
        //{
        //    policy.RequireAuthenticatedUser();
        //    policy.RequireClaim("Admin:ViewUsers", "Admin:DeleteUsers");
        //});
        //options.AddPolicy("ViewRoles", policy =>
        //{
        //    policy.RequireAuthenticatedUser();
        //    policy.RequireClaim("Admin:ViewRoles", "Admin:ViewRoles");
        //});
        //options.AddPolicy("EditRoles", policy =>
        //{
        //    policy.RequireAuthenticatedUser();
        //    policy.RequireClaim("Admin:EditRoles", "Admin:EditRoles");
        //});
        //options.AddPolicy("DeleteRoles", policy =>
        //{
        //    policy.RequireAuthenticatedUser();
        //    policy.RequireClaim("Admin:DeleteRoles", "Admin:DeleteRoles");
        //});
        //options.AddPolicy("CreateRoles", policy =>
        //{
        //    policy.RequireAuthenticatedUser();
        //    policy.RequireClaim("Admin:CreateRoles", "Admin:CreateRoles");
        //});
        //options.AddPolicy("RequireValidToken", policy =>
        //{
        //    policy.RequireAuthenticatedUser();
        //    policy.RequireClaim("scope", "identityApi");
        //});
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
            //var configContext = services.GetRequiredService<IdentityConfigurationContext>();

            // Get UserManager and RoleManager instances
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Run the seed method for configuration data
            //ConfigurationSeedData.Seed(configContext);

            // Run the seed method for users and roles
            //await ConfigurationSeedData.SeedUsersAndRoles(userManager, roleManager);
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

app.MapControllers();


app.Run();