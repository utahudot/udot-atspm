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
    }
}
