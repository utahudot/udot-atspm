using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SpeedViolationsController : SpeedBaseController<SpeedViolationsOptions, List<SpeedViolationsDto>>
    {
        public SpeedViolationsController(IReportService<SpeedViolationsOptions, List<SpeedViolationsDto>> reportService) : base(reportService)
        {
        }
    }
}