using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IDetectorRepository : IAsyncRepository<Detector>
    {
        //SPM GetContext();

        [Obsolete("Use Lookup instead")]
        Detector GetDetectorByDetectorID(string DetectorID);
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<Detector> GetDetectorsBySignalID(string SignalID);
        [Obsolete("Use Lookup instead")]
        Detector GetDetectorByID(int ID);

        //List<MOE.Common.Models.Detectors> GetDetectorsBySignalIDAndPhase(string SignalID, int PhaseNumber);
        IReadOnlyCollection<Detector> GetDetectorsBySignalIDAndMetricType(string SignalID, int MetricID);
        [Obsolete("Use Add in the BaseClass")]
        Detector Add(Detector Detector);
        [Obsolete("Use Update in the BaseClass")]
        void Update(Detector Detector);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(Detector Detector);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(int ID);
        bool CheckReportAvialbility(string detectorID, int metricID);
        bool CheckReportAvialbilityByDetector(Detector gd, int metricID);
        int GetMaximumDetectorChannel(int versionId);
        IReadOnlyCollection<Detector> GetDetectorsByIds(List<int> excludedDetectorIds);
        IReadOnlyCollection<Detector> GetDetectorsBySignalIdMovementTypeIdDirectionTypeId(string signalId, int directionTypeId, List<int> movementTypeIds);
    }
}
