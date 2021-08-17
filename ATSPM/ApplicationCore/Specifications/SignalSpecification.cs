using ATSPM.Application.Models;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ATSPM.Application.Specifications
{
    public class SignalSpecification : BaseSpecification<IQueryable<Signal>>
    {
        public SignalSpecification(Expression<Func<IQueryable<Signal>, bool>> criteria) : base (criteria)
        {

        }

        public Signal Signal { get; set; }


    }
}
