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

            var gd = _db.Set<Detector>().GroupBy(g => g.DetectorId).Select(r => r.OrderByDescending(g => g.DateAdded).FirstOrDefault()).ToList();
            var result = false;
            if (gd != null)
                foreach (var firstGd in gd)
                    foreach (var dt in firstGd.DetectionTypes)
                        foreach (var m in dt.MetricTypes)
                            if (m.MetricId == metricID)
                                return true;
            return result;
        }

        public bool CheckReportAvialbilityByDetector(Detector gd, int metricID)
        {
            var result = false;
            if (gd != null)
                if (gd.DetectionTypes != null)
                {
                    foreach (var dt in gd.DetectionTypes)
                    {
                        foreach (var m in dt.MetricTypes)
                            if (m.MetricId == metricID)
                                return true;
                    }
                }
            return result;
        }

        public Detector GetDetectorByDetectorID(string DetectorID)
        {
            return _db.Set<Detector>().Where(d => d.DetectorId == DetectorID).OrderByDescending(x => x.DateAdded).FirstOrDefault();
        }

        public Detector GetDetectorByID(int ID)
        {
            return _db.Set<Detector>().Find(ID);
        }

        public IReadOnlyCollection<Detector> GetDetectorsByIds(List<int> excludedDetectorIds)
        {
            return _db.Set<Detector>().Where(d => excludedDetectorIds.Contains(d.Id)).ToList();
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
            return _db.Set<Approach>().Where(a => a.DirectionTypeId == directionTypeId)
                                    .SelectMany(a => a.Detectors)
                                    .Where(d => movementTypeIds.Contains(d.MovementTypeId ?? -1))
                                    .ToList();
        }

        public int GetMaximumDetectorChannel(int versionId)
        {
            throw new NotImplementedException();
        }

        public void Remove(int ID)
        {
            throw new NotImplementedException();
        }

        Detector IDetectorRepository .Add(Detector Detector)
        {
            throw new NotImplementedException();
        }
    }
}
