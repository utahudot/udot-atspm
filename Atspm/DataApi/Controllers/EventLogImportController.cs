#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Controllers/EventLogController.cs
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
using Utah.Udot.ATSPM.DataApi.Services;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Event log controller
    /// for querying raw device log data
    /// </summary>
    [ApiVersion("1.0")]
    [Authorize(Policy = "CanEditData")]
    public class EventLogImportController : ControllerBase
    {
        private EventLogImporterService _eventLogImporterService;

        /// <inheritdoc/>
        public EventLogImportController(EventLogImporterService eventLogImporterService)
        {
            _eventLogImporterService = eventLogImporterService;
        }

        /// <summary>
        /// Save Events for a location
        /// This must be a json file gzipped so for example it should be a file like this (7115CEL.json.gz)
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <body name="CompressedJson">This must be a json file gzipped so for example it should be a file like this (7115CEL.json.gz) </body>
        /// <returns></returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request </response>
        /// <response code="404">Resource not found</response>
        [HttpPost("[action]/{locationIdentifier}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadEventsFromCompressedJsonAsync(string locationIdentifier)
        {
            var tempFilePath = Path.GetTempFileName();
            try
            {
                // Write incoming request to temp file
                await using (var tempFileStream = System.IO.File.Create(tempFilePath))
                {
                    await Request.Body.CopyToAsync(tempFileStream);
                }

                List<IndianaEvent> events;

                // Open temp file for reading and decompress
                await using (var fileStream = System.IO.File.OpenRead(tempFilePath))
                using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
                {
                    var json = await reader.ReadToEndAsync();
                    events = JsonConvert.DeserializeObject<List<IndianaEvent>>(json);
                }

                if (events == null || events.Count == 0)
                    return BadRequest("No events found in decompressed JSON");

                foreach (var e in events)
                    e.LocationIdentifier = locationIdentifier;

                var compressedEventLogs = _eventLogImporterService.CompressEvents(locationIdentifier, events);
                bool success = await _eventLogImporterService.InsertLogsWithRetryAsync(compressedEventLogs);

                return success
                    ? Ok(new { message = "Log inserted successfully or already exists" })
                    : StatusCode(500, new { message = "Failed to insert log" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing compressed JSON: {ex.Message}");
            }
            finally
            {
                // Delete temp file AFTER all streams are disposed
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                        System.IO.File.Delete(tempFilePath);
                }
                catch
                {
                    // ignore deletion errors
                    var other = 0;
                }
            }
        }
        //The old way for smaller files.
        //public async Task<IActionResult> UploadEventsFromCompressedJsonAsync(string locationIdentifier)
        //{
        //    try
        //    {
        //        using var memoryStream = new MemoryStream();
        //        await Request.Body.CopyToAsync(memoryStream);

        //        // Rewind the stream
        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        // Decompress using GZip
        //        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        //        using var reader = new StreamReader(gzipStream, Encoding.UTF8);
        //        var json = reader.ReadToEnd();

        //        // Now deserialize the JSON
        //        var events = JsonConvert.DeserializeObject<List<IndianaEvent>>(json);
        //        foreach (var e in events)
        //        {
        //            e.LocationIdentifier = locationIdentifier;
        //        }

        //        if (events == null || events.Count == 0)
        //            return BadRequest("No events found in decompressed JSON");
        //        var compressedEventLog = _eventLogImporterService.CompressEvents(locationIdentifier, events);

        //        bool success = await _eventLogImporterService.InsertLogWithRetryAsync(compressedEventLog);

        //        if (success)
        //            return Ok(new { message = "Log inserted successfully or already exists" });
        //        else
        //            return StatusCode(500, new { message = "Failed to insert log" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error processing compressed JSON: {ex.Message}");
        //    }
        //}



        //Add in the run the aggregation for a certain location over certain dates. :P

    }
}
