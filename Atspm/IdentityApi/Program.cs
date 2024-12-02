#region license
// Copyright 2024 Utah Departement of Transportation
// for IdentityApi - %Namespace%/Program.cs
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

using Asp.Versioning;
using Identity.Business.Accounts;
using Identity.Business.Agency;
using Identity.Business.Claims;
using Identity.Business.Tokens;
using Identity.Business.Users;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ApplyVolumeConfiguration()
    .ConfigureServices((h, s) =>
{
    s.AddControllers(o =>
    {
        o.ReturnHttpNotAcceptable = true;
        o.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
        o.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
    });
    s.AddProblemDetails();

    s.AddSwaggerGen(o =>
    {
        var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        // integrate xml comments
        o.IncludeXmlComments(filePath);
        o.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Atspm Authentication Api",
            Version = "v1",
            Contact = new OpenApiContact() { Name = "udotdevelopment", Email = "udotdevelopment@gmail.com", Url = new Uri("https://udottraffic.utah.gov/atspm/") },
            License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
        });
    });

    var allowedHosts = builder.Configuration.GetSection("AllowedHosts").Get<string>();
    s.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
        builder =>
        {
            builder.WithOrigins(allowedHosts.Split(','))
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-7.0
    s.AddHttpLogging(l =>
    {
        l.LoggingFields = HttpLoggingFields.All;
        //l.RequestHeaders.Add("My-Request-Header");
        //l.ResponseHeaders.Add("My-Response-Header");
        //l.MediaTypeOptions.AddText("application/json");
        l.RequestBodyLogLimit = 4096;
        l.ResponseBodyLogLimit = 4096;
    });

    s.AddApiVersioning(o =>
    {
        o.ReportApiVersions = true;
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;

        //Sunset policies
        o.Policies.Sunset(0.1).Effective(DateTimeOffset.Now.AddDays(60)).Link("").Title("These are only available during development").Type("text/html");

    }).AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

    s.AddDbContext<IdentityContext>(h, Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking);

    s.AddIdentity<ApplicationUser, IdentityRole>() // Use AddDefaultIdentity if you don't need roles
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

    s.AddEmailServices(h);

    s.AddScoped<IAgencyService, AgencyService>();
    s.AddScoped<IAccountService, AccountService>();
    s.AddScoped<ClaimsService, ClaimsService>();
    s.AddScoped<TokenService, TokenService>();
    s.AddScoped<RoleManager<IdentityRole>>();
    s.AddScoped<UserManager<ApplicationUser>>();
    s.AddScoped<UsersService>();

    s.AddPathBaseFilter(h);

    s.AddAtspmAuthentication(h);
    s.AddAtspmAuthorization();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Services.PrintHostInformation();
    app.UseDeveloperExceptionPage();
}

app.UseCors("CorsPolicy");
app.UseHttpLogging();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint($"{app.Configuration["PathBaseSettings:ApplicationPathBase"]}/swagger/v1/swagger.json", "v1"));
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();