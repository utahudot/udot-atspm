using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedCompliance;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SpeedComplianceController : SpeedBaseController<SpeedComplianceOptions, List<SpeedComplianceDto>>
    {
        public SpeedComplianceController(IReportService<SpeedComplianceOptions, List<SpeedComplianceDto>> reportService) : base(reportService)
        {
        }
    }
}