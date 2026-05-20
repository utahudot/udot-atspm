using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Utah.Udot.Atspm.Data.Utility
{
    /// <summary>
    /// Forces all non-nullable DateTimeOffset properties to UTC (+00:00) 
    /// when sending to or retrieving from the database.
    /// </summary>
    public class DateTimeOffsetToUtcConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
    {
        /// <summary>
        /// Forces all non-nullable DateTimeOffset properties to UTC (+00:00) 
        /// when sending to or retrieving from the database.
        /// </summary>
        public DateTimeOffsetToUtcConverter()
            : base(
                csharp => csharp.ToUniversalTime(),
                database => database.ToUniversalTime()
            )
        { }
    }
}
