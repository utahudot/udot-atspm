using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Configuration
{
    public class LocationControllerLoggerConfiguration
    {
        public int SaveToDatabaseBatchSize { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
        public int BulkCopyTimeout { get; set; }
    }
}
