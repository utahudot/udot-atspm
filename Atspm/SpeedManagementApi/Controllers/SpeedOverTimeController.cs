using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverTime;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpeedOverTimeController : SpeedBaseController<SpeedOverTimeOptions, SpeedOverTimeDto>
    {
        public SpeedOverTimeController(IReportService<SpeedOverTimeOptions, SpeedOverTimeDto> reportService) : base(reportService)
        {
        }
    }
}
