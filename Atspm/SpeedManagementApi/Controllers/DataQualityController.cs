using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Business.SpeedManagement.DataQuality;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DataQualityController : SpeedBaseController<DataQualityOptions, List<DataQualitySource>>
    {
        public DataQualityController(IReportService<DataQualityOptions, List<DataQualitySource>> reportService) : base(reportService)
        {
        }
    }
}