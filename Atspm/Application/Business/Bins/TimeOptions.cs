#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Bins/TimeOptions.cs
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

using System.ComponentModel;
using System.Globalization;

namespace Utah.Udot.Atspm.Business.Bins
{

    public class TimeOptions
    {
        [TypeConverter(typeof(EnumToStringUsingDescription))]
        public enum BinSize
        {
            [Description("Fifteen Minute")] FifteenMinute,
            [Description("Thirty Minute")] ThirtyMinute,
            Hour,
            Day,
            Month,
            Year
        }

        public enum TimePeriodOptions
        {
            StartToEnd,
            TimePeriod
        }


        public DateTime Start { get; set; }


        public DateTime End { get; set; }


        public int? TimeOfDayStartHour { get; set; }


        public int? TimeOfDayStartMinute { get; set; }


        public int? TimeOfDayEndHour { get; set; }


        public int? TimeOfDayEndMinute { get; set; }


        public List<DayOfWeek> DaysOfWeek { get; set; }


        public TimePeriodOptions TimeOption { get; set; }


        public BinSize SelectedBinSize { get; set; }

        /// <summary>
        /// Explains why these options give nothing to chart, or returns null when they are usable.
        /// </summary>
        public string GetValidationError()
        {
            if (End <= Start)
                return "End must be later than Start.";
            if (TimeOption == TimePeriodOptions.TimePeriod)
            {
                if (TimeOfDayStartHour == null || TimeOfDayStartMinute == null ||
                    TimeOfDayEndHour == null || TimeOfDayEndMinute == null)
                    return "A time period needs a start and end time of day.";
                if (!IsTimeOfDay(TimeOfDayStartHour.Value, TimeOfDayStartMinute.Value) ||
                    !IsTimeOfDay(TimeOfDayEndHour.Value, TimeOfDayEndMinute.Value))
                    return "Times of day must be between 00:00 and 23:59.";
                if (BinFactory.GetTimeOfDayEnd(TimeOfDayEndHour.Value, TimeOfDayEndMinute.Value) <=
                    new TimeSpan(TimeOfDayStartHour.Value, TimeOfDayStartMinute.Value, 0))
                    return "The end time of day must be later than the start time of day.";
            }
            if (BinFactory.GetBins(this).All(container => container.Bins.Count == 0))
            {
                if (TimeOption == TimePeriodOptions.TimePeriod && HasChosenDayInRange())
                    return "The chosen times of day do not fall between Start and End.";
                return TimeOption == TimePeriodOptions.TimePeriod || DaysOfWeek != null
                    ? "None of the chosen days fall in the date range."
                    : "No bins fall in the date range.";
            }
            return null;
        }

        // Each day that starts before End counts, as when the bins are made.
        private bool HasChosenDayInRange()
        {
            for (var date = Start.Date; date < End; date = date.AddDays(1))
                if (DaysOfWeek == null || DaysOfWeek.Contains(date.DayOfWeek))
                    return true;
            return false;
        }

        private static bool IsTimeOfDay(int hour, int minute) => hour >= 0 && hour < 24 && minute >= 0 && minute < 60;

        public List<DateTime> GetDateList()
        {
            if (DaysOfWeek != null)
                return GetDateList(Start, End, DaysOfWeek);
            var tempDateList = new List<DateTime>();
            for (var counterDate = Start; counterDate <= End; counterDate = counterDate.AddDays(1))
                tempDateList.Add(counterDate.Date);
            return tempDateList;
        }

        private static List<DateTime> GetDateList(DateTime startDate, DateTime endDate, List<DayOfWeek> daysOfWeek)
        {
            var dates = new List<DateTime>();

            for (var counterDate = startDate; counterDate <= endDate; counterDate = counterDate.AddDays(1))
                if (daysOfWeek.Contains(counterDate.DayOfWeek))
                    dates.Add(counterDate);

            return dates;
        }
    }

    public class EnumToStringUsingDescription : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType.Equals(typeof(Enum));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType.Equals(typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (!destinationType.Equals(typeof(string)))
                throw new ArgumentException("Can only convert to string.", "destinationType");

            if (!value.GetType().BaseType.Equals(typeof(Enum)))
                throw new ArgumentException("Can only convert an instance of enum.", "value");

            var name = value.ToString();
            var attrs =
                value.GetType().GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attrs.Length > 0 ? ((DescriptionAttribute)attrs[0]).Description : name;
        }
    }

    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var enumType = value.GetType();
            var field = enumType.GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute),
                false);
            return attributes.Length == 0
                ? value.ToString()
                : ((DescriptionAttribute)attributes[0]).Description;
        }
    }
}