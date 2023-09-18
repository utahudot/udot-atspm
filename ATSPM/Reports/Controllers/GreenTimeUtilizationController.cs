//using ATSPM.Application.Extensions;
//using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
//using ATSPM.Application.Repositories;
//using ATSPM.Data.Models;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class GreenTimeUtilizationController : ControllerBase
//    {
//        private readonly GreenTimeUtilizationService greenTimeUtilizationService;
//        private readonly IControllerEventLogRepository controllerEventLogRepository;
//        private readonly ISignalRepository signalRepository;

//        public GreenTimeUtilizationController(
//            GreenTimeUtilizationService GreenTimeUtilizationService,
//            IControllerEventLogRepository controllerEventLogRepository,
//            ISignalRepository signalRepository)
//        {
//            this.greenTimeUtilizationService = GreenTimeUtilizationService;
//            this.controllerEventLogRepository = controllerEventLogRepository;
//            this.signalRepository = signalRepository;
//        }

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public GreenTimeUtilizationResult Test()
//        {
//            Fixture fixture = new();
//            GreenTimeUtilizationResult viewModel = fixture.Create<GreenTimeUtilizationResult>();
//            return viewModel;
//        }

//        [HttpPost("getChartData")]
//        public async Task<IEnumerable<GreenTimeUtilizationResult>> GetChartData([FromBody] GreenTimeUtilizationOptions options)
//        {
//            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
//            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
//            var planEvents = controllerEventLogs.GetPlanEvents(
//                options.Start.AddHours(-12),
//                options.End.AddHours(12)).ToList();
//            var tasks = new List<Task<GreenTimeUtilizationResult>>();
//            foreach (var approach in signal.Approaches)
//            {
//                tasks.Add(GetChartDataForApproach(options, approach, controllerEventLogs, planEvents));
//            }

//            var results = await Task.WhenAll(tasks);

//            return results.Where(result => result != null);
//        }

//        private async Task<GreenTimeUtilizationResult> GetChartDataForApproach(
//            GreenTimeUtilizationOptions options,
//            Approach approach,
//            IReadOnlyList<ControllerEventLog> controllerEventLogs,
//            IReadOnlyList<ControllerEventLog> planEvents,
//            bool usePermissivePhase)
//        {

//            GreenTimeUtilizationResult viewModel = greenTimeUtilizationService.GetChartData(
//                approach,
//                options,
//                usePermissivePhase,


//                );
//            viewModel.SignalDescription = approach.Signal.SignalDescription();
//            viewModel.ApproachDescription = approach.Description;
//            return viewModel;
//        }
//    }
//}
