using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Utah.Udot.Atspm.Data.Utility
{
    /// <summary>
    /// Forces all nullable DateTimeOffset properties to UTC (+00:00) 
    /// while safely handling null database values.
    /// </summary>
    public class NullableDateTimeOffsetToUtcConverter : ValueConverter<DateTimeOffset?, DateTimeOffset?>
    {
        /// <summary>
        /// Forces all nullable DateTimeOffset properties to UTC (+00:00) 
        /// while safely handling null database values.
        /// </summary>
        public NullableDateTimeOffsetToUtcConverter()
            : base(
                csharp => csharp.HasValue ? csharp.Value.ToUniversalTime() : csharp,
                database => database.HasValue ? database.Value.ToUniversalTime() : database
            )
        { }
    }
}
