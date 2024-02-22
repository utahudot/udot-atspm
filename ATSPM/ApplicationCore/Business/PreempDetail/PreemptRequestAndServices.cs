using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PreempDetail
{
    public class PreemptRequestAndServices : LocationResult
    {
        public List<RequestAndServices> RequestAndServices { get; set; }

        public PreemptRequestAndServices(string locationId,
            DateTime start,
            DateTime end) : base(locationId, start, end) { }
    }
}
