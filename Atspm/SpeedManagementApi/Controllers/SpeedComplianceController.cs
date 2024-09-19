using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SpeedComplianceController : SpeedBaseController<SpeedOverDistanceOptions, List<SpeedComplianceDto>>
    {
        public SpeedComplianceController(IReportService<SpeedOverDistanceOptions, List<SpeedComplianceDto>> reportService) : base(reportService)
        {
        }
    }
}