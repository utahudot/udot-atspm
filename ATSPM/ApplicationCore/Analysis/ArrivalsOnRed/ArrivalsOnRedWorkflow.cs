using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.ArrivalsOnRed
{
    public class ArrivalsOnRedWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, IEnumerable<ApproachDelayResult>>
    {
        public override void AddStepsToTracker()
        {
            throw new NotImplementedException();
        }

        public override void InstantiateSteps()
        {
            throw new NotImplementedException();
        }

        public override void LinkSteps()
        {
            throw new NotImplementedException();
        }
    }
}
