using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Specifications
{
    public class ActiveSignalSpecification : BaseSpecification<Signal>
    {
        public ActiveSignalSpecification() : base(s => s.VersionActionId != SignaVersionActions.Delete) 
        {
            ApplyOrderByDescending(o => o.Start);
        }
    }

    public class SignalIdSpecification : BaseSpecification<Signal>
    {
        public SignalIdSpecification(string signalId) : base(s => s.SignalId == signalId) { }
    }
}
