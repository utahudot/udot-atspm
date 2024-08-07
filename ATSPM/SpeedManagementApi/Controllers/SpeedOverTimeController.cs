using ATSPM.Application.Business;
using ATSPM.Data.Models.SpeedManagement.SpeedOverTime;

namespace SpeedManagementApi.Controllers
{
    public class SpeedOverTimeController : SpeedBaseController<SpeedOverTimeOptions, SpeedOverTimeDto>
    {
        public SpeedOverTimeController(IReportService<SpeedOverTimeOptions, SpeedOverTimeDto> reportService) : base(reportService)
        {
        }
    }
}
