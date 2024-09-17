using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.ViolationsAndExtremeViolations;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViolationsAndExtremeViolationsController : SpeedBaseController<ViolationsAndExtremeViolationsOptions, List<ViolationsAndExtremeViolationsDto>>
    {
        public ViolationsAndExtremeViolationsController(IReportService<ViolationsAndExtremeViolationsOptions, List<ViolationsAndExtremeViolationsDto>> reportService) : base(reportService)
        {
        }
    }
}