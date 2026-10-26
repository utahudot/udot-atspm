#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Bins/BinFactory.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace Utah.Udot.Atspm.Business.Bins
{
    public static class BinFactory
    {
        public static List<BinsContainer> GetBins(TimeOptions timeOptions)
        {
            switch (timeOptions.SelectedBinSize)
            {
                case TimeOptions.BinSize.FifteenMinute:
                    return GetBinsForRange(timeOptions, 15);
                case TimeOptions.BinSize.ThirtyMinute:
                    return GetBinsForRange(timeOptions, 30);
                case TimeOptions.BinSize.Hour:
                    return GetBinsForRange(timeOptions, 60);
                case TimeOptions.BinSize.Day:
                    return GetDayBinsContainersForRange(timeOptions);
                case TimeOptions.BinSize.Month:
                    return GetMonthBinsForRange(timeOptions);
                case TimeOptions.BinSize.Year:
                    return GetYearBinsForRange(timeOptions);
                default:
                    return GetBinsForRange(timeOptions, 15);
            }
        }

        private static List<Bin> GetDayBinsForRange(DateTime startDate, DateTime endDate, int startHour,
            int startMinute, int endHour, int endMinute, List<DayOfWeek> daysOfWeek)
        {
            var bins = new List<Bin>();
            for (var startTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
                startTime < endDate;
                startTime = startTime.AddDays(1))
                if (daysOfWeek == null || daysOfWeek.Contains(startTime.DayOfWeek))
                    AddWindowInRange(bins, startTime.AddHours(startHour).AddMinutes(startMinute),
                        startTime.Add(GetTimeOfDayEnd(endHour, endMinute)), startDate, endDate);
            return bins;
        }

        // Adds the part of a day's time window that falls between start and end, if any, so the
        // first and last days do not count time outside the range.
        private static void AddWindowInRange(List<Bin> bins, DateTime windowStart, DateTime windowEnd,
            DateTime start, DateTime end)
        {
            var bin = new Bin
            {
                Start = windowStart > start ? windowStart : start,
                End = windowEnd < end ? windowEnd : end
            };
            if (bin.Start < bin.End)
                bins.Add(bin);
        }

        // The days of a month or year that fall between Start and End, so the first and last
        // containers do not count days outside the range.
        private static List<Bin> GetDayBinsInRange(DateTime containerStart, DateTime containerEnd,
            TimeOptions timeOptions) =>
            GetDayBinsForRange(
                containerStart > timeOptions.Start ? containerStart : timeOptions.Start,
                containerEnd < timeOptions.End ? containerEnd : timeOptions.End,
                timeOptions.TimeOfDayStartHour.Value, timeOptions.TimeOfDayStartMinute.Value,
                timeOptions.TimeOfDayEndHour.Value, timeOptions.TimeOfDayEndMinute.Value,
                timeOptions.DaysOfWeek);

        private static List<BinsContainer> GetDayBinsContainersForRange(TimeOptions timeOptions)
        {
            var binsContainers = new List<BinsContainer>();
            if (timeOptions.DaysOfWeek == null)
                timeOptions.DaysOfWeek = new List<DayOfWeek>
                {
                    DayOfWeek.Sunday,
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday,
                    DayOfWeek.Saturday
                };
            var binsContainer = new BinsContainer(timeOptions.Start, timeOptions.End);
            // Each day that starts before the end is included, as in GetDaysToProcess.
            for (var startTime = new DateTime(timeOptions.Start.Year, timeOptions.Start.Month, timeOptions.Start.Day, 0,
                    0, 0);
                startTime < timeOptions.End;
                startTime = startTime.AddDays(1))
                if (timeOptions.TimeOption == TimeOptions.TimePeriodOptions.StartToEnd && timeOptions.DaysOfWeek.Contains(startTime.DayOfWeek))
                {
                    binsContainer.Bins.Add(new Bin { Start = startTime, End = startTime.AddDays(1) });
                }
                else
                {
                    if (timeOptions.TimeOfDayStartHour != null && timeOptions.TimeOfDayStartMinute != null &&
                        timeOptions.TimeOfDayEndHour != null && timeOptions.TimeOfDayEndMinute != null &&
                        timeOptions.DaysOfWeek.Contains(startTime.DayOfWeek))
                    {
                        AddWindowInRange(binsContainer.Bins,
                            startTime.AddHours(timeOptions.TimeOfDayStartHour.Value)
                                .AddMinutes(timeOptions.TimeOfDayStartMinute.Value),
                            startTime.Add(GetTimeOfDayEnd(timeOptions.TimeOfDayEndHour.Value,
                                timeOptions.TimeOfDayEndMinute.Value)),
                            timeOptions.Start, timeOptions.End);
                    }
                }
            binsContainers.Add(binsContainer);
            return binsContainers;
        }

        private static List<BinsContainer> GetYearBinsForRange(TimeOptions timeOptions)
        {
            var binsContainers = new List<BinsContainer>();
            if (timeOptions.TimeOption == TimeOptions.TimePeriodOptions.StartToEnd)
            {
                var binsContainer = new BinsContainer(timeOptions.Start, timeOptions.End);

                // Each year that starts before the end is included, as for months.
                for (var startTime = new DateTime(timeOptions.Start.Year, 1, 1);
                    startTime < timeOptions.End;
                    startTime = startTime.AddYears(1))
                    binsContainer.Bins.Add(new Bin { Start = startTime, End = startTime.AddYears(1) });
                binsContainers.Add(binsContainer);
            }
            else
            {
                for (var startTime = new DateTime(timeOptions.Start.Year, 1, 1);
                    startTime < timeOptions.End;
                    startTime = startTime.AddYears(1))
                    binsContainers.Add(new BinsContainer(startTime, startTime.AddYears(1))
                    {
                        Bins = GetDayBinsInRange(startTime, startTime.AddYears(1), timeOptions)
                    });
            }
            return binsContainers;
        }

        public static List<BinsContainer> GetMonthBinsForRange(TimeOptions timeOptions)
        {
            var binsContainers = new List<BinsContainer>();
            if (timeOptions.TimeOption == TimeOptions.TimePeriodOptions.StartToEnd)
            {
                var binsContainer = new BinsContainer(timeOptions.Start, timeOptions.End);
                for (var startTime = new DateTime(timeOptions.Start.Year, timeOptions.Start.Month, 1);
                    startTime < timeOptions.End;
                    startTime = startTime.AddMonths(1))
                    binsContainer.Bins.Add(new Bin { Start = startTime, End = startTime.AddMonths(1) });
                binsContainers.Add(binsContainer);
            }
            else
            {
                for (var startTime = new DateTime(timeOptions.Start.Year, timeOptions.Start.Month, 1);
                    startTime < timeOptions.End;
                    startTime = startTime.AddMonths(1))
                    binsContainers.Add(new BinsContainer(startTime, startTime.AddMonths(1))
                    {
                        Bins = GetDayBinsInRange(startTime, startTime.AddMonths(1), timeOptions)
                    });
            }
            return binsContainers;
        }


        public static List<BinsContainer> GetBinsForRange(
            TimeOptions timeOptions,
            int minutes)
        {
            var binsContainers = new List<BinsContainer>();
            var startTimeSpan = new TimeSpan();
            var endTimeSpan = new TimeSpan();
            var tempStart = timeOptions.Start;
            var tempEnd = timeOptions.End;
            if (timeOptions.TimeOfDayStartHour != null &&
                timeOptions.TimeOfDayStartMinute != null &&
                timeOptions.TimeOfDayEndHour != null &&
                timeOptions.TimeOfDayEndMinute != null)
            {
                startTimeSpan = new TimeSpan(0, timeOptions.TimeOfDayStartHour.Value,
                    timeOptions.TimeOfDayStartMinute.Value, 0);
                endTimeSpan = GetTimeOfDayEnd(timeOptions.TimeOfDayEndHour.Value,
                    timeOptions.TimeOfDayEndMinute.Value);
            }
            var binsContainer = new BinsContainer(timeOptions.Start, timeOptions.End);
            var daysToProcess = GetDaysToProcess(timeOptions.Start, timeOptions.End, timeOptions.DaysOfWeek);
            switch (timeOptions.TimeOption)
            {
                case TimeOptions.TimePeriodOptions.StartToEnd:
                    var startTime = tempStart;
                    while (startTime < tempEnd)
                    {
                        var endBin = startTime.AddMinutes(minutes);
                        binsContainer.Bins.Add(new Bin { Start = startTime, End = endBin });
                        startTime = endBin;
                    }
                    break;
                case TimeOptions.TimePeriodOptions.TimePeriod:
                    foreach (DateTime date in daysToProcess)
                    {
                        var start = date.Add(startTimeSpan);
                        var end = date.Add(endTimeSpan);
                        while (start < end)
                        {
                            var endBin = start.AddMinutes(minutes);
                            // Only whole bins between Start and End: a bin cut short would chart
                            // as a time of day of its own.
                            if (start >= timeOptions.Start && endBin <= timeOptions.End)
                                binsContainer.Bins.Add(new Bin { Start = start, End = endBin });
                            start = endBin;
                        }
                    }
                    break;
            }
            binsContainers.Add(binsContainer);
            return binsContainers;
        }

        // A time period that ends at 00:00 runs to the end of the day.
        public static TimeSpan GetTimeOfDayEnd(int hour, int minute)
        {
            var end = new TimeSpan(hour, minute, 0);
            return end == TimeSpan.Zero ? TimeSpan.FromDays(1) : end;
        }

        private static List<DateTime> GetDaysToProcess(DateTime startDate, DateTime endDate, List<DayOfWeek> daysOfWeek)
        {
            List<DateTime> datesToInclude = new List<DateTime>();

            // Each day that starts before the end is included, so an end of midnight leaves that day out
            // and an end later in the day keeps it.
            for (var date = startDate.Date; date < endDate; date = date.AddDays(1))
            {
                if (daysOfWeek == null || daysOfWeek.Contains(date.DayOfWeek))
                {
                    datesToInclude.Add(date);
                }
            }

            return datesToInclude;
        }
    }
}