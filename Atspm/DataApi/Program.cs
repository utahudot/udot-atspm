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

using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Threading.RateLimiting;
using Utah.Udot.Atspm.DataApi.CustomOperations;
using Utah.Udot.NetStandardToolkit.Authentication;
using Utah.Udot.NetStandardToolkit.Services;

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
        })
        .AddNewtonsoftJson();
        s.AddProblemDetails();
        s.AddConfiguredCompression(new[] { "application/json", "application/xml", "text/csv", "application/x-ndjson" });
        s.AddConfiguredSwagger(builder.Configuration, o =>
        {
            o.IncludeXmlComments(typeof(Program).Assembly);
            o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
            o.CustomSchemaIds(type => type.Name);
            o.EnableAnnotations();
            o.AddJwtAuthorization();

            //o.OperationFilter<TimestampFormatHeader>();
            o.OperationFilter<DataTypeEnumOperationFilter>();
            o.DocumentFilter<GenerateAggregationSchemas>();
            o.DocumentFilter<GenerateEventSchemas>();
        });
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
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFAggregationRepositories();
        s.AddPathBaseFilter(h);
        s.AddAtspmIdentity(h);
        s.AddHealthChecks();
    });

var app = builder.Build();

#region Middleware Pipeline

//Error handling
if (!app.Environment.IsProduction())
{
    app.Services.PrintHostInformation();
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
//app.UseMiddleware<DownloadLoggingMiddleware>();

//Swagger
app.UseConfiguredSwaggerUI();

//Endpoints
app.MapControllers();
app.MapJsonHealthChecks();

#endregion

app.Run();


//builder.Services.Configure<RateLimitingOptions>(
//builder.Configuration.GetSection("RateLimiting"));

//builder.Services.AddSingleton<RateLimitingPolicyService>();

//builder.Services.AddRateLimiter(options =>
//{
//    var sp = builder.Services.BuildServiceProvider();
//    var policyService = sp.GetRequiredService<RateLimitingPolicyService>();
//    options.GlobalLimiter = policyService.CreateGlobalLimiter();
//});

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

//app.UseRateLimiter();


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

public class LoggingStream : Stream
{
    private readonly Stream _inner;
    private readonly StringBuilder _buffer = new(); // for non-streaming JSON
    private long _bytesWritten = 0;
    private int _ndJsonCount = 0;
    private readonly string _contentType;

    public LoggingStream(Stream inner, string contentType)
    {
        _inner = inner;
        _contentType = contentType ?? "";
    }

    public long BytesWritten => _bytesWritten;
    public int NdJsonCount => _ndJsonCount;
    public string BufferedContent => _buffer.ToString();

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        _bytesWritten += count;
        var chunk = Encoding.UTF8.GetString(buffer, offset, count);

        if (_contentType.Contains("nd-json") || _contentType.Contains("x-ndjson"))
        {
            // Count ND-JSON objects line by line
            foreach (var line in chunk.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line) && line.TrimStart().StartsWith("{"))
                    _ndJsonCount++;
            }
        }
        else if (_contentType.Contains("application/json"))
        {
            // Buffer JSON for later parsing
            _buffer.Append(chunk);
        }

        await _inner.WriteAsync(buffer, offset, count, cancellationToken);
    }

    // Delegate other members
    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }
    public override void Flush() => _inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
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

        var originalBody = context.Response.Body;
        var loggingStream = new LoggingStream(originalBody, context.Response.ContentType);
        context.Response.Body = loggingStream;

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

            int? resultCount = null;

            // Decide based on Content-Type
            var contentType = context.Response.ContentType ?? "";
            if (contentType.Contains("application/json") && !contentType.Contains("ndjson"))
            {
                try
                {
                    var json = loggingStream.BufferedContent;
                    if (json.TrimStart().StartsWith("["))
                    {
                        var array = System.Text.Json.JsonDocument.Parse(json).RootElement;
                        if (array.ValueKind == System.Text.Json.JsonValueKind.Array)
                            resultCount = array.GetArrayLength();
                    }
                }
                catch { /* ignore parse errors */ }
            }
            else if (contentType.Contains("ndjson"))
            {
                resultCount = loggingStream.NdJsonCount;
            }

            var test = _currentUserService.GetCurrentUser();

            var endpoint = context.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

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
                Controller = actionDescriptor?.ControllerName,
                Action = actionDescriptor?.ActionName,
                ResultSizeBytes = loggingStream.BytesWritten,
                ResultCount = resultCount,
                Success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                ErrorMessage = errorMessage
            };

            Console.WriteLine($"log: {log}");

            context.Response.Body = originalBody; // restore
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
}



