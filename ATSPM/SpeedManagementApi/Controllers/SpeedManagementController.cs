using ATSPM.Application.Business.RouteSpeed;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpeedManagementController: ControllerBase
    {
        private RouteSpeedService routeSpeedService;
        private RouteService routeService;

        public SpeedManagementController(RouteSpeedService routeSpeedService, RouteService routeService)
        {
            this.routeSpeedService = routeSpeedService;
            this.routeService = routeService;
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] RouteSpeedOptions item)
        {
            await this.routeSpeedService.AddPemsSpeed(item);

            return Ok();
        }

        [HttpPut("PutTestRoutes")]
        public async Task<IActionResult> Put([FromBody] RouteSpeedOptions item)
        {
            await this.routeService.AddRandomRoutes();

            return Ok();
        }

    }
}
