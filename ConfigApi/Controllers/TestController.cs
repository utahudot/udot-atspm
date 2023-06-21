using Microsoft.AspNetCore.Mvc;
using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ATSPM.ConfigApi.Controllers
{
    //[ApiController]
    //[Route("")]
    public class TestController : ODataController
    {
        [HttpGet("AtspmData.Test.GetTime()")]
        public IActionResult GetTime()
        {
            return Ok("Now server time is: " + DateTime.Now.ToString());
        }
    }
}
