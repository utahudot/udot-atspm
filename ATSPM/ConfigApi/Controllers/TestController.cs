using Asp.Versioning.OData;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;
using System.Net;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    //[ApiController]
    //[Route("")]
    public class TestController : ODataController
    {
        private readonly ISignalRepository _repository;

        public TestController(ISignalRepository repository)
        {
            _repository = repository;

        }

        [HttpGet("AtspmData.Test.GetTime()")]
        public IActionResult GetTime()
        {
            return Ok("Now server time is: " + DateTime.Now.ToString());
        }

        [HttpGet("api/v{version:apiVersion}/[Controller]/[action](signalIdentifier={identifier})")]
        //[ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery]
        public IActionResult GetLatestVersionOfSignal(string identifier)
        {
            var i = _repository.GetLatestVersionOfSignal(identifier);

            Console.WriteLine($"-----------------------------------------------{i.Id} - {i.Ipaddress}");

            //var i = identifier;

            if (i == null)
            {
                return NotFound(identifier);
            }

            i.Ipaddress = IPAddress.Parse(i.Ipaddress.ToString());

            return Ok(new Signal() { Ipaddress = i.Ipaddress});
        }
    }
}
