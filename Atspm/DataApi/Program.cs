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
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.Threading.RateLimiting;
using Utah.Udot.Atspm.DataApi.Configuration;
using Utah.Udot.Atspm.DataApi.CustomOperations;
using Utah.Udot.Atspm.DataApi.Formatters;
using Utah.Udot.NetStandardToolkit.Authentication;
using Utah.Udot.NetStandardToolkit.Services;

//gitactions: IIII

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ApplyVolumeConfiguration()
    .ConfigureLogging((h, l) =>
    {
        l.AddGoogle(h);
    })
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
    s.AddProblemDetails()

    // Register response compression
    .AddResponseCompression(options =>
    {
        options.EnableForHttps = true; // allow compression over HTTPS

        // Add compression providers
        options.Providers.Add<GzipCompressionProvider>();
        options.Providers.Add<BrotliCompressionProvider>();

        // Restrict to specific MIME types
        options.MimeTypes = new[]
        {
        "application/json",
        "application/xml",
        "text/csv",
        "application/x-ndjson"
    };
    });

    // Configure _provider options (optional)
    s.Configure<GzipCompressionProviderOptions>(opts =>
    {
        opts.Level = System.IO.Compression.CompressionLevel.Fastest;
    });

    s.Configure<BrotliCompressionProviderOptions>(opts =>
    {
        opts.Level = System.IO.Compression.CompressionLevel.Optimal;
    });

    s.AddConfiguredSwagger(builder.Configuration, o =>
    {
        o.IncludeXmlComments(typeof(Program));
        o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
        o.EnableAnnotations();
        o.AddJwtAuthorization();

        o.OperationFilter<TimestampFormatHeader>();
        o.OperationFilter<DataTypeEnumOperationFilter>();
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

    builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection("RateLimiting"));

    builder.Services.AddSingleton<RateLimitingPolicyService>();

    builder.Services.AddRateLimiter(options =>
    {
        var sp = builder.Services.BuildServiceProvider();
        var policyService = sp.GetRequiredService<RateLimitingPolicyService>();
        options.GlobalLimiter = policyService.CreateGlobalLimiter();
    });


    //builder.Services.AddRateLimiter(options =>
    //{
    //    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    //    {
    //        // Partition key: combine IP + user
    //        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    //        var user = httpContext.User.Identity?.Name ?? "anonymous";

    //        // Example deny list
    //        var blockedIps = new[] { "203.0.113.42", "198.51.100.77" };
    //        var blockedUsers = new[] { "baduser@example.com", "spammer", "anonymous" };

    //        // Block specific IPs outright
    //        if (blockedIps.Contains(ip))
    //        {
    //            return RateLimitPartition.GetNoLimiter($"ip:{ip}");
    //        }

    //        // Block specific users outright
    //        if (blockedUsers.Contains(user))
    //        {
    //            return RateLimitPartition.GetNoLimiter($"user:{user}");
    //        }

    //        // Otherwise apply a sliding window limiter per user+IP combo
    //        var key = $"{user}:{ip}";
    //        return RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
    //        {
    //            PermitLimit = 10,                  // max 10 requests
    //            Window = TimeSpan.FromMinutes(1),  // per 1 minute
    //            SegmentsPerWindow = 2,             // smoother distribution
    //            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    //            QueueLimit = 0
    //        });
    //    });
    //});


    s.AddAtspmDbContext(h);
    s.AddAtspmEFConfigRepositories();
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

//app.UseRateLimiter();
app.UseResponseCompression();
app.UseCors("CorsPolicy");
app.UseHttpLogging();
app.UseConfiguredSwaggerUI();
app.UseMiddleware<DownloadLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();








public class SwaggerSettings
{
    public string Title { get; set; }
    public string Description { get; set; }
    public ContactSettings Contact { get; set; }
    public LicenseSettings License { get; set; }
}

public class ContactSettings
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Url { get; set; }
}

public class LicenseSettings
{
    public string Name { get; set; }
    public string Url { get; set; }
}

public class PathBaseSettings
{
    public string ApplicationPathBase { get; set; }
}

public class SwaggerUISettings
{
    public string DocExpansion { get; set; }
    public int DefaultModelsExpandDepth { get; set; }
    public bool DisplayRequestDuration { get; set; }
}

