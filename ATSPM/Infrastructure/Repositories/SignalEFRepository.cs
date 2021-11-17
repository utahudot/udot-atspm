using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ATSPM.Infrasturcture.Repositories
{
    public class SignalEFRepository : ATSPMRepositoryEFBase<Signal>, ISignalRepository
    {
        public SignalEFRepository(DbContext db, ILogger<SignalEFRepository> log) : base(db, log) { }

        #region ISignalRepository

        [Obsolete("This method isn't currently being used")]
        public void AddList(List<Signal> signals)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use the add in respository base class")]
        public void AddOrUpdate(Signal signal)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public int CheckVersionWithFirstDate(string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use ICloneable")]
        public Signal CopySignalToNewVersion(Signal originalVersion)
        {
            var newVersion = (Signal)originalVersion.Clone();

            newVersion.VersionAction = _db.Set<VersionAction>().Find(4);

            //newVersion.VersionAction = (from r in _db.VersionActions
            //                            where r.ID == 4
            //                            select r).FirstOrDefault();

            //TODO: use clone instead
            //newVersion.SignalID = originalVersion.SignalID;
            //newVersion.Start = DateTime.Today;
            //newVersion.Note = "Copy of " + originalVersion.Note;
            //newVersion.PrimaryName = originalVersion.PrimaryName;
            //newVersion.SecondaryName = originalVersion.SecondaryName;
            //newVersion.IPAddress = originalVersion.IPAddress;
            //newVersion.ControllerTypeID = originalVersion.ControllerTypeID;
            //newVersion.RegionID = originalVersion.RegionID;
            //newVersion.Enabled = originalVersion.Enabled;
            //newVersion.Latitude = originalVersion.Latitude;
            //newVersion.Longitude = originalVersion.Longitude;


            this.Add(newVersion);

            //_db.Signals.Add(newVersion);
            //_db.SaveChanges();

            //CopyApproaches(originalVersion, newVersion);

            return newVersion;
        }

        [Obsolete("Redundant to GetAllSignals")]
        public IReadOnlyList<Signal> EagerLoadAllSignals()
        {
            throw new NotImplementedException();
        }

        public bool Exists(string signalId)
        {
            //return _db.DatabaseArchiveExcludedSignals.Any(s => s.SignalId == signalId);
            throw new NotImplementedException();
        }

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        public IReadOnlyList<Signal> GetAllEnabledSignals()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        public IList<Signal> GetAllSignals()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use overload of GetLatestVersionOfAllSignals")]
        public IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalID(string signalID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use overload of GetLatestVersionOfAllSignals")]
        public IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(int controllerTypeId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Signal> GetLatestVersionOfAllSignals()
        {
            var result = table 
                .Where(v => v.VersionActionId != 3)
                .Include(i => i.ControllerType)
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(r => r.SignalId)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return result;

            //var activeSignals = _db.Set<Signal>().Where(r => r.VersionActionId != 3)
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.DetectionTypes)))
            //    .Include(signal =>
            //        signal.Approaches.Select(
            //            a => a.Detectors.Select(d => d.DetectionTypes.Select(dt => dt.MetricTypes))))
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.DetectionHardware)))
            //    .Include(signal => signal.Approaches.Select(a => a.DirectionType))
            //    .GroupBy(r => r.SignalID)
            //    .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault()).ToList();
            //return activeSignals;

            //throw new NotImplementedException();
        }

        //public IReadOnlyList<Signal> GetLatestVersionOfAllSignalsForFtp()
        //{
        //    throw new NotImplementedException();
        //}

        public Signal GetLatestVersionOfSignalBySignalID(string signalID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should not be in respository")]
        public IReadOnlyList<Pin> GetPinInfo()
        {
            //var pins = new List<Pin>();
            ////foreach (var signal in GetLatestVersionOfAllSignals().Where(s => s.Enabled //&& s.SignalID == "7063"
            ////).ToList())
            //List<Signal> signals = GetLatestVersionOfAllSignals().Where(s => s.Enabled).ToList();
            //foreach (var signal in signals)
            //{
            //    var pin = new Pin(signal.SignalId, signal.Latitude,
            //        signal.Longitude,
            //        signal.PrimaryName + " " + signal.SecondaryName, signal.RegionId.ToString());
            //    pin.MetricTypes = signal.GetMetricTypesString();
            //    pins.Add(pin);
            //    //Console.WriteLine(pin.SignalID);
            //}
            //return pins;

            throw new NotImplementedException();
        }

        [Obsolete("Just get whole object")]
        public string GetSignalDescription(string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should not be in respository")]
        public string GetSignalLocation(string signalID)
        {
            var signal = GetLatestVersionOfSignalBySignalID(signalID);
            var location = string.Empty;
            if (signal != null)
                location = signal.PrimaryName + " @ " + signal.SecondaryName;

            return location;
        }

        public IReadOnlyList<Signal> GetSignalsBetweenDates(string signalId, DateTime startDate, DateTime endDate)
        {
            //var signals = new List<Signal>();
            //var signalBeforeStart = _db.Signals
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.MovementType)))
            //    .Include(signal => signal.Approaches.Select(a => a.DirectionType))
            //    .Where(signal => signal.SignalID == signalId
            //                     && signal.Start <= startDate
            //                     && signal.VersionActionId != 3).OrderByDescending(s => s.Start)
            //    .Take(1)
            //    .FirstOrDefault();
            //if (signalBeforeStart != null)
            //    signals.Add(signalBeforeStart);
            //if (_db.Signals.Any(signal => signal.SignalID == signalId
            //                              && signal.Start > startDate
            //                              && signal.Start < endDate
            //                              && signal.VersionActionId != 3))
            //    signals.AddRange(_db.Signals
            //        .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.MovementType)))
            //        .Include(signal => signal.Approaches.Select(a => a.DirectionType))
            //        .Where(signal => signal.SignalID == signalId
            //                         && signal.Start > startDate
            //                         && signal.Start < endDate
            //                         && signal.VersionActionId != 3).ToList());
            //return signals;

            throw new NotImplementedException();
        }

        public Signal GetSignalVersionByVersionId(int versionId)
        {
            //var version = _db.Signals
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.MovementType)))
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.DetectionTypes)))
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.DetectionHardware)))
            //    .Include(signal => signal.Approaches.Select(a => a.DirectionType))
            //    .FirstOrDefault(signal => signal.VersionID == versionId);
            //if (version != null)
            //{
            //    AddSignalAndDetectorLists(version);
            //}
            //return version;

            throw new NotImplementedException();
        }

        public Signal GetVersionOfSignalByDate(string signalId, DateTime startDate)
        {
            //var signals = _db.Signals
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.MovementType)))
            //    .Include(signal => signal.Approaches.Select(a => a.DirectionType))
            //    .Where(signal => signal.SignalID == signalId)
            //    .Where(signal => signal.Start <= startDate)
            //    .Where(signal => signal.VersionActionId != 3)
            //    .ToList();

            //if (signals.Count > 1)
            //{
            //    var orderedSignals = signals.OrderByDescending(signal => signal.Start);
            //    return orderedSignals.First();
            //}
            //else
            //{
            //    return signals.FirstOrDefault();
            //}

            throw new NotImplementedException();
        }

        public Signal GetVersionOfSignalByDateWithDetectionTypes(string signalId, DateTime startDate)
        {
            //var signals = _db.Signals
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.MovementType)))
            //    .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.DetectionTypes)))
            //    .Include(signal => signal.Approaches.Select(a => a.DirectionType))
            //    .Where(signal => signal.SignalID == signalId)
            //    .Where(signal => signal.Start <= startDate)
            //    .Where(signal => signal.VersionActionId != 3)
            //    .ToList();

            //if (signals.Count > 1)
            //{
            //    var orderedSignals = signals.OrderByDescending(signal => signal.Start);
            //    return orderedSignals.First();
            //}
            //else
            //{
            //    return signals.FirstOrDefault();
            //}

            throw new NotImplementedException();
        }

        public void SetAllVersionsOfASignalToDeleted(string id)
        {
            //var signals = from r in _db.Signals
            //              where r.SignalID == signalId
            //              select r;

            //foreach (var s in signals)
            //    s.VersionActionId = 3;

            //_db.SaveChanges();

            throw new NotImplementedException();
        }

        public void SetVersionToDeleted(int versionId)
        {
            //var signal = (from r in _db.Signals where r.VersionID == versionId select r).FirstOrDefault();
            //if (signal != null)
            //    signal.VersionActionId = 3;
            //_db.SaveChanges();

            throw new NotImplementedException();
        }

        #endregion
    }
}
