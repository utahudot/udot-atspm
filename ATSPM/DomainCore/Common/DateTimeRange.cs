using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ATSPM.Domain.Common
{
    public interface IStartEndRange
    {
        DateTime End { get; set; }
        DateTime Start { get; set; }

        bool InRange(DateTime time);
    }

    //public interface IDateTimeRange
    //{
    //    DateTime StartTime { get; set; }

    //    DateTime EndTime { get; set; }

    //    bool InRange(DateTime time);
    //}

    public class StartEndRange : IStartEndRange
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        public override string ToString()
        {
            return $"Start: {Start} End: {End}";
        }
    }

    public enum TimelineType
    {
        Unknown,
        Hours,
        Minutes,
        Seconds
    }

    public class TimelineOptions
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Size { get; set; }
        public TimelineType Type { get; set; } = TimelineType.Minutes;
    }

    public static class Timeline
    {
        //public Timeline() { }

        //public Timeline(int capacity) : base(capacity) { }

        //public Timeline(IEnumerable<StartEndRange> collection) : base(collection) { }

        public static Timeline<T> FromType<T>(TimelineType tlTYpe, DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        {
            switch (tlTYpe)
            {
                case TimelineType.Hours:
                    return FromHours<T>(start, end, size);
                case TimelineType.Minutes:
                    return FromMinutes<T>(start, end, size);
                case TimelineType.Seconds:
                    return FromSeconds<T>(start, end, size);
            }

            return null;
        }

        public static Timeline<T> FromHours<T>(DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        {
            var values = Enumerable
                .Range(0, Convert.ToInt32((end.TimeOfDay.TotalHours - start.TimeOfDay.TotalHours) / size + 1))
                .Select((s, i) => start.AddHours(i * size)).ToList();

            var result = new Timeline<T>(ReturnDateTimeRangeList<T>(values), TimelineType.Hours, size);

            //result.Start = start;
            //result.End = end;
            //result.Size = size;
            //result.Type = TimelineType.Hours;

            return result;
        }

        public static Timeline<T> FromMinutes<T>(DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        {
            var values = Enumerable
                .Range(0, Convert.ToInt32((end.TimeOfDay.TotalMinutes - start.TimeOfDay.TotalMinutes) / size + 1))
                .Select((s, i) => start.AddMinutes(i * size)).ToList();

            var result = new Timeline<T>(ReturnDateTimeRangeList<T>(values), TimelineType.Minutes, size);

            //result.Start = start;
            //result.End = end;
            //result.Size = size;
            //result.Type = TimelineType.Minutes;

            return result;
        }

        public static Timeline<T> FromSeconds<T>(DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        {
            var values = Enumerable
                .Range(0, Convert.ToInt32((end.TimeOfDay.TotalSeconds - start.TimeOfDay.TotalSeconds) / size + 1))
                .Select((s, i) => start.AddSeconds(i * size)).ToList();

            var result = new Timeline<T>(ReturnDateTimeRangeList<T>(values), TimelineType.Seconds, size);

            //result.Start = start;
            //result.End = end;
            //result.Size = size;
            //result.Type = TimelineType.Seconds;

            return result;
        }

        internal static List<T> ReturnDateTimeRangeList<T>(List<DateTime> values) where T : IStartEndRange, new()
        {
            return new List<T>(values.Take(values.Count() - 1).Select((s, i) => new T() { Start = values[i], End = values[i + 1] }));
        }
    }

    public class Timeline<T> : List<T>, IStartEndRange where T : IStartEndRange, new()
    {
        public Timeline(TimelineOptions options) : this(Timeline.FromType<T>(options.Type, options.Start, options.End, options.Size)) {}

        //public Timeline(int capacity) : base(capacity) { }

        public Timeline(Timeline<T> collection) : base(collection) 
        {
            Start = collection.Start;
            End = collection.End;
            Size = collection.Size;
            Type = collection.Type;
        }

        public Timeline(IEnumerable<T> collection, TimelineType tlTYpe, int size) : base(collection)
        {
            Start = this.Min(m => m.Start);
            End = this.Max(m => m.End);
            Size = size;
            Type = tlTYpe;
        }

        public int Size { get; internal set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public TimelineType Type { get; internal set; }

        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }
    }
}
