using Asp.Versioning;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Domain.Extensions;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Google.Api;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureServices((h, s) =>
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

    //https://github.com/dotnet/aspnet-api-versioning/wiki/OData-Versioned-Metadata
    s.AddApiVersioning(o =>
    {
        o.ReportApiVersions = true;
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        
        //Sunset policies
        o.Policies.Sunset(0.1).Effective(DateTimeOffset.Now.AddDays(60)).Link("").Title("These are only available during development").Type("text/html");
        //o.Policies.Sunset(0.9).Effective(DateTimeOffset.Now.AddDays(60)).Link("policy.html").Title("Versioning Policy").Type("text/html");
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
            // add a custom operation filter which sets default values
            o.OperationFilter<SwaggerDefaultValues>();

            var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
            var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

            // integrate xml comments
            o.IncludeXmlComments(filePath);
        });

    s.AddAtspmDbContext(h);
    //s.AddDbContext<ConfigContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(nameof(ConfigContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).EnableSensitiveDataLogging(h.HostingEnvironment.IsDevelopment()));
    s.AddAtspmEFRepositories();


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
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // navigate to ~/$odata to determine whether any endpoints did not match an odata route template
    app.UseODataRouteDebug();
    app.Services.PrintHostInformation();
}

app.UseHttpLogging();

app.UseSwagger();
app.UseSwaggerUI(o =>
    {
        var descriptions = app.DescribeApiVersions();

        // build a swagger endpoint for each discovered API version
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            o.SwaggerEndpoint(url, name);
        }
    });

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseVersionedODataBatching();
app.UseRouting();
app.MapControllers();
app.Run();




/// <summary>
/// Represents the OpenAPI/Swashbuckle operation filter used to document the implicit API version parameter.
/// </summary>
/// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
/// Once they are fixed and published, this class can be removed.</remarks>
public class SwaggerDefaultValues : IOperationFilter
{
    /// <summary>
    /// Applies the filter to the specified operation using the given context.
    /// </summary>
    /// <param name="operation">The operation to apply the filter to.</param>
    /// <param name="context">The current operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b7cf75e7905050305b115dd96640ddd6e74c7ac9/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGenerator.cs#L383-L387
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                //Console.WriteLine($"contentType: {contentType}");

                if (contentType != "application/json" && contentType != "application/xml")
                    response.Content.Remove(contentType);

                //if (!contentType.Contains("json") && !contentType.Contains("xml"))
                //    response.Content.Remove(contentType);

                //if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                //{
                //    Console.WriteLine($"remove: {contentType}");

                //    response.Content.Remove(contentType);
                //}
            }
        }

        if (operation.Parameters == null)
        {
            return;
        }

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            parameter.Description ??= description.ModelMetadata?.Description;

            if (parameter.Schema.Default == null && description.DefaultValue != null)
            {
                // REF: https://github.com/Microsoft/aspnet-api-versioning/issues/429#issuecomment-605402330
                var json = JsonSerializer.Serialize(description.DefaultValue, description.ModelMetadata.ModelType);
                parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
            }

            parameter.Required |= description.IsRequired;
        }
    }
}