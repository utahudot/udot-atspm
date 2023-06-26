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
        }

        // GET /Entity
        [HttpGet("GetSignalEventsBetweenDates")]
        //[EnableQuery]
        public ActionResult<List<ControllerLogArchive>> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            var result = _repository.GetSignalEventsBetweenDates(signalId, startTime, endTime);

            return Ok(result);
        }
    }
}