public class SunsetPolicySettings
{
    public int MajorVersion { get; set; }
    public int MinorVersion { get; set; }
    public int EffectiveDays { get; set; }
    public string Link { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
}

public class ApiVersioningSettings
{
    public int DefaultMajorVersion { get; set; }
    public int DefaultMinorVersion { get; set; }
    public List<SunsetPolicySettings> SunsetPolicies { get; set; }
}








/// <summary>
/// Adds and configures Swagger, API versioning, and related settings for the application.
/// </summary>
/// <param name="services">
/// The <see cref="IServiceCollection"/> to which Swagger and API versioning services are added.
/// </param>
/// <param name="config">
/// The application <see cref="IConfiguration"/> instance used to bind Swagger, 
/// API versioning, path base, and UI settings from <c>appsettings.json</c>.
/// </param>
/// <param name="setupAction">
/// An optional delegate to further configure <see cref="SwaggerGenOptions"/> 
/// (e.g., XML comments, operation filters, document filters).
/// </param>
/// <returns>
public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredSwagger(this IServiceCollection services, IConfiguration config, Action<SwaggerGenOptions> setupAction = null)
    {
        services.Configure<SwaggerSettings>(config.GetSection("Swagger"));
        services.Configure<PathBaseSettings>(config.GetSection("PathBaseSettings"));
        services.Configure<SwaggerUISettings>(config.GetSection("SwaggerUI"));
        services.Configure<ApiVersioningSettings>(config.GetSection("ApiVersioning"));

        services.AddApiVersioning(o =>
        {
            var versioning = config.GetSection("ApiVersioning").Get<ApiVersioningSettings>();
            o.ReportApiVersions = true;
            o.DefaultApiVersion = new ApiVersion(versioning.DefaultMajorVersion, versioning.DefaultMinorVersion);
            o.AssumeDefaultVersionWhenUnspecified = true;

            if (versioning.SunsetPolicies != null)
            {
                foreach (var policy in versioning.SunsetPolicies)
                {
                    var sunsetVersion = new ApiVersion(policy.MajorVersion, policy.MinorVersion);

                    o.Policies.Sunset(sunsetVersion)
                        .Effective(DateTimeOffset.Now.AddDays(policy.EffectiveDays))
                        .Link(policy.Link)
                        .Title(policy.Title)
                        .Type(policy.Type);
                }
            }
        })
        .AddApiExplorer(o =>
        {
            o.GroupNameFormat = "'v'VVV";
            o.SubstituteApiVersionInUrl = true;
        });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(setupAction);

        return services;
    }
}

public static class SwaggerApplicationBuilderExtensions
{
    /// <summary>
    /// Adds and configures Swagger and Swagger UI middleware to the application pipeline.
    /// </summary>
    /// <param name="app">
    /// The <see cref="IApplicationBuilder"/> used to configure the HTTP request pipeline.
    /// </param>
    /// <returns>
    /// The same <see cref="IApplicationBuilder"/> instance so that additional calls can be chained.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>Resolves <see cref="IApiVersionDescriptionProvider"/> to discover all API versions.</description>
    ///   </item>
    ///   <item>
    ///     <description>Reads <c>PathBaseSettings</c> and <c>SwaggerUI</c> configuration from <see cref="IConfiguration"/>.</description>
    ///   </item>
    ///   <item>
    ///     <description>Registers the Swagger middleware (<c>app.UseSwagger()</c>).</description>
    ///   </item>
    ///   <item>
    ///     <description>Registers the Swagger UI middleware (<c>app.UseSwaggerUI()</c>) and builds endpoints for each API version.</description>
    ///   </item>
    ///   <item>
    ///     <description>Applies UI settings such as document expansion, model expand depth, and request duration display.</description>
    ///   </item>
    /// </list>
    /// <para>
    /// The first Swagger endpoint registered will be the default document shown when Swagger UI loads.
    /// </para>
    /// </remarks>
    public static IApplicationBuilder UseConfiguredSwaggerUI(this IApplicationBuilder app)
    {
        var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
        var descriptions = provider.ApiVersionDescriptions;
        var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var pathBase = config.GetValue<string>("PathBaseSettings:ApplicationPathBase");
        var uiSettings = config.GetSection("SwaggerUI").Get<SwaggerUISettings>();

        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            foreach (var description in descriptions)
            {
                var url = $"{pathBase}/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                o.SwaggerEndpoint(url, name);
            }

            o.DocExpansion(Enum.Parse<Swashbuckle.AspNetCore.SwaggerUI.DocExpansion>(uiSettings.DocExpansion));
            o.DefaultModelsExpandDepth(uiSettings.DefaultModelsExpandDepth);
            if (uiSettings.DisplayRequestDuration)
                o.DisplayRequestDuration();
        });

        return app;
    }
}





//"RateLimiting": {
//    "PermitLimit": 10,
//  "QueueLimit": 2,
//  "WindowSeconds": 30,
//  "Algorithm": "Sliding",
//  "BlockedIps": ["203.0.113.42", "198.51.100.77"],
//  "BlockedUsers": ["baduser@example.com", "spammer"]
//}

