<<<<<<< HEAD
﻿using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
=======
﻿using ATSPM.Application.Repositories;
>>>>>>> main
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.ArrivalOnRed;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Arrival on red report service
    /// </summary>
    public class ArrivalOnRedReportService : ReportServiceBase<ArrivalOnRedOptions, IEnumerable<ArrivalOnRedResult>>
    {
        private readonly ArrivalOnRedService arrivalOnRedService;
        private readonly LocationPhaseService LocationPhaseService;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        /// <inheritdoc/>
        public ArrivalOnRedReportService(
            ArrivalOnRedService arrivalOnRedService,
            LocationPhaseService LocationPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService
            )
        {
            this.arrivalOnRedService = arrivalOnRedService;
            this.LocationPhaseService = LocationPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ArrivalOnRedResult>> ExecuteAsync(ArrivalOnRedOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");

                return await Task.FromException<IEnumerable<ArrivalOnRedResult>>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");

                return await Task.FromException<IEnumerable<ArrivalOnRedResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<ArrivalOnRedResult>>();
            foreach (var phase in phaseDetails)
            {
                if ((phase.IsPermissivePhase && parameter.GetPermissivePhase) || !phase.IsPermissivePhase)
                {
                    tasks.Add(
                   GetChartDataByApproach(parameter, phase, controllerEventLogs, planEvents, Location.LocationDescription()));
                }
                    
            }

            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PhaseNumber).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<ArrivalOnRedResult> GetChartDataByApproach(
            ArrivalOnRedOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string LocationDescription)
        {
            var LocationPhase = await LocationPhaseService.GetLocationPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.BinSize,
                null,
                controllerEventLogs,
                planEvents,
                false
                );
            if (LocationPhase == null)
            {
                return null;
            }
            ArrivalOnRedResult viewModel = arrivalOnRedService.GetChartData(options, LocationPhase, phaseDetail.Approach);
            viewModel.LocationDescription = LocationDescription;
            return viewModel;
        }
    }
}
