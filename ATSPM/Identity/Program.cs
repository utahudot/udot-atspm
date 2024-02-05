using System.Security.Claims;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

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
    services.AddScoped<TokenService, TokenService>();
    services.AddScoped<RoleManager<IdentityRole>>();

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("CanViewUser", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                    (c.Type == ClaimTypes.Role && c.Value == "User:View") ||
                    (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
        options.AddPolicy("CanEditUser", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                    (c.Type == ClaimTypes.Role && c.Value == "User:Edit") ||
                    (c.Type == ClaimTypes.Role && c.Value == "Admin"))));
        options.AddPolicy("CanDeleteUser", policy =>
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();