using ATSPM.Application.Business;
using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CongestionTrackingController : SpeedBaseController<CongestionTrackingOptions, CongestionTrackingDto>
    {
        public CongestionTrackingController(IReportService<CongestionTrackingOptions, CongestionTrackingDto> reportService): base(reportService) {}
    }
}
