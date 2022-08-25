using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.DataApi.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    public class SignalsController : ATSPMDataControllerBase<Signal, int>
    {
        private readonly ConfigContext _configContext;
        private readonly ISignalRepository _repository;

        public SignalsController(ConfigContext configContext, ISignalRepository repository) : base(configContext, repository)
        {
            _configContext = configContext;
            _repository = repository;
        }

        //[HttpGet("GetTime()")]
        //public IActionResult GetTime()
        //{
        //    return Ok("Now server time is: " + DateTime.Now.ToString());
        //}
    }
}
