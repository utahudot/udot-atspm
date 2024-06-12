#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Bins/BinFactory.cs
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.Bins
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
                //case TimeOptions.BinSize.Week:
                //    return GetBinsForRange(timeOptions, 60 * 24 * 7);
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
                startTime.Date < endDate.Date;
                startTime = startTime.AddDays(1))
                if (daysOfWeek.Contains(startTime.DayOfWeek))
                    bins.Add(new Bin
                    {
                        Start = startTime.AddHours(startHour).AddMinutes(startMinute),
                        End = startTime.AddHours(endHour).AddMinutes(endMinute)
                    });
            return bins;
        }

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
            for (var startTime = new DateTime(timeOptions.Start.Year, timeOptions.Start.Month, timeOptions.Start.Day, 0,
                    0, 0);
                startTime.Date < timeOptions.End.Date;
                startTime = startTime.AddDays(1))
                if (timeOptions.TimeOption == TimeOptions.TimePeriodOptions.StartToEnd && timeOptions.DaysOfWeek.Contains(startTime.DayOfWeek))
                {
                    binsContainer.Bins.Add(new Bin { Start = startTime, End = startTime.AddDays(1) });
                }
                else
                {
                    if (timeOptions.TimeOfDayStartHour != null && timeOptions.TimeOfDayStartMinute != null &&
                        timeOptions.TimeOfDayEndHour != null && timeOptions.TimeOfDayEndMinute != null)
                        if (timeOptions.DaysOfWeek.Contains(startTime.DayOfWeek))
                        {
                            binsContainer.Bins.Add(new Bin
                            {
                                Start = startTime.AddHours(timeOptions.TimeOfDayStartHour.Value)
                                    .AddMinutes(timeOptions.TimeOfDayStartMinute.Value),
                                End = startTime.AddHours(timeOptions.TimeOfDayEndHour.Value)
                                    .AddMinutes(timeOptions.TimeOfDayEndMinute.Value)
                            });
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

                for (var startTime = new DateTime(timeOptions.Start.Year, 1, 1);
                    startTime.Date < new DateTime(timeOptions.End.Year, 1, 1);
                    startTime = startTime.AddYears(1))
                    binsContainer.Bins.Add(new Bin { Start = startTime, End = startTime.AddYears(1) });
                binsContainers.Add(binsContainer);
            }
            else
            {
                for (var startTime = new DateTime(timeOptions.Start.Year, 1, 1);
                    startTime.Date < timeOptions.End.Date;
                    startTime = startTime.AddYears(1))
                    binsContainers.Add(new BinsContainer(startTime, startTime.AddYears(1))
                    {
                        Bins = GetDayBinsForRange(startTime, startTime.AddYears(1),
                            timeOptions.TimeOfDayStartHour.Value, timeOptions.TimeOfDayStartMinute.Value,
                            timeOptions.TimeOfDayEndHour.Value, timeOptions.TimeOfDayEndMinute.Value,
                            timeOptions.DaysOfWeek)
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
                    startTime.Date < timeOptions.End.Date;
                    startTime = startTime.AddMonths(1))
                    binsContainer.Bins.Add(new Bin { Start = startTime, End = startTime.AddMonths(1) });
                binsContainers.Add(binsContainer);
            }
            else
            {
                for (var startTime = new DateTime(timeOptions.Start.Year, timeOptions.Start.Month, 1);
                    startTime.Date < timeOptions.End.Date;
                    startTime = startTime.AddMonths(1))
                    binsContainers.Add(new BinsContainer(startTime, startTime.AddMonths(1))
                    {
                        Bins = GetDayBinsForRange(startTime, startTime.AddMonths(1),
                            timeOptions.TimeOfDayStartHour.Value, timeOptions.TimeOfDayStartMinute.Value,
                            timeOptions.TimeOfDayEndHour.Value, timeOptions.TimeOfDayEndMinute.Value,
                            timeOptions.DaysOfWeek)
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
                endTimeSpan = new TimeSpan(0, timeOptions.TimeOfDayEndHour.Value,
                    timeOptions.TimeOfDayEndMinute.Value, 0);
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
                            binsContainer.Bins.Add(new Bin { Start = start, End = endBin });
                            start = endBin;
                        }
                    }
                    break;
            }
            //for (var startTime = tempStart; startTime < tempEnd; startTime = startTime.AddMinutes(minutes))
            //    switch (timeOptions.TimeOption)
            //    {
            //        case TimeOptions.TimePeriodOptions.StartToEnd:
            //            binsContainer.Bins.Add(new Bin { Start = startTime, End = startTime.AddMinutes(minutes) });
            //            break;
            //        case TimeOptions.TimePeriodOptions.TimePeriod:
            //            var periodStartTimeSpan = new TimeSpan(0, startTime.Hour,
            //                startTime.Minute, 0);
            //            if (timeOptions.DaysOfWeek.Contains(startTime.DayOfWeek)
            //                && periodStartTimeSpan >= startTimeSpan
            //                && periodStartTimeSpan < endTimeSpan)
            //                binsContainer.Bins.Add(new Bin { Start = startTime, End = startTime.AddMinutes(minutes) });
            //            break;
            //    }
            binsContainers.Add(binsContainer);
            return binsContainers;
        }

        private static List<DateTime> GetDaysToProcess(DateTime startDate, DateTime endDate, List<DayOfWeek> daysOfWeek)
        {
            List<DateTime> datesToInclude = new List<DateTime>();
            var days = endDate.DayOfYear - startDate.DayOfYear;

            for (int i = 0; i <= days; i++)
            {
                var date = startDate.AddDays(i);
                if (daysOfWeek.Contains(date.DayOfWeek))
                {
                    datesToInclude.Add(date);
                }
            }

            return datesToInclude;
        }
    }
}