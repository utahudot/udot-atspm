using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Configuration
{
    public class SignalControllerLoggerConfiguration
    {
        public int SaveToDatabaseBatchSize { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
        public int BulkCopyTimeout { get; set; }
    }
}
