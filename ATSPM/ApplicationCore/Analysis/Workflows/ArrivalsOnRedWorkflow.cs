using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.Workflows
{
    public class ArrivalsOnRedWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, IEnumerable<ApproachDelayResult>>
    {
        protected override void AddStepsToTracker()
        {
            throw new NotImplementedException();
        }

        protected override void InstantiateSteps()
        {
            throw new NotImplementedException();
        }

        protected override void LinkSteps()
        {
            throw new NotImplementedException();
        }
    }
}
