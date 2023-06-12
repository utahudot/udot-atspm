using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.Plans
{
    public class PreemptPlan : Plan
    {
        public int PreemptCount { get; set; }



        #region IPlan

        /// <inheritdoc/>
        public override void AssignToPlan<T>(IEnumerable<T> range)
        {
        }

        #endregion
    }
}
