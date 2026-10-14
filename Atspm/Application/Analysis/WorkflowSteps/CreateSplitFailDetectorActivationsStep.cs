#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/CreateSplitFailDetectorActivationsStep.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Evaluates detector activations, adjusts timestamps for distance/latency, and merges overlapping presence intervals.
    /// </summary>
    public class CreateSplitFailDetectorActivationsStep : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>>, Tuple<Location, Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>>>
    {
        /// <summary>
        /// Initializes a new instance of the CreateSplitFailDetectorActivationsStep class with dataflow options.
        /// </summary>
        public CreateSplitFailDetectorActivationsStep(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<Tuple<Location, Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>>> Process(Tuple<Location, IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents, phaseCycles) = input;
            var locationEvents = rawEvents.ToList();

            var phasePresence = new Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>();

            if (!locationEvents.Any())
            {
                foreach (var phaseDetail in phaseCycles.Keys)
                {
                    phasePresence.Add(phaseDetail, new List<Tuple<DateTime, DateTime>>());
                }
                return Task.FromResult(Tuple.Create(location, phaseCycles, phasePresence));
            }

            var minTime = locationEvents.Min(e => e.Timestamp);
            var maxTime = locationEvents.Max(e => e.Timestamp);

            foreach (var phaseDetail in phaseCycles.Keys)
            {
                var stopbarDetectors = phaseDetail.Approach.GetAllDetectorsOfDetectionType(DetectionTypes.SBP);

                if (stopbarDetectors == null || !stopbarDetectors.Any())
                {
                    phasePresence.Add(phaseDetail, new List<Tuple<DateTime, DateTime>>());
                    continue;
                }

                var rawIntervals = new List<Tuple<DateTime, DateTime>>();

                foreach (var detector in stopbarDetectors)
                {
                    var channelEvents = locationEvents
                        .Where(e => e.EventParam == detector.DetectorChannel)
                        .Select(e => new IndianaEvent
                        {
                            LocationIdentifier = e.LocationIdentifier,
                            EventCode = e.EventCode,
                            EventParam = e.EventParam,
                            Timestamp = AtspmMath.AdjustTimeStamp(e.Timestamp, phaseDetail.Approach.Mph ?? 0, detector.DistanceFromStopBar ?? 0, detector.LatencyCorrection)
                        });

                    rawIntervals.AddRange(channelEvents.GetPresenceIntervals(minTime, maxTime));
                }

                phasePresence.Add(phaseDetail, rawIntervals.MergeOverlappingIntervals());
            }

            return Task.FromResult(Tuple.Create(location, phaseCycles, phasePresence));
        }
    }
}
