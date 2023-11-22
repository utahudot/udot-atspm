using ATSPM.Domain.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

    public abstract class StartEndList<T> : StartEndRange, IList<T> where T : ITimestamp, new()
    {
        private readonly List<T> _list = new List<T>();
        
        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICollection<T>

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return _list.ToArray().IsReadOnly; }
        }

        #endregion

        #region Implementation of IList<T>

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        #endregion
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
        public DateTime End { get; set; }

        public DateTime Start { get; set; }

        public TimeSpan SementLength { get; set; }

        public TimelineType Type { get; set; } = TimelineType.Minutes;

        public int Size { get; set; }
    }

    public static class Timeline
    {
        public static TimeSpan ToTimeSpan(TimelineType tlTYpe, int size) => tlTYpe switch
        {
            TimelineType.Hours => TimeSpan.FromHours(size),
            TimelineType.Minutes => TimeSpan.FromMinutes(size),
            TimelineType.Seconds => TimeSpan.FromSeconds(size),
            TimelineType.Unknown => TimeSpan.Zero,
            _ => TimeSpan.Zero,
        };

        public static Timeline<T> CreateTimeline<T>(this IEnumerable<IStartEndRange> ranges, TimelineType tlTYpe, int size) where T : IStartEndRange, new()
        {
            var start = ranges.Min(m => m.Start).RoundDown(ToTimeSpan(tlTYpe, size));
            var end = ranges.Max(m => m.End).RoundUp(ToTimeSpan(tlTYpe, size));

            return CreateTimeline<T>(tlTYpe, start, end, size);
        }

        public static Timeline<T> CreateTimeline<T>(this IEnumerable<ITimestamp> Timestamps, TimelineType tlTYpe, int size) where T : IStartEndRange, new()
        {
            var start = Timestamps.Min(m => m.Timestamp).RoundDown(ToTimeSpan(tlTYpe, size));
            var end = Timestamps.Max(m => m.Timestamp).RoundUp(ToTimeSpan(tlTYpe, size));

            return CreateTimeline<T>(tlTYpe, start, end, size);
        }

        public static Timeline<T> CreateTimeline<T>(TimelineType tlTYpe, DateTime start, DateTime end, int size) where T : IStartEndRange, new()
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

            return result;
        }

        public static Timeline<T> FromMinutes<T>(DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        {
            var values = Enumerable
                .Range(0, Convert.ToInt32((end.TimeOfDay.TotalMinutes - start.TimeOfDay.TotalMinutes) / size + 1))
                .Select((s, i) => start.AddMinutes(i * size)).ToList();

            var result = new Timeline<T>(ReturnDateTimeRangeList<T>(values), TimelineType.Minutes, size);

            return result;
        }

        public static Timeline<T> FromSeconds<T>(DateTime start, DateTime end, int size) where T : IStartEndRange, new()
        {
            var values = Enumerable
                .Range(0, Convert.ToInt32((end.TimeOfDay.TotalSeconds - start.TimeOfDay.TotalSeconds) / size + 1))
                .Select((s, i) => start.AddSeconds(i * size)).ToList();

            var result = new Timeline<T>(ReturnDateTimeRangeList<T>(values), TimelineType.Seconds, size);

            return result;
        }

        private static List<T> ReturnDateTimeRangeList<T>(List<DateTime> values) where T : IStartEndRange, new()
        {
            return new List<T>(values.Take(values.Count() - 1).Select((s, i) => new T() { Start = values[i], End = values[i + 1] }));
        }
    }

    //public class Timeline<T> : List<T>, IStartEndRange where T : IStartEndRange, new()
    //{
    //    public Timeline() { }

    //    public Timeline(TimelineOptions options) : this(Timeline.FromType<T>(options.Type, options.Start, options.End, options.Size)) { }

    //    //public Timeline(int capacity) : base(capacity) { }

    //    public Timeline(Timeline<T> collection) : base(collection) 
    //    {
    //        Start = collection.Start;
    //        End = collection.End;
    //        Size = collection.Size;
    //        Type = collection.Type;
    //    }

    //    public Timeline(IEnumerable<T> collection, TimelineType tlTYpe, int size) : base(collection)
    //    {
    //        Start = collection.ToList().Min(m => m.Start);
    //        End = collection.ToList().Max(m => m.End);
    //        Size = size;
    //        Type = tlTYpe;
    //    }

    //    public int Size { get; internal set; }
    //    public DateTime End { get; set; }
    //    public DateTime Start { get; set; }
    //    public TimelineType Type { get; internal set; }

    //    ///// <inheritdoc/>
    //    //public virtual bool InRange(DateTime time)
    //    //{
    //    //    return time >= Start && time <= End;
    //    //}

    //    ///// <inheritdoc/>
    //    //public bool InRange(IStartEndRange range)
    //    //{
    //    //    return range.Start >= Start && range.End < End;
    //    //}
    //}

    public class Timeline<T> : StartEndRange where T : IStartEndRange, new()
    {
        public Timeline() { }

        public Timeline(TimelineOptions options)
        {
            End = options.End;
            Start = options.Start;
            SementLength = options.SementLength;
            Type = options.Type;
        }

        public Timeline(Timeline<T> timeline)
        {
            End = timeline.End;
            Start = timeline.Start;
            SementLength = timeline.SementLength;
            Type = timeline.Type;
            Segments = timeline.Segments;
        }

        public Timeline(IEnumerable<T> stuff, TimelineType type, int size)
        {
            
        }

        public TimeSpan SementLength { get; internal set; }

        public TimelineType Type { get; internal set; }

        public IReadOnlyList<T> Segments { get; internal set; } = new List<T>();
    }
}
