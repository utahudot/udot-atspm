using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.Common
{
    public interface IDetectorCount
    {
        int DetectorCount { get; }
    }
    
    public interface IPhaseVolume : IStartEndRange, ISignalPhaseLayer, IDetectorCount
    {
        DirectionTypes Direction { get; set; }
    }

    public interface IToltalVolume : IStartEndRange, ISignalLayer, IDetectorCount
    { 
        
    }
    
    public class TotalVolume : StartEndRange, IToltalVolume
    {
        public Volume Primary { get; set; }
        public Volume Opposing { get; set; }

        #region IToltalVolume

        #region ISignalLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        #endregion

        #region IStartEndRange

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        #endregion

        #region IDetectorCount

        public int DetectorCount => Primary?.Count + Opposing?.Count ?? 0;

        #endregion

        #endregion
    }

    public class TotalVolumes : Timeline<TotalVolume>, IToltalVolume
    {
        public TotalVolumes(TimelineOptions options) : base(options) { }

        public TotalVolumes(Timeline<TotalVolume> collection) : base(collection) { }

        #region IToltalVolume

        #region ISignalLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        #endregion

        #region IStartEndRange

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        #endregion

        #region IDetectorCount

        public int DetectorCount => this.Sum(s => s.DetectorCount);

        #endregion

        #endregion
    }

    //public class Volume : List<IDetectorEvent>, ISignalPhaseLayer, IPhaseVolume
    //{
    //    #region IPhaseVolume

    //    #region ISignalPhaseLayer

    //    /// <inheritdoc/>
    //    public string SignalIdentifier { get; set; }

    //    /// <inheritdoc/>
    //    public int PhaseNumber { get; set; }

    //    #endregion

    //    #region IStartEndRange

    //    public DateTime End { get; set; }

    //    public DateTime Start { get; set; }

    //    public virtual bool InRange(DateTime time)
    //    {
    //        return time >= Start && time < End;
    //    }



    //    #endregion

    //    #region IDetectorCount

    //    public int DetectorCount => this.Count();

    //    #endregion

    //    public DirectionTypes Direction { get; set; }

    //    #endregion

    //    public override string ToString()
    //    {
    //        return $"{SignalIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
    //    }
    //}

    public class Volume : StartEndList<CorrectedDetectorEvent>, ISignalPhaseLayer, IPhaseVolume
    {
        #region IPhaseVolume

        #region ISignalPhaseLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region IDetectorCount

        public int DetectorCount => this.Count();

        #endregion

        public DirectionTypes Direction { get; set; }

        #endregion

        #region IStartEndRange

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        //public new void AddRange(IEnumerable<CorrectedDetectorEvent> collection)
        //{
        //    base.AddRange()
        //}

        #endregion

        public override string ToString()
        {
            return $"{SignalIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
        }
    }

    public class Volumes : Timeline<Volume>, IPhaseVolume
    {
        public Volumes(TimelineOptions options) : base(options) { }

        public Volumes(Timeline<Volume> collection) : base(collection) { }

        #region IPhaseVolume

        #region ISignalPhaseLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region IStartEndRange

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        #endregion

        #region IDetectorCount

        public int DetectorCount => this.Sum(s => s.DetectorCount);

        #endregion

        public DirectionTypes Direction { get; set; }

        #endregion

        public override string ToString()
        {
            return $"{SignalIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
        }
    }
}
