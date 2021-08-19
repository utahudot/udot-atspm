using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Domain.Specifications
{
    /// <summary>
    /// Generic Specification class for making adhoc specifications
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericSpecification<T> : BaseSpecification<T>
    {
        /// <summary>
        /// Pass critera function
        /// </summary>
        /// <param name="criteria">Specification Criteria</param>
        public GenericSpecification(Expression<Func<T, bool>> criteria) : base(criteria) { }
    }
}
