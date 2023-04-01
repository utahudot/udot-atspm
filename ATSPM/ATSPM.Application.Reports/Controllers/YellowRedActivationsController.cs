using ATSPM.Application.Reports.Business.WaitTime;
using ATSPM.Application.Reports.Business.YellowRedActivations;
using ATSPM.Application.Repositories;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YellowRedActivationsController : ControllerBase
    {
        private readonly YellowRedActivationsService yellowRedActivationsService;
        private readonly IApproachRepository approachRepository;

        public YellowRedActivationsController(
            YellowRedActivationsService yellowRedActivationsService,
            IApproachRepository approachRepository)
        {
            this.yellowRedActivationsService = yellowRedActivationsService;
            this.approachRepository = approachRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public YellowRedActivationsResult Test()
        {
            Fixture fixture = new();
            YellowRedActivationsResult viewModel = fixture.Create<YellowRedActivationsResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public YellowRedActivationsResult GetChartData([FromBody] YellowRedActivationsOptions options)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            return yellowRedActivationsService.GetChartData(options, approach);
        }
    }
}
