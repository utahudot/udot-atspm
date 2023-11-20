using ATSPM.Data.Enums;
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
    public class TotalVolume : VolumeBase
    {
        public Volume Primary { get; set; }
        public Volume Opposing { get; set; }

        public int DetectorCount => Primary?.Count + Opposing?.Count ?? 0;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    //public class Volumes : Timeline<Volume>
    //{
    //    public Volumes(TimelineOptions options) : base(options) { }

    //    public Volumes(Timeline<Volume> collection) : base(collection) { }

    //    public int DetectorCount => this.Sum(s => s.DetectorCount);
    //}



    public class TotalVolumes : Timeline<TotalVolume>
    {
        public TotalVolumes(TimelineOptions options) : base(options) { }

        public TotalVolumes(Timeline<TotalVolume> collection) : base(collection) { }

        public int DetectorCount => this.Sum(s => s.DetectorCount);
    }

    public abstract class VolumeBase : StartEndRange
    {
        public int DetectorCount { get; set; }

        public override bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    //public class Volume : VolumeBase
    //{
    //    public int Phase { get; set; }
    //    public DirectionTypes Direction { get; set; }

    //    public override string ToString()
    //    {
    //        return JsonSerializer.Serialize(this);
    //    }
    //}

    public class Volume : List<IDetectorEvent>, IStartEndRange
    {
        public int Phase { get; set; }
        public DirectionTypes Direction { get; set; }

        public DateTime End { get; set; }
        public DateTime Start { get; set; }

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        public override string ToString()
        {
            return $"{Phase} - {Direction} - {Start} - {End} - {this.Count}";
        }
    }

    public class Volumes : Timeline<Volume>
    {
        public Volumes(TimelineOptions options) : base(options) { }

        public Volumes(Timeline<Volume> collection) : base(collection) { }

        public int DetectorCount => this.Sum(s => s.Count());

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
