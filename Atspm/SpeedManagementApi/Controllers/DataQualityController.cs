using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.DataQuality;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataQualityController : SpeedBaseController<DataQualityOptions, List<DataQualityDto>>
    {
        public DataQualityController(IReportService<DataQualityOptions, List<DataQualityDto>> reportService) : base(reportService)
        {
        }
    }
}