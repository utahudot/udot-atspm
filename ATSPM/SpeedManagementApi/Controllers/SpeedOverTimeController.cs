using ATSPM.Application.Business;
using ATSPM.Data.Models.SpeedManagement.SpeedOverTime;
using Microsoft.AspNetCore.Mvc;

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
