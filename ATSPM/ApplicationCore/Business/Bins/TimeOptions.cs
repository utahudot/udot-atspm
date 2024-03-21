using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace ATSPM.Application.Business.Bins
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


        public TimeOptions(DateTime start, DateTime end, int? timeOfDayStartHour, int? timeOfDayStartMinute,
            int? timeOfDayEndHour, int? timeOfDayEndMinute, List<DayOfWeek> daysOfWeek, BinSize binSize,
            TimePeriodOptions timeOption)
        {
            DaysOfWeek = new List<DayOfWeek>();
            Start = start;
            End = end;
            TimeOfDayStartHour = timeOfDayStartHour;
            TimeOfDayStartMinute = timeOfDayStartMinute;
            TimeOfDayEndHour = timeOfDayEndHour;
            TimeOfDayEndMinute = timeOfDayEndMinute;
            int dayOfWeekCounter = 0;
            DateTime oneDay = Start;
            DaysOfWeek = daysOfWeek;
            SelectedBinSize = binSize;
            TimeOption = timeOption;
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

        public List<DateTime> GetDateList()
        {
            if (DaysOfWeek != null)
                return GetDateList(Start, End, DaysOfWeek);
            var tempDateList = new List<DateTime>();
            for (var counterDate = Start; counterDate <= End; counterDate = counterDate.AddDays(1))
                tempDateList.Add(counterDate.Date);
            return tempDateList;
        }

        private List<DateTime> GetDateList(DateTime startDate, DateTime endDate, List<DayOfWeek> daysOfWeek)
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

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return base.ConvertFrom(context, culture, value);
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