#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Common/UsageLoggingMiddleware.cs
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Utah.Udot.Atspm.Infrastructure.Common
{
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
