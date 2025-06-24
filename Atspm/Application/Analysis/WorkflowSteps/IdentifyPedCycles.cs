#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/CalculateTotalVolumes.cs
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

using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public class IdentifyPedCycles : TransformManyProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, PedCycle>
    {
        public IdentifyPedCycles(ExecutionDataflowBlockOptions dataflowBlockOptions) : base(dataflowBlockOptions)
        {
        }

        protected override Task<IEnumerable<PedCycle>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var events = input.Item2
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()

                .Where(w =>
                w.EventCode == (int)IndianaEnumerations.PedestrianBeginWalk ||
                w.EventCode == (int)IndianaEnumerations.PedestrianBeginChangeInterval)

                .ToList()
                .AsEnumerable();

            var approaches = location.Approaches.Select(s => Tuple.Create(s, events));

            var approachcount = approaches.Count();

            foreach (var a in approaches)
            {
                var phases = a.Item2.GroupBy(g => g.EventParam, (phase, i) => Tuple.Create(input.Item1, Convert.ToInt32(phase), i.Where(w => w.EventParam == phase)));

                var phasecount = phases.Count();

                foreach (var p in phases)
                {
                    //int cycle;

                    //var cycles = new List<PedCycle>();

                    var test = p.Item3.Select((s, i) =>
                    {
                        if (s.EventCode == 21)
                        {
                            //cycles.Add(new PedCycle() { BeginWalk = s.Timestamp });
                            return new PedCycle() { BeginWalk = s.Timestamp };
                        }
                        return null;

                    });

                    //yield return Task.FromResult(test);
                }
            }

            return Task.FromResult(new List<PedCycle>().AsEnumerable());
        }
    }

    public class  PedCycle : StartEndRange
    {
        public int Cycle { get; set; }
        public DateTime BeginWalk { get; set; }
        public DateTime BeginChangeInterval { get; set; }
        public DateTime CallRegistered { get; set; }
        public DateTime PedDetected { get; set; }

        public bool Detected => PedDetected != DateTime.MinValue;
        public double PedDelay => PedDetected > BeginWalk ? 0 : Math.Abs((BeginWalk - PedDetected).TotalSeconds);

        public override string ToString()
        {
            //return $"Cycle: {Cycle}, Start: {Start}, BeginWalk: {BeginWalk}, BeginChangeInterval: {BeginChangeInterval}, CallRegistered: {CallRegistered}, PedDetected: {PedDetected}, PedDelay: {PedDelay}, End: {End}";
            return $"BeginWalk: {BeginWalk}, PedDetected: {PedDetected}, PedDelay: {PedDelay}";
        }
    }

    /// <summary>
    /// Breaks out all <see cref="Approach"/> from <see cref="Location"/>
    /// and returns separate Tuples of <see cref="Approach"/>/<see cref="IEnumerable{IndianaEvent}"/> pairs
    /// sorted by <see cref="ITimestamp.Timestamp"/>.
    /// </summary>
    public class GroupLocationsByApproaches1 : TransformManyProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, Tuple<Approach, IEnumerable<IndianaEvent>>>
    {
        /// <inheritdoc/>
        public GroupLocationsByApproaches1(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Approach, IEnumerable<IndianaEvent>>>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var events = input.Item2
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
                .ToList()
                .AsEnumerable();

            var result = location.Approaches.Select(s => Tuple.Create(s, events));

            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// Breaks out all the Phase numbers from <see cref="Approach"/>
    /// and returns separate Tuples of <see cref="Approach"/>/<see cref="Approach.ProtectedPhaseNumber"/>/<see cref="IEnumerable{IndianaEvent}"/> sets
    /// where the <see cref="IndianaEvent.EventCode"/> is equal to one of the following:
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PhaseGapOut"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseMaxOut"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseForceOff"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseGreenTermination"/></item>
    /// </list>
    /// and the <see cref="IndianaEvent.EventParam"/> is equal to <see cref="Approach.ProtectedPhaseNumber"/>
    /// </summary>
    public class GroupPhaseTerminationsByApproaches2 : TransformManyProcessStepBase<Tuple<Approach, IEnumerable<IndianaEvent>>, Tuple<Approach, int, IEnumerable<IndianaEvent>>>
    {
        /// <inheritdoc/>
        public GroupPhaseTerminationsByApproaches2(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Approach, int, IEnumerable<IndianaEvent>>>> Process(Tuple<Approach, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var approach = input.Item1;
            var events = input.Item2
                .Where(w => w.EventCode >= (int)IndianaEnumerations.PhaseGapOut && w.EventCode <= (int)IndianaEnumerations.PhaseGreenTermination)
                .FromSpecification(new EventLogSpecification(approach?.Location))
                .Cast<IndianaEvent>()
                .ToList();

            var result = input.Item2.GroupBy(g => g.EventParam, (phase, i) => Tuple.Create(input.Item1, Convert.ToInt32(phase), i.Where(w => w.EventParam == phase)));

            return Task.FromResult(result);
        }
    }
}