public class RateLimitingOptions
{
    public int PermitLimit { get; set; }
    public int QueueLimit { get; set; }
    public int WindowSeconds { get; set; }
    public string Algorithm { get; set; } = "Fixed"; // Fixed, Sliding, TokenBucket, Concurrency
    public List<string> BlockedIps { get; set; } = new();
    public List<string> BlockedUsers { get; set; } = new();
}


public class RateLimitingPolicyService
{
    private readonly RateLimitingOptions _options;
    private readonly ILogger<RateLimitingPolicyService> _logger;

    public RateLimitingPolicyService(IOptions<RateLimitingOptions> options,
                                     ILogger<RateLimitingPolicyService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public PartitionedRateLimiter<HttpContext> CreateGlobalLimiter()
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = httpContext.User.Identity?.Name ?? "anonymous";

            if (_options.BlockedIps.Contains(ip))
            {
                _logger.LogWarning("Blocked IP attempted access: {Ip}", ip);
                return RateLimitPartition.GetNoLimiter($"ip:{ip}");
            }

            if (_options.BlockedUsers.Contains(user))
            {
                _logger.LogWarning("Blocked user attempted access: {User}", user);
                return RateLimitPartition.GetNoLimiter($"user:{user}");
            }

            var key = $"{user}:{ip}";

            return _options.Algorithm switch
            {
                "Fixed" => RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    Window = TimeSpan.FromSeconds(_options.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                }),
                "Sliding" => RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    Window = TimeSpan.FromSeconds(_options.WindowSeconds),
                    SegmentsPerWindow = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                }),
                "TokenBucket" => RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = _options.PermitLimit,
                    TokensPerPeriod = _options.PermitLimit / 2,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(_options.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                }),
                "Concurrency" => RateLimitPartition.GetConcurrencyLimiter(key, _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                }),
                _ => RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    Window = TimeSpan.FromSeconds(_options.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                })
            };
        });
    }
}

public class DownloadLoggingMiddleware
{
    private readonly RequestDelegate _next;
    //private readonly IDownloadLogRepository _repo;
    private readonly ICurrentUserService<JwtUserSession> _currentUserService;

    public DownloadLoggingMiddleware(RequestDelegate next, ICurrentUserService<JwtUserSession> currentUserService)//, IDownloadLogRepository repo)
    {
        _next = next;
        _currentUserService = currentUserService;
        

    //_repo = repo;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        var originalBodyStream = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        string errorMessage = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            throw;
        }
        finally
        {
            sw.Stop();
            memStream.Seek(0, SeekOrigin.Begin);
            var resultSizeBytes = memStream.Length;

            // Optionally, try to parse result count if response is JSON array
            int? resultCount = null;
            if (context.Response.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    var json = await new StreamReader(memStream).ReadToEndAsync();
                    if (json.TrimStart().StartsWith("["))
                    {
                        var array = System.Text.Json.JsonDocument.Parse(json).RootElement;
                        if (array.ValueKind == System.Text.Json.JsonValueKind.Array)
                            resultCount = array.GetArrayLength();
                    }
                }
                catch { /* ignore parse errors */ }
                memStream.Seek(0, SeekOrigin.Begin);
            }

            var routeData = context.GetRouteData();
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();

            var test = _currentUserService.GetCurrentUser();

            var log = new DownloadLog
            {
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier,
                ConnectionId = context.Connection.Id,
                RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                UserId = context.User?.FindFirst("sub")?.Value ?? context.User?.Identity?.Name,
                Route = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                DurationMs = sw.ElapsedMilliseconds,
                Controller = controller,
                Action = action,
                ResultCount = resultCount,
                ResultSizeBytes = resultSizeBytes,
                Success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                ErrorMessage = errorMessage
            };

            //await _repo.LogAsync(log);

            Console.WriteLine($"log: {log}");

            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }

}

public class DownloadLog
{
    public int Id { get; set; } // Optional: for database primary key
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; }
    public string ConnectionId { get; set; }
    public string RemoteIp { get; set; }
    public string UserAgent { get; set; }
    public string UserId { get; set; }
    public string Route { get; set; }
    public string QueryString { get; set; }
    public string Method { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    // Optionally add more fields as needed:
    public string Controller { get; set; }
    public string Action { get; set; }
    public int? ResultCount { get; set; }
    public long? ResultSizeBytes { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }

    public override string? ToString()
    {
        return JsonConvert.SerializeObject(this, formatting: Formatting.Indented);
    }
}



