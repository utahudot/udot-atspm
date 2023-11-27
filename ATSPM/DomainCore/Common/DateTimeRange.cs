using ATSPM.Domain.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace ATSPM.Domain.Common
{
    public interface ITimestamp
    {
        DateTime Timestamp { get; set;}
    }
    
    /// <summary>
    /// A date/time range object
    /// </summary>
    public interface IStartEndRange
    {
        /// <summary>
        /// End of range
        /// </summary>
        DateTime End { get; set; }
        
        /// <summary>
        /// Start of range
        /// </summary>
        DateTime Start { get; set; }

        /// <summary>
        /// Checks to see if <see cref="DateTime"/> is in range of <see cref="Start"/> and <see cref="End"/>
        /// </summary>
        /// <param name="time">Returns true if <see cref="DateTime"/> is in range</param>
        /// <returns></returns>
        bool InRange(DateTime time);

        /// <summary>
        /// Checks to see if <see cref="IStartEndRange"/> is in range of <see cref="Start"/> and <see cref="End"/>
        /// </summary>
        /// <param name="range">Returns true if <see cref="IStartEndRange"/> is in range</param>
        /// <returns></returns>
        bool InRange(IStartEndRange range);
    }

    public static class StartEndRangeExtensions
    {
        public static bool InRange(this IStartEndRange range, ITimestamp timestamp)
        {
            return range.InRange(timestamp.Timestamp);
        }

        
    }

    /// <inheritdoc/>
    public class StartEndRange : IStartEndRange
    {
        /// <inheritdoc/>
        public DateTime Start { get; set; }

        /// <inheritdoc/>
        public DateTime End { get; set; }

        /// <inheritdoc/>
        public virtual bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        /// <inheritdoc/>
        public virtual bool InRange(IStartEndRange range)
        {
            return range.Start >= Start && range.End <= End;
        }
    }

    //public enum TimelineType
    //{
    //    Unknown,
    //    Hours,
    //    Minutes,
    //    Seconds
    //}

    //public class TimelineOptions
    //{
    //    public DateTime End { get; set; }

    //    public DateTime Start { get; set; }

    //    public TimeSpan SementLength { get; set; }

    //    public TimelineType Type { get; set; } = TimelineType.Minutes;

    //    public int Size { get; set; }
    //}

    //public static class Timeline
    //{
        //public static TimeSpan ToTimeSpan(TimelineType tlTYpe, int size) => tlTYpe switch
        //{
        //    TimelineType.Hours => TimeSpan.FromHours(size),
        //    TimelineType.Minutes => TimeSpan.FromMinutes(size),
        //    TimelineType.Seconds => TimeSpan.FromSeconds(size),
        //    TimelineType.Unknown => TimeSpan.Zero,
        //    _ => TimeSpan.Zero,
        //};

        //public static Timeline<T> CreateTimeline<T>(this IEnumerable<IStartEndRange> ranges, TimelineType tlTYpe, int size) where T : IStartEndRange, new()
        //{
        //    var start = ranges.Min(m => m.Start).RoundDown(ToTimeSpan(tlTYpe, size));
        //    var end = ranges.Max(m => m.End).RoundUp(ToTimeSpan(tlTYpe, size));

        //    return CreateTimeline<T>(tlTYpe, start, end, size);
        //}

        //public static Timeline<T> CreateTimeline<T>(this IEnumerable<ITimestamp> Timestamps, TimelineType tlTYpe, int size) where T : IStartEndRange, new()
        //{
        //    var start = Timestamps.Min(m => m.Timestamp).RoundDown(ToTimeSpan(tlTYpe, size));
        //    var end = Timestamps.Max(m => m.Timestamp).RoundUp(ToTimeSpan(tlTYpe, size));

        //    return CreateTimeline<T>(tlTYpe, start, end, size);
        //}

        //public static Timeline<T> CreateTimeline<T>(TimelineType tlTYpe, DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        //{
        //    switch (tlTYpe)
        //    {
        //        case TimelineType.Hours:
        //            return FromHours<T>(start, end, size);
        //        case TimelineType.Minutes:
        //            return FromMinutes<T>(start, end, size);
        //        case TimelineType.Seconds:
        //            return FromSeconds<T>(start, end, size);
        //    }

        //    return null;
        //}

        //public static Timeline<T> FromHours<T>(DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        //{
        //    var values = Enumerable
        //        .Range(0, Convert.ToInt32((end.TimeOfDay.TotalHours - start.TimeOfDay.TotalHours) / size + 1))
        //        .Select((s, i) => start.AddHours(i * size)).ToList();

        //    var result = new Timeline<T>(ReturnDateTimeRangeList<T>(values), TimelineType.Hours, size);

        //    return result;
        //}

        //public static Timeline<T> FromMinutes<T>(DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        //{
        //    var values = Enumerable
        //        .Range(0, Convert.ToInt32((end.TimeOfDay.TotalMinutes - start.TimeOfDay.TotalMinutes) / size + 1))
        //        .Select((s, i) => start.AddMinutes(i * size)).ToList();

        //    var result = new Timeline<T>(ReturnDateTimeRangeList<T>(values), TimelineType.Minutes, size);

        //    return result;
        //}

        //public static Timeline<T> FromSeconds<T>(DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        //{
        //    var values = Enumerable
        //        .Range(0, Convert.ToInt32((end.TimeOfDay.TotalSeconds - start.TimeOfDay.TotalSeconds) / size + 1))
        //        .Select((s, i) => start.AddSeconds(i * size)).ToList();

        //    var result = new Timeline<T>(ReturnDateTimeRangeList<T>(values), TimelineType.Seconds, size);

        //    return result;
        //}

        //private static List<T> ReturnDateTimeRangeList<T>(List<DateTime> values) where T : IStartEndRange, new()
        //{
        //    return new List<T>(values.Take(values.Count() - 1).Select((s, i) => new T() { Start = values[i], End = values[i + 1] }));
        //}
    //}

    public class Timeline<T> : StartEndRange where T : IStartEndRange, new()
    {
        public Timeline() { }

        public Timeline(DateTime start, DateTime end, TimeSpan segmentSpan)
        {
            Start = start.RoundDown(segmentSpan);
            End = end.RoundDown(segmentSpan);

            Segments = CreateTimelineSegements(Start, End, segmentSpan);
        }

        public Timeline(IEnumerable<ITimestamp> ranges, TimeSpan segmentSpan)
        {
            Start = ranges.Min(m => m.Timestamp).RoundDown(segmentSpan);
            End = ranges.Max(m => m.Timestamp).RoundUp(segmentSpan);

            Segments = CreateTimelineSegements(Start, End, segmentSpan);
        }

        public Timeline(IEnumerable<IStartEndRange> ranges, TimeSpan segmentSpan)
        {
            Start = ranges.Min(m => m.Start).RoundDown(segmentSpan);
            End = ranges.Max(m => m.End).RoundUp(segmentSpan);

            Segments = CreateTimelineSegements(Start, End, segmentSpan);
        }

        public TimeSpan SegmentSpan => TimeSpan.FromSeconds(Segments.Select(a => a.End - a.Start).Average(a => a.TotalSeconds));

        public IReadOnlyList<T> Segments { get; internal set; } = new List<T>();

        private IReadOnlyList<T> CreateTimelineSegements(DateTime start, DateTime end, TimeSpan segmentSpan)
        {
            var values = Enumerable
                .Range(0, Convert.ToInt32((end - start).Divide(segmentSpan)) + 1)
                .Select((s, i) => start.Add(segmentSpan.Multiply(i)))
                .ToList();

            var result = values.Take(values.Count() - 1).Select((s, i) => new T() { Start = values[i], End = values[i + 1] }).ToList();

            return result;
        }
    }
}
