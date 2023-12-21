using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Specifications
{
    public class ActiveLocationSpecification : BaseSpecification<Location>
    {
        public ActiveLocationSpecification() : base(s => s.VersionAction != LocationVersionActions.Delete) 
        {
            ApplyOrderByDescending(o => o.Start);
        }
    }

    public class LocationIdSpecification : BaseSpecification<Location>
    {
        public LocationIdSpecification(string locationId) : base(s => s.LocationIdentifier == locationId) { }
    }
}
