#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - %Namespace%/Program.cs
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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Serialization;
using Utah.Udot.Atspm.ConfigApi.Services;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Infrastructure.Services;
using Utah.Udot.ATSPM.ConfigApi.Mappings;
using Utah.Udot.ATSPM.ConfigApi.Utility;
using Utah.Udot.NetStandardToolkit.Configuration;
using Utah.Udot.NetStandardToolkit.Extensions;
using Utah.Udot.NetStandardToolkit.Services.GitHubReleaseService;

//git 1

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ApplyVolumeConfiguration()
    .ConfigureLogging((h, l) => l.AddGoogle(h))
    .ConfigureServices((h, s) =>
    {
        s.AddControllers(o =>
        {
            o.ReturnHttpNotAcceptable = true;
            o.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
            //o.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
            //o.OutputFormatters.Add(new EventLogCsvFormatter());
            o.OutputFormatters.RemoveType<StringOutputFormatter>();
        }).AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            //o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        })
        .AddOData(o =>
        {
            o.Count().Select().OrderBy().Expand().Filter().SetMaxTop(null);
            o.RouteOptions.EnableKeyInParenthesis = false;
            o.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
            o.RouteOptions.EnablePropertyNameCaseInsensitive = true;
            o.RouteOptions.EnableQualifiedOperationCall = false;
            o.RouteOptions.EnableUnqualifiedOperationCall = true;
        });
        s.AddProblemDetails();
        s.AddConfiguredCompression(new[] { "application/json", "application/xml", "text/csv", "application/x-ndjson" });
        s.AddConfiguredSwagger(builder.Configuration, o =>
        {
            o.IncludeXmlComments(typeof(Program).Assembly);
            o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
            o.EnableAnnotations();


            //o.AddJwtAuthorization();
            o.AddAtspmSecurityDefinitions();


            o.DocumentFilter<GenerateMeasureOptionSchemas>();
        }, v =>
        v.AddOData(o => o.AddRouteComponents("api/v{version:apiVersion}"))
        .AddODataApiExplorer(o =>
        {
            o.GroupNameFormat = "'v'VVV";
            o.SubstituteApiVersionInUrl = true;
        }));
        s.AddConfiguredCors(builder.Configuration);
        s.AddHttpLogging(l =>
        {
            l.LoggingFields = HttpLoggingFields.All;
            //l.RequestHeaders.Add("My-Request-Header");
            //l.ResponseHeaders.Add("My-Response-Header");
            //l.MediaTypeOptions.AddText("application/json");
            l.RequestBodyLogLimit = 4096;
            l.ResponseBodyLogLimit = 4096;
        });
        s.AddAtspmDbContext(h);
        s.AddAtspmEFConfigRepositories();
        s.AddScoped<IRouteService, RouteService>();
        s.AddScoped<IApproachService, ApproachService>();
        s.AddPathBaseFilter(h);

        s.AddAtspmIdentity(h);






        s.Configure<GitHubReleaseConfiguration>(options =>
        {
            options.UserAgengt = "AtspmAgent";
            options.RepositoryOwner = "utahudot";
            options.RepositoryName = "udot-atspm";
        });







        s.AddHttpClient<IGitHubReleaseService, GitHubReleaseService>();

        s.AddAutoMapper(c =>
        {
            c.AddProfile<VersionMappingProfile>();
        });

        s.AddScoped<ILocationManager, LocationManager>();
        s.AddHealthChecks();
    });

var app = builder.Build();

#region Middleware Pipeline

//Error handling
if (!app.Environment.IsProduction())
{
    app.Services.PrintHostInformation();
    app.UseODataRouteDebug();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

//Security
app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

//Cross-cutting
app.UseResponseCompression();
app.UseHttpLogging();

//Swagger
app.UseConfiguredSwaggerUI();

//Endpoints
app.UseVersionedODataBatching();
app.MapControllers();
app.MapJsonHealthChecks();

#endregion

app.Run();


public static class StuffToMove
{
    public static SwaggerGenOptions AddAtspmSecurityDefinitions(this SwaggerGenOptions swaggerGenOptions)
    {
        // 1. Define the JWT Scheme
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        // 2. Define the API Key Scheme
        var apiKeySecurityScheme = new OpenApiSecurityScheme
        {
            Name = "X-API-KEY", // The actual header name the code looks for
            Description = "Enter your API Key directly (no 'Bearer' prefix needed)",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "ApiKey",
            Reference = new OpenApiReference
            {
                Id = "ApiKey", // This ID is used for the requirement below
                Type = ReferenceType.SecurityScheme
            }
        };

        // 3. Register both definitions
        swaggerGenOptions.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
        swaggerGenOptions.AddSecurityDefinition(apiKeySecurityScheme.Reference.Id, apiKeySecurityScheme);

        // 4. Require BOTH for all operations
        // Swagger will allow EITHER to satisfy the requirement if the user provides one
        swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() },
        { apiKeySecurityScheme, Array.Empty<string>() }
    });

        return swaggerGenOptions;
    }
}



