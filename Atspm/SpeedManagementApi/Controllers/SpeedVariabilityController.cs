using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedVariability;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SpeedVariabilityController : SpeedBaseController<SpeedVariabilityOptions, SpeedVariabilityDto>
    {
        public SpeedVariabilityController(IReportService<SpeedVariabilityOptions, SpeedVariabilityDto> reportService) : base(reportService)
        {
        }
    }
}
