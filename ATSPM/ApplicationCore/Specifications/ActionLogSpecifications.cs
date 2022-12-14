using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Specifications
{
    public class ActionLogDateRangeSpecification : BaseSpecification<ActionLog>
    {
        public ActionLogDateRangeSpecification(DateTime startDate, DateTime endDate) : base(s => s.Date >= startDate && s.Date <= endDate) { }
    }
}
