using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Specifications
{
    public class ActiveSignalSpecification : BaseSpecification<Location>
    {
        public ActiveSignalSpecification() : base(s => s.VersionAction != LocationVersionActions.Delete) 
        {
            ApplyOrderByDescending(o => o.Start);
        }
    }

    public class SignalIdSpecification : BaseSpecification<Location>
    {
        public SignalIdSpecification(string signalId) : base(s => s.LocationIdentifier == signalId) { }
    }
}
