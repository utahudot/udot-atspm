using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SpeedOverDistanceController : SpeedBaseController<SpeedOverDistanceOptions, List<SpeedOverDistanceDto>>
    {
        public SpeedOverDistanceController(IReportService<SpeedOverDistanceOptions, List<SpeedOverDistanceDto>> reportService) : base(reportService)
        {
        }
    }
}