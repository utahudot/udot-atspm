using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// A wrapper stream that intercepts writes to an inner stream
    /// and logs metrics about the response body.
    /// Specifically designed for ND‑JSON streaming responses.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LoggingStream"/> class.
    /// </remarks>
    /// <param name="inner">The underlying stream to wrap and forward writes to.</param>
    public class LoggingStream(Stream inner) : Stream
    {
        private readonly Stream _inner = inner;
        private long _bytesWritten = 0;
        private int _ndJsonCount = 0;
        private string _lineBuffer = "";

        /// <summary>
        /// Gets the total number of bytes written through this stream.
        /// </summary>
        public long BytesWritten => _bytesWritten;

        /// <summary>
        /// Gets the number of ND‑JSON objects written.
        /// This is incremented whenever a complete line starting with '{' is detected.
        /// </summary>
        public int NdJsonCount => _ndJsonCount;

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream,
        /// increments the byte counter, and counts ND‑JSON objects by line.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write.</param>
        /// <param name="offset">The zero‑based byte offset in <paramref name="buffer"/> at which to begin copying bytes.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _bytesWritten += count;
            var chunk = Encoding.UTF8.GetString(buffer, offset, count);

            _lineBuffer += chunk;
            var lines = _lineBuffer.Split('\n');

            // Keep the last partial line in the buffer
            _lineBuffer = lines[^1];

            // Count complete ND‑JSON objects
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];
                if (!string.IsNullOrWhiteSpace(line) && line.TrimStart().StartsWith('{'))
                    _ndJsonCount++;
            }

            await _inner.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override bool CanRead => _inner.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _inner.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _inner.CanWrite;

        /// <inheritdoc />
        public override long Length => _inner.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        /// <inheritdoc />
        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

        /// <inheritdoc />
        public override void SetLength(long value) => _inner.SetLength(value);

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        /// <inheritdoc />
        public override void Flush() => _inner.Flush();
    }


    /// <summary>
    /// Middleware that logs API usage details, including request/response metadata,
    /// execution duration, and result metrics. Supports streaming and buffering modes
    /// for capturing response content.
    /// </summary>
    public class UsageLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UsageLoggingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Func<HttpContext, string>? _getUserId;
        private readonly Func<HttpContext, string>? _apiName;
        private readonly Func<HttpContext, ControllerActionDescriptor?, bool> _useStreaming;
        private readonly Func<HttpContext, ControllerActionDescriptor?, bool> _useBuffering;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsageLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger used to emit structured usage logs.</param>
        /// <param name="scopeFactory">Factory for creating service scopes to resolve repositories.</param>
        /// <param name="apiName">Optional delegate to resolve the API name for the current request. Defaults to entry assembly name.</param>
        /// <param name="useStreaming">Optional delegate to determine whether streaming mode should be used for the response.</param>
        /// <param name="useBuffering">Optional delegate to determine whether buffering mode should be used for the response.</param>
        /// <param name="getUserId">Optional delegate to resolve the user ID from the <see cref="HttpContext"/>.</param>
        public UsageLoggingMiddleware(
                RequestDelegate next,
                ILogger<UsageLoggingMiddleware> logger,
                IServiceScopeFactory scopeFactory,
                Func<HttpContext, string>? apiName = null,
                Func<HttpContext, ControllerActionDescriptor?, bool>? useStreaming = null,
                Func<HttpContext, ControllerActionDescriptor?, bool>? useBuffering = null,
                Func<HttpContext, string>? getUserId = null)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _apiName = apiName ?? (ctx => Assembly.GetEntryAssembly()?.GetName().Name);
            _useStreaming = useStreaming ?? ((ctx, ad) => false);
            _useBuffering = useBuffering ?? ((ctx, ad) => false);
            _getUserId = getUserId ?? (ctx => ctx.User?.FindFirst("sub")?.Value);
        }

        /// <summary>
        /// Invokes the middleware logic for the current HTTP request.
        /// Captures response metrics, logs usage details, and persists entries to the repository.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var originalBody = context.Response.Body;

            Exception error = null;
            LoggingStream? loggingStream = null;
            MemoryStream? bufferStream = null;

            var actionDescriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();

            try
            {
                if (_useStreaming(context, actionDescriptor))
                {
                    loggingStream = new LoggingStream(originalBody);
                    context.Response.Body = loggingStream;
                }
                else if (_useBuffering(context, actionDescriptor))
                {
                    bufferStream = new MemoryStream();
                    context.Response.Body = bufferStream;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                error = ex;
                throw;
            }
            finally
            {
                sw.Stop();

                var (resultCount, resultSizeBytes) = await GetResultMetricsAsync(bufferStream, loggingStream, originalBody);

                if (_useStreaming(context, actionDescriptor) || _useBuffering(context, actionDescriptor))
                {
                    var entry = CreateEntry(context, actionDescriptor, sw.ElapsedMilliseconds, resultCount ?? 0, resultSizeBytes, error?.Message);

                    var log = new ApiUsageLogMessage(_logger, entry);

                    if (entry.Success) log.CallSuccessful(entry);
                    if (!entry.Success) log.CallWarning(entry, entry.StatusCode);
                    if (error != null) log.CallError(entry, error);

                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repo = scope.ServiceProvider.GetService<IUsageEntryRepository>();
                            await repo.AddAsync(entry);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"stuff: {e.Message}");
                    }
                }

                context.Response.Body = originalBody; // restore
            }
        }

        /// <summary>
        /// Extracts result metrics (count and size) from the response stream.
        /// Supports both buffered JSON responses and streaming NDJSON responses.
        /// </summary>
        /// <param name="bufferStream">The buffered response stream, if buffering was enabled.</param>
        /// <param name="loggingStream">The logging stream, if streaming was enabled.</param>
        /// <param name="originalBody">The original response body stream.</param>
        /// <returns>A tuple containing the result count (if available) and the response size in bytes.</returns>
        private static async Task<(int? count, long? size)> GetResultMetricsAsync(
            MemoryStream? bufferStream,
            LoggingStream? loggingStream,
            Stream originalBody)
        {
            if (bufferStream != null)
            {
                bufferStream.Position = 0;
                await bufferStream.CopyToAsync(originalBody);

                long size = bufferStream.Length;
                int? count = null;

                try
                {
                    var json = Encoding.UTF8.GetString(bufferStream.ToArray());
                    if (json.TrimStart().StartsWith("["))
                    {
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            count = doc.RootElement.GetArrayLength();
                    }
                }
                catch { }

                return (count, size);
            }

            if (loggingStream != null)
            {
                return (loggingStream.NdJsonCount, loggingStream.BytesWritten);
            }

            return (null, null);
        }

        /// <summary>
        /// Creates a <see cref="UsageEntry"/> object representing the logged API usage details.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="actionDescriptor">The controller action descriptor, if available.</param>
        /// <param name="durationMs">The duration of the request in milliseconds.</param>
        /// <param name="resultCount">The number of items returned in the response, if available.</param>
        /// <param name="resultSizeBytes">The size of the response in bytes, if available.</param>
        /// <param name="errorMessage">The error message if an exception occurred, otherwise null.</param>
        /// <returns>A populated <see cref="UsageEntry"/> representing the API call.</returns>
        private UsageEntry CreateEntry(
            HttpContext context,
            ControllerActionDescriptor? actionDescriptor,
            long durationMs,
            int? resultCount,
            long? resultSizeBytes,
            string? errorMessage)
        {
            return new UsageEntry
            {
                ApiName = _apiName(context),
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier,
                ConnectionId = context.Connection.Id,
                RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                UserId = _getUserId(context),
                Route = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                DurationMs = durationMs,
                Controller = actionDescriptor?.ControllerName,
                Action = actionDescriptor?.ActionName,
                ResultSizeBytes = resultSizeBytes,
                ResultCount = resultCount,
                Success = context.Response.StatusCode is >= 200 and < 300,
                ErrorMessage = errorMessage
            };
        }
    }

}