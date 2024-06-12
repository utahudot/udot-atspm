#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - %Namespace%/ArrivalsOnRedWorkflow.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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
