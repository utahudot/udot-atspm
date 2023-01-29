using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using Microsoft.Extensions.Logging;

namespace Legacy.Common.Business
{
    public class RedLightMonitorPhaseCollectionData
    {
        public double Violations
        {
            get { return SignalPhaseList.Sum(d => d.Violations); }
        }
        public List<RLMSignalPhaseData> SignalPhaseList { get; set; } = new List<RLMSignalPhaseData>();
        public double SevereRedLightViolationsSeconds { get; set; }
        public double Srlv
        {
            get { return SignalPhaseList.Sum(d => d.SevereRedLightViolations); }
        }
    }
    public class RedLightMonitorSignalPhaseCollectionService
    {
        private readonly ISignalRepository signalRepository;
        private readonly RedLLightMonitorService redLLightMonitorService;
        private readonly ILogger logger;

        public RedLightMonitorSignalPhaseCollectionService(ISignalRepository signalRepository, RedLLightMonitorService redLLightMonitorService, ILogger logger)
        {
            this.signalRepository = signalRepository;
            this.redLLightMonitorService = redLLightMonitorService;
            this.logger = logger;
        }

        public RedLightMonitorPhaseCollectionData GetRedLightMonitorSignalPhaseCollectionData(DateTime startDate, DateTime endDate, string signalID,
         int binSize, double srlvSeconds)
        {
            try
            {
                var signalPhaseCollection = new RedLightMonitorPhaseCollectionData();
                var metricTypeID = 11;
                var signal = signalRepository.GetVersionOfSignalByDate(signalID, startDate);
                signalPhaseCollection.SevereRedLightViolationsSeconds = srlvSeconds;
                var approachesForMetric = signal.GetApproachesForSignalThatSupportMetric(metricTypeID);
                //If there are phases in the database add the charts
                if (approachesForMetric.Any())
                    foreach (var approach in approachesForMetric)
                    {
                        signalPhaseCollection.SignalPhaseList.Add(redLLightMonitorService.GetRedLLightMonitorSignalPhaseData(
                            startDate, endDate, binSize, signalPhaseCollection.SevereRedLightViolationsSeconds, approach, false));

                        if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber > 0)
                            signalPhaseCollection.SignalPhaseList.Add(redLLightMonitorService.GetRedLLightMonitorSignalPhaseData(
                                startDate, endDate, binSize, signalPhaseCollection.SevereRedLightViolationsSeconds, approach, true));
                    }
                return signalPhaseCollection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw new Exception("An error occured while getting red light monitor signal phase collection data", ex);
            }
        }

    }
}