#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - %Namespace%/PhaseTerminationWorkflow.cs
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
    //HACK: clean this up
    public class PhaseCycle
    {
        public enum NextEventResponse
        {
            CycleOK,
            CycleMissingData,
            CycleComplete
        }

        public enum TerminationType
        {
            GapOut = 4,
            MaxOut = 5,
            ForceOff = 6,
            Unknown = 0
        }

        private double pedDuration;

        /// <summary>
        ///     Phase Objects primarily for the split monitor and terminaiton chart
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="phasenumber"></param>
        /// <param name="starttime"></param>
        public PhaseCycle(string locationId, int phasenumber, DateTime starttime)
        {
            locationId = locationId;
            PhaseNumber = phasenumber;
            StartTime = starttime;
            HasPed = false;
            TerminationEvent = 0;
        }

        public int PhaseNumber { get; }

        public string locationId { get; }

        public DateTime StartTime { get; }

        public DateTime EndTime { get; private set; }

        public DateTime PedStartTime { get; private set; }

        public DateTime PedEndTime { get; private set; }

        public int TerminationEvent { get; private set; }

        public TimeSpan Duration { get; private set; }

        public double PedDuration
        {
            get
            {
                if (pedDuration > 0)
                    return pedDuration;
                return 0;
            }
        }


        public bool HasPed { get; set; }

        public DateTime YellowEvent { get; set; }

        public void SetTerminationEvent(int terminatonCode)
        {
            TerminationEvent = terminatonCode;
        }

        public void SetEndTime(DateTime endtime)
        {
            EndTime = endtime;
            Duration = EndTime.Subtract(StartTime);
        }

        public void SetPedStart(DateTime starttime)
        {
            PedStartTime = starttime;
            HasPed = true;
        }

        public void SetPedEnd(DateTime endtime)
        {
            PedEndTime = endtime;
            pedDuration = PedEndTime.Subtract(PedStartTime).TotalSeconds;
        }
    }

    public abstract class PhaseTerminationValueBase
    {
        public DateTime StartTime { get; set; }
        public int PhaseNumber { get; set; }
    }

    public class GapOut : PhaseTerminationValueBase { }

    public class MaxOut : PhaseTerminationValueBase { }

    public class ForceOff : PhaseTerminationValueBase { }

    public class PedWalkBegin : PhaseTerminationValueBase { }

    public class UnknownTermination : PhaseTerminationValueBase { }

    public class PhaseTerminationResult
    {
        public string ChartName { get; internal set; }
        public string locationId { get; internal set; }
        public string LocationLocation { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public int ConsecutiveCount { get; internal set; }
        //public ICollection<Plan> Plans { get; internal set; }
        public ICollection<GapOut> GapOuts { get; internal set; }
        public ICollection<MaxOut> MaxOuts { get; internal set; }
        public ICollection<ForceOff> ForceOffs { get; internal set; }
        public ICollection<PedWalkBegin> PedWalkBegins { get; internal set; }
        public ICollection<UnknownTermination> UnknownTerminations { get; internal set; }
    }

    public class PhaseTerminationWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, IEnumerable<ApproachDelayResult>>
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
