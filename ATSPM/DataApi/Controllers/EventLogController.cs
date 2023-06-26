using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using Google.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using ATSPM.Application.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using System.Collections;

namespace ATSPM.DataApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventLogController : ControllerBase
    {
        private readonly IControllerEventLogRepository _repository;
        private readonly DbContext _db;

        public EventLogController(IControllerEventLogRepository repository, EventLogContext db)
        {
            _repository = repository;
            _db = db;

            //FileExtensionContentTypeProvider
        }

        // GET /Entity
        [HttpGet("GetSignalEventsBetweenDates")]
        //[EnableQuery]
        public ActionResult<List<ControllerLogArchive>> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            var result = _repository.GetSignalEventsBetweenDates(signalId, startTime, endTime);

            return Ok(result);
        }

        [HttpGet("{signalId}")]
        public ActionResult<List<ControllerLogArchive>> GetEventLogs(string signalId, DateTime startTime, DateTime endTime)
        {
            var result = _repository.GetSignalEventsBetweenDates(signalId, startTime, endTime);

            return Ok(result);
        }

        //[HttpGet("{signalId}/{eventCode}")]
        //public ActionResult<List<ControllerLogArchive>> GetEventLogs(string signalId, int eventCode, DateTime startTime, DateTime endTime)
        //{
        //    var result = _repository.GetSignalEventsByEventCode(signalId, startTime, endTime, eventCode);

        //    return Ok(result);
        //}

        [HttpGet("{signalId}/{eventCode}")]
        public ActionResult GetEventLogs(string signalId, int eventCode, DateTime startTime, DateTime endTime)
        {
            var result = _repository.GetSignalEventsByEventCode(signalId, startTime, endTime, eventCode);

            var csv = result.Select(x => $"{x.SignalId},{x.Timestamp},{x.EventCode},{x.EventParam}");

            return File(System.Text.Encoding.UTF8.GetBytes(string.Join(",", csv)), "text/csv", "data.csv");
        }
    }
}
