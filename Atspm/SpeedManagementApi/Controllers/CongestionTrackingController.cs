using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.CongestionTracking;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CongestionTrackingController : SpeedBaseController<CongestionTrackingOptions, CongestionTrackingDto>
    {
        public CongestionTrackingController(IReportService<CongestionTrackingOptions, CongestionTrackingDto> reportService) : base(reportService) { }
    }
}
