using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Utah.Udot.NetStandardToolkit.Authentication;
using Utah.Udot.NetStandardToolkit.Services;

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

    public class DataDownloaderLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DataDownloaderLoggingMiddleware> _logger;
        private readonly Func<HttpContext, string>? _getUserId;
        private readonly Func<HttpContext, ControllerActionDescriptor?, bool> _useStreaming;
        private readonly Func<HttpContext, ControllerActionDescriptor?, bool> _useBuffering;

        public DataDownloaderLoggingMiddleware(
                RequestDelegate next,
                ILogger<DataDownloaderLoggingMiddleware> logger,
                Func<HttpContext, ControllerActionDescriptor?, bool>? useStreaming = null,
                Func<HttpContext, ControllerActionDescriptor?, bool>? useBuffering = null,
                Func<HttpContext, string>? getUserId = null)
        {
            _next = next;
            _logger = logger;
            _useStreaming = useStreaming ?? ((ctx, ad) => false);
            _useBuffering = useBuffering ?? ((ctx, ad) => false);
            _getUserId = getUserId ?? (ctx => ctx.User?.FindFirst("sub")?.Value);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var originalBody = context.Response.Body;

            string? errorMessage = null;
            LoggingStream? loggingStream = null;
            MemoryStream? bufferStream = null;

            var actionDescriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var actionName = actionDescriptor?.ActionName;

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
                errorMessage = ex.Message;
                throw;
            }
            finally
            {
                sw.Stop();

                var (resultCount, resultSizeBytes) = await GetResultMetricsAsync(bufferStream, loggingStream, originalBody);

                if (_useStreaming(context, actionDescriptor) || _useBuffering(context, actionDescriptor))
                {
                    var entry = CreateEntry(context, actionDescriptor, actionName, sw.ElapsedMilliseconds, resultCount, resultSizeBytes, errorMessage);
                    
                    new DataDownloaderLogMessages(_logger, entry).DataDownloadSuccessful(entry);
                    
                    Console.WriteLine($"log: {entry}");
                }

                context.Response.Body = originalBody; // restore
            }
        }

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

        private DataDownloadLog CreateEntry(
            HttpContext context,
            ControllerActionDescriptor? actionDescriptor,
            string? actionName,
            long durationMs,
            int? resultCount,
            long? resultSizeBytes,
            string? errorMessage)
        {
            return new DataDownloadLog
            {
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
                Action = actionName,
                ResultSizeBytes = resultSizeBytes,
                ResultCount = resultCount,
                Success = context.Response.StatusCode is >= 200 and < 300,
                ErrorMessage = errorMessage
            };
        }
    }
}
