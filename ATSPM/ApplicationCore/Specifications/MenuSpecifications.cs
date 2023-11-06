using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Specifications
{
    public class MenuTopLevelSpecification : BaseSpecification<MenuItem>
    {
        public MenuTopLevelSpecification(string application) : base(s => s.ParentId == 0) 
        {
            ApplyOrderBy(o => o.DisplayOrder);
        }
    }
}
