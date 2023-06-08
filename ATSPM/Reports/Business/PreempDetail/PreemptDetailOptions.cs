using AutoFixture.Kernel;
using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PreempDetail
{
    public class PreemptDetailOptions
    {
        public string SignalId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}