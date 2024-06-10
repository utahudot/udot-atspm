using ATSPM.Identity.Business.Claims;
using ATSPM.Infrastructure.Extensions;
using Identity.Business.Accounts;
using Identity.Business.Agency;
using Identity.Business.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using ATSPM.Identity.Business.Users;
using Identity.Business.EmailSender;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureServices((host, services) =>
{
    services.AddIdentityDbContext(host);
    services.AddIdentity<ApplicationUser, IdentityRole>() // Use AddDefaultIdentity if you don't need roles
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

    services.AddAtspmAuthentication(host, builder);
    services.AddAtspmAuthorization(host);

    services.AddEmailServices(host);
    services.AddScoped<EmailService>();

    services.AddScoped<IAgencyService, AgencyService>();
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<ClaimsService, ClaimsService>();
    services.AddScoped<TokenService, TokenService>();
    services.AddScoped<RoleManager<IdentityRole>>();
    services.AddScoped<UserManager<ApplicationUser>>();
    services.AddScoped<UsersService>();


    services.AddControllers();

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
        // Add other Swagger configuration as needed
    });

    //services.ConfigureApplicationCookie(options =>
    //{
    //    // ... other options ...

    //    options.Cookie.SameSite = SameSiteMode.Lax;
    //    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //});
    var allowedHosts = builder.Configuration.GetSection("AllowedHosts").Get<string>();
    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
        builder =>
        {
            builder.WithOrigins(allowedHosts.Split(','))
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
});

var app = builder.Build();
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    // Create a new service scope
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {

            // Get UserManager and RoleManager instances
            //var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            //var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();