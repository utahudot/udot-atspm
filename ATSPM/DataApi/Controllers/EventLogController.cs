using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.DataApi.Controllers
{
    /// <summary>
    /// Signal controller event log data
    /// </summary>
    [ApiController]

    //[ApiVersion("1.0")]
    //[ApiVersion("2.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class EventLogController : ControllerBase
    {
        private readonly IControllerEventLogRepository _repository;
        private readonly ILogger _log;

        /// <inheritdoc/>
        public EventLogController(IControllerEventLogRepository repository, ILogger<EventLogController> log)
        {
            _repository = repository;
            _log = log;
        }

        /// <summary>
        /// Get signal events between dates
        /// </summary>
        /// <param name="signalIdentifier">Signal identifier</param>
        /// <param name="start">date/time of first event</param>
        /// <param name="end">date/time of last event</param>
        /// <returns>List of ControllerEventLogs</returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (start/end range)</response>
        /// <response code="404">Resource not found</response>
        [ApiVersion("1.0")]
        [HttpGet("{signalIdentifier}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<ControllerEventLog>> GetSignalEventsBetweenDates(string signalIdentifier, DateTime start, DateTime end)
        {
            _log.LogDebug("signal: {signal} start: {start} end: {end}", signalIdentifier, start, end);

            if (start == DateTime.MinValue || end == DateTime.MinValue)
                return BadRequest("Invalid datetime range on start/end");
            
            var result = _repository.GetSignalEventsBetweenDates(signalIdentifier, start, end);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get signal events between dates with event code
        /// </summary>
        /// <param name="signalIdentifier">Signal identifier</param>
        /// <param name="eventCode">Event code number</param>
        /// <param name="start">date/time of first event</param>
        /// <param name="end">date/time of last event</param>
        /// <returns>List of ControllerEventLogs</returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (start/end range or event code)</response>
        /// <response code="404">Resource not found</response>
        [ApiVersion("1.0")]
        [HttpGet("{signalIdentifier}/{eventCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<ControllerEventLog>> GetSignalEventsByEventCode(string signalIdentifier, int eventCode, DateTime start, DateTime end)
        {
            _log.LogDebug("signal: {signal} event: {event} start: {start} end: {end}", signalIdentifier, eventCode, start, end);

            if (start == DateTime.MinValue || end == DateTime.MinValue)
                return BadRequest("Invalid datetime range on start/end");

            var result = _repository.GetSignalEventsByEventCode(signalIdentifier, start, end, eventCode);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
