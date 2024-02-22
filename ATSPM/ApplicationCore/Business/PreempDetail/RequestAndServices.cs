using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.PreempDetail
{
    public class RequestAndServices
    {
        public RequestAndServices() { }

        public int PreemptionNumber { get; set; }
        public List<string> Requests { get; set; }
        public List<string> Services { get; set; }
    }
}
