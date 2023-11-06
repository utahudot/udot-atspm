using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.PreempDetail
{
    public class PreemptRequestAndServices : SignalResult
    {
        public List<RequestAndServices> RequestAndServices { get; set; }

        public PreemptRequestAndServices(string signalId,
            DateTime start,
            DateTime end) : base(signalId, start, end) { }
    }
}
