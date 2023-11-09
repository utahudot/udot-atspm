using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.DataApi.Formatters;
using ATSPM.Domain.Extensions;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        //o.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
        o.OutputFormatters.Add(new EventLogCsvFormatter());
        o.OutputFormatters.RemoveType<StringOutputFormatter>();
    })
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
        o.Policies.Sunset(0.9)
        .Effective(DateTimeOffset.Now.AddDays(60))
        .Link("policy.html")
        .Title("Versioning Policy")
        .Type("text/html");
    }).AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

    s.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    s.AddSwaggerGen(o =>
     {
         // add a custom operation filter which sets default values
         o.OperationFilter<SwaggerDefaultValues>();

         var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
         var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

         // integrate xml comments
         o.IncludeXmlComments(filePath);

         o.OperationFilter<TimestampFormatHeader>();
     });

    s.AddAtspmDbContext(h);
    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();

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

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.Services.PrintHostInformation();
}

app.UseHttpLogging();

// Configure the HTTP request pipeline.
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
app.MapControllers();
app.Run();



/// <summary>
/// Represents the OpenAPI/Swashbuckle operation filter used to document information provided, but not used.
/// </summary>
/// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
/// Once they are fixed and published, this class can be removed.</remarks>
public class SwaggerDefaultValues : IOperationFilter
{
    /// <inheritdoc />
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
                if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                {
                    response.Content.Remove(contentType);
                }
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

            if (parameter.Schema.Default == null &&
                 description.DefaultValue != null &&
                 description.DefaultValue is not DBNull &&
                 description.ModelMetadata is ModelMetadata modelMetadata)
            {
                // REF: https://github.com/Microsoft/aspnet-api-versioning/issues/429#issuecomment-605402330
                var json = JsonSerializer.Serialize(description.DefaultValue, modelMetadata.ModelType);
                parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
            }

            parameter.Required |= description.IsRequired;
        }
    }
}