using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.EffectivenessOfStrategies;
using Utah.Udot.Atspm.Services;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EffectivenessOfStrategiesController : SpeedBaseController<EffectivenessOfStrategiesOptions, List<EffectivenessOfStrategiesDto>>
    {
        public EffectivenessOfStrategiesController(IReportService<EffectivenessOfStrategiesOptions, List<EffectivenessOfStrategiesDto>> reportService) : base(reportService)
        {
        }
    }
}