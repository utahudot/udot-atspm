using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ATSPM.Domain.Extensions;

namespace ATSPM.Application.Analysis.Common
{
    public interface IDetectorCount
    {
        int DetectorCount { get; }
    }

    public interface IPhaseVolume : IStartEndRange, ILocationPhaseLayer, IDetectorCount
    {
        DirectionTypes Direction { get; set; }
    }

    public interface IToltalVolume : IStartEndRange, ILocationLayer, IDetectorCount
    {

    }

    public class TotalVolume : StartEndRange, IToltalVolume
    {
        public Volume Primary { get; set; }
        public Volume Opposing { get; set; }

        #region IToltalVolume

        #region ILocationLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        #endregion

        #region IDetectorCount

        public int DetectorCount => Primary?.DetectorCount + Opposing?.DetectorCount ?? 0;

        #endregion

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier} --- {Primary} --- {Opposing} --- {Start} - {End} - {DetectorCount}";
        }
    }

    public class TotalVolumes : Timeline<TotalVolume>, IToltalVolume
    {
        public TotalVolumes() { }

        public TotalVolumes(DateTime start, DateTime end, TimeSpan segmentSpan) : base(start, end, segmentSpan) { }

        public TotalVolumes(IEnumerable<ITimestamp> ranges, TimeSpan segmentSpan) : base(ranges, segmentSpan) { }

        public TotalVolumes(IEnumerable<IStartEndRange> ranges, TimeSpan segmentSpan) : base(ranges, segmentSpan) { }

        #region IToltalVolume

        #region ILocationLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        #endregion

        #region IDetectorCount

        public int DetectorCount => this.Segments.Sum(s => s.DetectorCount);

        #endregion

        #endregion
    }

    //public class Volume : List<IDetectorEvent>, ILocationPhaseLayer, IPhaseVolume
    //{
    //    #region IPhaseVolume

    //    #region ILocationPhaseLayer

    //    /// <inheritdoc/>
    //    public string locationIdentifier { get; set; }

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
    //        return $"{locationIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
    //    }
    //}

    public class Volume : StartEndRange, IPhaseVolume
    {
        #region IPhaseVolume

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region IDetectorCount

        /// <inheritdoc/>
        public int DetectorCount => DetectorEvents.Count();

        #endregion

        public DirectionTypes Direction { get; set; }

        #endregion

        #region IStartEndRange

        /// <inheritdoc/>
        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        #endregion

        public List<CorrectedDetectorEvent> DetectorEvents { get; set; } = new();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
        }
    }

    //[JsonObject(memberSerialization: MemberSerialization.OptIn)]
    //public class Volume : StartEndList<CorrectedDetectorEvent>, ILocationPhaseLayer, IPhaseVolume
    //{
    //    #region IPhaseVolume

    //    #region ILocationPhaseLayer

    //    /// <inheritdoc/>
    //    [JsonProperty]
    //    public string locationIdentifier { get; set; }

    //    /// <inheritdoc/>
    //    [JsonProperty]
    //    public int PhaseNumber { get; set; }

    //    #endregion

    //    #region IDetectorCount

    //    [JsonProperty]
    //    public int DetectorCount => this.Count();

    //    #endregion

    //    [JsonProperty]
    //    public DirectionTypes Direction { get; set; }

    //    #endregion

    //    #region IStartEndRange

    //    public virtual bool InRange(DateTime time)
    //    {
    //        return time >= Start && time < End;
    //    }

    //    //public new void AddRange(IEnumerable<CorrectedDetectorEvent> collection)
    //    //{
    //    //    base.AddRange()
    //    //}

    //    #endregion

    //    [JsonProperty]
    //    CorrectedDetectorEvent[] Items
    //    {
    //        get
    //        {
    //            return this.ToArray();
    //        }
    //        set
    //        {
    //            if (value != null)
    //                this.AddRange(value);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return $"{locationIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
    //    }
    //}

    //[JsonObject(memberSerialization: MemberSerialization.OptIn)]
    public class Volumes : Timeline<Volume>, IPhaseVolume
    {
        public Volumes() { }

        public Volumes(DateTime start, DateTime end, TimeSpan segmentSpan) : base(start, end, segmentSpan) { }

        public Volumes(IEnumerable<ITimestamp> ranges, TimeSpan segmentSpan) : base(ranges, segmentSpan) { }

        public Volumes(IEnumerable<IStartEndRange> ranges, TimeSpan segmentSpan) : base(ranges, segmentSpan) { }

        #region IPhaseVolume

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        [JsonProperty]
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        public int PhaseNumber { get; set; }

        #endregion

        #region IStartEndRange

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        #endregion

        #region IDetectorCount

        [JsonProperty]
        public int DetectorCount => this.Segments.Sum(s => s.DetectorCount);

        #endregion

        [JsonProperty]
        public DirectionTypes Direction { get; set; }

        #endregion

        public override string ToString()
        {
            return $"{LocationIdentifier} - {PhaseNumber} - {Direction} - {Start} - {End} - {DetectorCount}";
        }
    }
}
