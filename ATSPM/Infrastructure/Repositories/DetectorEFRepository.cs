using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class DetectorEFRepository : ATSPMRepositoryEFBase<Detector>, IDetectorRepository
    {
        public DetectorEFRepository(DbContext db, ILogger<DetectorEFRepository> log) : base(db, log)
        {

        }

        public bool CheckReportAvialbility(string detectorID, int metricID)
        {
            throw new NotImplementedException();
        }

        public bool CheckReportAvialbilityByDetector(Detector gd, int metricID)
        {
            throw new NotImplementedException();
        }

        public Detector GetDetectorByDetectorID(string DetectorID)
        {
            throw new NotImplementedException();
        }

        public Detector GetDetectorByID(int ID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Detector> GetDetectorsByIds(List<int> excludedDetectorIds)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Detector> GetDetectorsBySignalID(string SignalID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Detector> GetDetectorsBySignalIDAndMetricType(string SignalID, int MetricID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Detector> GetDetectorsBySignalIdMovementTypeIdDirectionTypeId(string signalId, int directionTypeId, List<int> movementTypeIds)
        {
            throw new NotImplementedException();
        }

        public int GetMaximumDetectorChannel(int versionId)
        {
            throw new NotImplementedException();
        }

        public void Remove(int ID)
        {
            throw new NotImplementedException();
        }

        Detector IDetectorRepository.Add(Detector Detector)
        {
            throw new NotImplementedException();
        }
    }
}
