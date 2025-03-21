#region license
// Copyright 2025 Utah Departement of Transportation
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

using Asp.Versioning;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Serialization;
using Utah.Udot.Atspm.ConfigApi.Configuration;
using Utah.Udot.Atspm.ConfigApi.Services;
using Utah.Udot.Atspm.ConfigApi.Utility;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.NetStandardToolkit.Extensions;

//gitactions: II

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
            o.OutputFormatters.RemoveType<StringOutputFormatter>();
        }).AddXmlDataContractSerializerFormatters()
        .AddJsonOptions(o =>
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
        })
        // Configure JSON options to use custom DateTime converter
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
        });
        s.AddProblemDetails();

        //https://github.com/dotnet/aspnet-api-versioning/wiki/OData-Versioned-Metadata
        s.AddApiVersioning(o =>
        {
            o.ReportApiVersions = true;
            o.DefaultApiVersion = new ApiVersion(1, 0);
            o.AssumeDefaultVersionWhenUnspecified = true;

            //Sunset policies
            o.Policies.Sunset(0.1).Effective(DateTimeOffset.Now.AddDays(60)).Link("").Title("These are only available during development").Type("text/html");
        })
        .AddOData(o => o.AddRouteComponents("api/v{version:apiVersion}"))
            .AddODataApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;

                //configure query options(which cannot otherwise be configured by OData conventions)
                //o.QueryOptions.Controller<JurisdictionController>()
                //                    .Action(c => c.Get(default))
                //                        .Allow(AllowedQueryOptions.Skip | AllowedQueryOptions.Count)
                //                        .AllowTop(100);
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        builder.Services.AddSwaggerGen(o =>
        {
            o.IncludeXmlComments(typeof(Program));
            o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
            o.EnableAnnotations();
            o.AddJwtAuthorization();
        });

        var allowedHosts = builder.Configuration.GetSection("AllowedHosts").Get<string>() ?? "*";
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

        s.AddAtspmDbContext(h);
        s.AddAtspmEFConfigRepositories();
        s.AddScoped<IRouteService, RouteService>();
        s.AddScoped<IApproachService, ApproachService>();
        s.AddPathBaseFilter(h);
        s.AddAtspmIdentity(h);
    });

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    // navigate to ~/$odata to determine whether any endpoints did not match an odata route template
    app.UseODataRouteDebug();
    app.Services.PrintHostInformation();
    app.UseDeveloperExceptionPage();
}

app.UseCors("CorsPolicy");
app.UseHttpLogging();
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    var descriptions = app.DescribeApiVersions();

    // build a swagger endpoint for each discovered API version
    foreach (var description in descriptions)
    {
        var url = $"{app.Configuration["PathBaseSettings:ApplicationPathBase"]}/swagger/{description.GroupName}/swagger.json";
        var name = description.GroupName.ToUpperInvariant();
        o.SwaggerEndpoint(url, name);
    }
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseVersionedODataBatching();
app.MapControllers();

app.Run();
