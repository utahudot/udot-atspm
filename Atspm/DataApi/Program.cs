#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - %Namespace%/Program.cs
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
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Utah.Udot.Atspm.DataApi.Configuration;
using Utah.Udot.Atspm.DataApi.CustomOperations;
using Utah.Udot.Atspm.DataApi.Formatters;

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
            //o.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
            o.OutputFormatters.Add(new EventLogCsvFormatter());
            o.OutputFormatters.RemoveType<StringOutputFormatter>();
        })
        .AddNewtonsoftJson()
        .AddXmlDataContractSerializerFormatters();
        s.AddProblemDetails();

        s.AddResponseCompression(o =>
        {
            o.EnableForHttps = true; // Enable compression for HTTPS requests
                                     //o.Providers.Add<GzipCompressionProvider>(); // Enable GZIP compression
                                     //o.Providers.Add<BrotliCompressionProvider>();

            o.MimeTypes = new[] { "application/json", "application/xml", "text/csv" };
        });

        //https://github.com/dotnet/aspnet-api-versioning/wiki/OData-Versioned-Metadata
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
            //configure query options(which cannot otherwise be configured by OData conventions)
            //o.QueryOptions.Controller<JurisdictionController>()
            //                    .Action(c => c.Get(default))
            //                        .Allow(AllowedQueryOptions.Skip | AllowedQueryOptions.Count)
            //                        .AllowTop(100);
        });

        s.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        s.AddSwaggerGen(o =>
        {
            o.IncludeXmlComments(typeof(Program));
            o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
            o.EnableAnnotations();
            o.AddJwtAuthorization();

            o.OperationFilter<TimestampFormatHeader>();
            o.DocumentFilter<GenerateAggregationSchemas>();
            o.DocumentFilter<GenerateEventSchemas>();
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
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFAggregationRepositories();

        s.AddPathBaseFilter(h);

        s.AddAtspmIdentity(h);
    });

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.Services.PrintHostInformation();
    app.UseDeveloperExceptionPage();
}

app.UseResponseCompression();
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
app.UseAuthorization();
app.MapControllers();

app.Run();