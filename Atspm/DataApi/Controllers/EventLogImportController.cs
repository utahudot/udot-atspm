#region license
// Copyright 2026 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Controllers/EventLogImportController.cs
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
using Utah.Udot.Atspm.DataApi.Services;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Imports gzipped Indiana event JSON into the compressed event-log store.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Policy = "CanEditData")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EventLogImportController(
        IEventLogImporterService eventLogImporterService,
        ILogger<EventLogImportController> logger) : ControllerBase
    {
        /// <summary>
        /// Saves events for a location from a request body containing a gzip-compressed JSON array.
        /// </summary>
        /// <param name="locationIdentifier">Location identifier assigned to every imported event.</param>
        /// <param name="cancelToken">Request cancellation token.</param>
        /// <returns>The import result.</returns>
        /// <response code="200">Events were inserted or already existed.</response>
        /// <response code="400">The location, gzip stream, or JSON payload was invalid.</response>
        /// <response code="500">The events could not be inserted.</response>
        [HttpPost("UploadEventsFromCompressedJson/{locationIdentifier}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadEventsFromCompressedJsonAsync(
            string locationIdentifier,
            CancellationToken cancelToken)
        {
            if (string.IsNullOrWhiteSpace(locationIdentifier))
            {
                return BadRequest("A location identifier is required.");
            }

            try
            {
                List<IndianaEvent>? events;

                await using (var gzipStream = new GZipStream(
                    Request.Body,
                    CompressionMode.Decompress,
                    leaveOpen: true))
                using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
                {
                    var json = await reader.ReadToEndAsync(cancelToken);
                    events = JsonConvert.DeserializeObject<List<IndianaEvent>>(json);
                }

                if (events == null || events.Count == 0)
                {
                    return BadRequest("No events found in decompressed JSON.");
                }

                var normalizedLocationIdentifier = locationIdentifier.Trim();
                foreach (var eventLog in events)
                {
                    eventLog.LocationIdentifier = normalizedLocationIdentifier;
                }

                var compressedEventLogs = eventLogImporterService.CompressEvents(
                    normalizedLocationIdentifier,
                    events);
                var success = await eventLogImporterService.InsertLogsWithRetryAsync(
                    compressedEventLogs,
                    cancelToken);

                return success
                    ? Ok(new { message = "Log inserted successfully or already exists" })
                    : Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Failed to insert event logs");
            }
            catch (OperationCanceledException) when (cancelToken.IsCancellationRequested)
            {
                throw;
            }
            catch (InvalidDataException ex)
            {
                return BadRequest($"Invalid gzip payload: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return BadRequest($"Invalid event JSON: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected error importing event logs for location {LocationIdentifier}",
                    locationIdentifier);
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error importing event logs");
            }
        }
    }
}
