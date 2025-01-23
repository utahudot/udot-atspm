using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Options pattern model for services that implement <see cref="IEventLogImporter"/>
    /// </summary>
    public class EventLogImporterConfiguration
    {
        /// <summary>
        /// Earliest acceptable date for importing from source
        /// </summary>
        public DateTime EarliestAcceptableDate { get; set; } = DateTime.Parse("01/01/1980");

        /// <summary>
        /// Flag for deleting source after importing
        /// </summary>
        public bool DeleteSource { get; set; }

        public override string ToString()
        {
            return $"{EarliestAcceptableDate} - {DeleteSource}";
        }
    }
}
