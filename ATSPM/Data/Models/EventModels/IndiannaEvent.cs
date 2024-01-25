using ATSPM.Data.Enums;

#nullable disable

namespace ATSPM.Data.Models.EventModels
{
    /// <summary>
    /// Indiana Traffic Signal Hi Resolution Data Logger Enumerations
    /// <seealso cref="https://docs.lib.purdue.edu/jtrpdata/4/"/>
    /// </summary>
    public class IndiannaEvent : AtspmEventModelBase
    {
        /// <summary>
        /// Event code from <see cref="DataLoggerEnum"/>
        /// </summary>
        public DataLoggerEnum EventCode { get; set; }
        
        /// <summary>
        /// Event parameter that is specific to <see cref="EventCode"/>
        /// </summary>
        public byte EventParam { get; set; }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{EventCode}-{EventParam}";
        }
    }
}
