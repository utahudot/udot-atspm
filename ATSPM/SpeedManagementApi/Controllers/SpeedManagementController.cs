using ATSPM.Application.Business.RouteSpeed;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpeedManagementController: ControllerBase
    {
        private RouteSpeedService routeSpeedService;

        public SpeedManagementController(RouteSpeedService routeSpeedService)
        {
            this.routeSpeedService = routeSpeedService;
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] RouteSpeedOptions item)
        {
            await this.routeSpeedService.AddPemsSpeed(item);

            return Ok();
        }

    }
}
